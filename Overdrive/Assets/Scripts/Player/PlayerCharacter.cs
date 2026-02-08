using KinematicCharacterController;
using UnityEngine;

public class PlayerCharacter : MonoBehaviour , ICharacterController
{
  
    [SerializeField] private KinematicCharacterMotor motor;
    [SerializeField] private Transform cameraTarget;

    [Space]
    [SerializeField] private float walkSpeed = 20f;
    [SerializeField] private float walkResponse = 25f; //how fast you reach max

    private float crouchSpeed = 20f;
    private float crouchResponse = 25f;
    
    
   
    [SerializeField] private float jumpSpeed = 20f;
    
    [Range(0f, 1f)]
    [Header("Lower gravity by multiplier while jump is held ")]
    [SerializeField] private float jumpSustainGravity = 0.4f;
    [SerializeField] private float gravity = -90f;

    [SerializeField] private float slideStartSpeed = 25f;
    [SerializeField] private float slideEndSpeed = 15f; //if slide speed goes lower than this character will move to stand
    [SerializeField] private float slideFriction = 0.8f; //rate that the player loses their slide speed
    [SerializeField] private float slideSteerAcceleration = 5f;
    [SerializeField] private float slideGravity = -90f;

    [Space] [SerializeField] private float airSpeed = 15f; //max speed
    [Space] [SerializeField] private float airAcceleration = 70f; //how fast speed will change
    [SerializeField] private float coyoteTime = 0.2f; //grace period for jumps after leaving ground
    

    private CharacterState state;
    private CharacterState lastState;
    private CharacterState tempState;
   
    private Quaternion requestedRotation;
    private Vector3 requestedMovement;
    private bool requestedJump;
    private bool requestedSustainedJump;
   // private bool requestedSlide;
    private bool requestedCrouch;
    private bool requestedCrouchInAir;

    private float timeSinceUngrounded;
    private float timeSinceLastJumpRequest;
    private bool ungroundedDueToJump;
    public void Initialize()
    {
        state.Stance = Stance.Stand;
        lastState = state;
        motor.CharacterController = this;
    }
    
    public Transform GetCameraTarget() => cameraTarget;
    
    public void UpdateInput(CharacterInput input)
    {
        requestedRotation = input.Rotation;
        requestedMovement = new Vector3(input.Move.x, 0f,input.Move.y);
        requestedMovement = Vector3.ClampMagnitude(requestedMovement, 1f);
        requestedMovement = input.Rotation * requestedMovement;
        //become true on jump pressed and stay true if jump was pressed since update input is called multiple
        //times until next physics tick so this keeps it in the queue(jump buffering)
        bool wasRequestingJump = requestedJump;
        requestedJump = requestedJump || input.Jump;

        if (requestedJump && !wasRequestingJump)
        {
            timeSinceLastJumpRequest = 0f;
        }
        requestedSustainedJump = input.JumpSustain;
       
        bool wasRequestingCrouch = requestedCrouch;
        requestedCrouch = input.Crouch switch
        {
            CrouchInput.Toggle => true,
            CrouchInput.None => false,
            _=> requestedCrouch
        };
        if (requestedCrouch && !wasRequestingCrouch)
        {
            requestedCrouchInAir = !state.Grounded;
        }
        else if (!requestedCrouch && wasRequestingCrouch)
        {
            requestedCrouchInAir = false;
        }
    }
    
    public void UpdateVelocity(ref Vector3 currentVelocity, float deltaTime)
    {
       
        if (motor.GroundingStatus.IsStableOnGround)
        {
            timeSinceUngrounded = 0f;
            ungroundedDueToJump = false;
            //snap to movement dir of angle that player is walking on
            Vector3 groundedMovement = motor.GetDirectionTangentToSurface
            (
                requestedMovement,
                motor.GroundingStatus.GroundNormal
            ) * requestedMovement.magnitude;

           
            
            //Start Sliding
            {
                bool moving = groundedMovement.sqrMagnitude > 0f;
                bool crouching = state.Stance is Stance.Crouch;
                bool wasStanding =  lastState.Stance is Stance.Stand;
                bool wasInAir = !lastState.Grounded;
                if (moving && crouching && (wasStanding || wasInAir))
                {
                    state.Stance = Stance.Slide;
                    
                    //when landin on stable ground character motor projects the velocity onto a flat ground plane
                    //See: KinematicCharacterMotor.HandleVelocityProjection()
                    //This is normally food, because under  normal circumstances the player shouldn't slide when landing on the ground
                    //in this case since we want the player to slide we can reproject the last frames (falling) velocity onto the ground normal to slide
                    if (wasInAir)
                    {
                        currentVelocity = Vector3.ProjectOnPlane
                        (
                            lastState.Velocity,
                            motor.GroundingStatus.GroundNormal
                        );
                    }

                    float effectiveSlideStartSpeed = slideStartSpeed;
                    if (!lastState.Grounded && !requestedCrouchInAir)
                    {
                        effectiveSlideStartSpeed = 0f;
                        requestedCrouchInAir = false;
                    }
                    float slideSpeed = Mathf.Max(slideStartSpeed, currentVelocity.magnitude); //if player is moving faster than slideStartSpeed then preserve previous velocity
                    currentVelocity = motor.GetDirectionTangentToSurface(direction: currentVelocity, surfaceNormal: motor.GroundingStatus.GroundNormal) * slideSpeed;
                   
                }

               
            }
            
            
            if(state.Stance is Stance.Stand or  Stance.Crouch)
            {
                float speed = state.Stance is Stance.Stand ? walkSpeed : crouchSpeed;
                float response = state.Stance is Stance.Stand ? walkResponse : crouchResponse;
                Vector3 targetVelocity = groundedMovement * speed;
                currentVelocity = Vector3.Lerp(currentVelocity, targetVelocity, 
                    1f - Mathf.Exp(-response * deltaTime));
            }
            else
            {
                //Friction
                currentVelocity -= currentVelocity * (slideFriction * deltaTime);
                
                //Steer
                //target velocity is the player's movement direction at the current speed
                float currentSpeed = currentVelocity.magnitude; //speed before steer force is applied
                Vector3 targetVelocity = groundedMovement * currentSpeed;
                Vector3 steerForce = (targetVelocity - currentVelocity) * slideSteerAcceleration * deltaTime;
                
                //slope 
                Vector3 force = Vector3.ProjectOnPlane(-motor.CharacterUp, motor.GroundingStatus.GroundNormal) * slideGravity;
                currentVelocity -= force * deltaTime;
                
                //add steer force but clamp speed as to not accelerate because of movement input
                currentVelocity += steerForce;
                currentVelocity = Vector3.ClampMagnitude(currentVelocity, currentSpeed);
                
                //change back to walking if sliding too slowly
                if (currentVelocity.magnitude < slideEndSpeed)
                {
                    state.Stance = Stance.Crouch;
                }
            }
            Debug.Log(currentVelocity.magnitude + "  " + state.Stance + "   " + requestedCrouch);
        }
        else //in the air
        {
            timeSinceUngrounded += deltaTime;
            //In air move
            if (requestedMovement.sqrMagnitude > 0.0001f)
            {
                //requested movement projected onto movement plane (magnitude preserved)
                Vector3 planarMovement = Vector3.ProjectOnPlane(requestedMovement, motor.CharacterUp) * requestedMovement.magnitude;
                planarMovement = Vector3.ClampMagnitude(planarMovement, 1f);
                
                //current velocity on movement plane 
                Vector3 currentPlanarVelocity = Vector3.ProjectOnPlane(currentVelocity, motor.CharacterUp);
                
                //calculate movement force //changed depending on current velocity
                Vector3 movementForce = planarMovement * airAcceleration * deltaTime;
                
                //if moving slower than the max air speed, movement force is for steering
                if (currentPlanarVelocity.magnitude < airSpeed)
                {
                    Vector3 targetPlanarVelocity = currentPlanarVelocity + movementForce;
                
                    //limit target velocity to airSpeed
                    targetPlanarVelocity = Vector3.ClampMagnitude(targetPlanarVelocity, airSpeed);
                    
                    //steer towards target velocity
                    movementForce = targetPlanarVelocity - currentPlanarVelocity;
                }
                //else nerf the movement force when it is in the direction of the current planar velocity
                //to prevent the player from accelerating further past max air speed
                else if (Vector3.Dot(currentPlanarVelocity, movementForce) > 0f)
                {
                    Vector3 constrainedMovementForce = Vector3.ProjectOnPlane(movementForce, currentPlanarVelocity.normalized);
                    movementForce = constrainedMovementForce;
                }
                
              //prevent air climbing steep slopes //found any ground is also true for any steep ground
              if (motor.GroundingStatus.FoundAnyGround)
              {
                  //if moving in direction as the resulting velocity(direction they are already moving in)
                  if (Vector3.Dot(movementForce, currentVelocity + movementForce) > 0f)
                  {
                      Vector3 perpendicularObstructionNormal = Vector3.Cross
                          (
                              motor.CharacterUp,
                              Vector3.Cross
                              (
                                  motor.CharacterUp, 
                                  motor.GroundingStatus.GroundNormal
                              )
                          ).normalized;
                      //project movement force onto obstruction plane
                      movementForce = Vector3.ProjectOnPlane(movementForce, perpendicularObstructionNormal);
                  }
              }
               
                
                //steer towards curremt velocity.
                currentVelocity += movementForce;
                
            }
            float effectiveGravity = gravity;
            var verticalSpeed = Vector3.Dot(currentVelocity, motor.CharacterUp);
            if (requestedSustainedJump && verticalSpeed > 0f)
            {
                //apply gravity multiplier
                effectiveGravity *= jumpSustainGravity;
            }
            
            currentVelocity += motor.CharacterUp * effectiveGravity * deltaTime;
        }

        if (requestedJump)
        {
            bool grounded = motor.GroundingStatus.IsStableOnGround;
            bool canCoyoteJump = timeSinceUngrounded < coyoteTime && !ungroundedDueToJump;
            if (grounded || canCoyoteJump)
            {
                requestedJump = false;
                requestedCrouch = false; //TODO might change this later right now if the player jumps while sliding they stop sliding
                requestedCrouchInAir = false;
                motor.ForceUnground(0.1f);
                ungroundedDueToJump = true;
            
                //set minimum vertical speed to jump speed, so if pressing jump while falling you dont just fall slower
                float currentVerticalSpeed = Vector3.Dot(currentVelocity, motor.CharacterUp);
                float targetVerticalSpeed = Mathf.Max(currentVerticalSpeed, jumpSpeed);
                currentVelocity += motor.CharacterUp * (targetVerticalSpeed - currentVerticalSpeed);
            }
            else
            {
                //queue jump // will defer jump until coyote time has passed
                timeSinceLastJumpRequest += deltaTime;
                bool canJumpLater = timeSinceLastJumpRequest < coyoteTime;
                
                requestedJump = canJumpLater;
            }
           
        }
        
    }
   
    public void UpdateRotation(ref Quaternion currentRotation, float deltaTime)
    { 
       Vector3 forward = Vector3.ProjectOnPlane(requestedRotation * Vector3.forward, motor.CharacterUp);
       if (forward != Vector3.zero)
       {
           currentRotation = Quaternion.LookRotation(forward, motor.CharacterUp);
       }
    }

    public void BeforeCharacterUpdate(float deltaTime)
    {
        tempState = state;
        if (requestedCrouch && state.Stance is Stance.Stand)
        {
            state.Stance = Stance.Crouch;
            //if going to implement crouch change dimensions would be here for now "Crouch" is just a walk
        }
       
        
    }

    public void PostGroundingUpdate(float deltaTime)
    {
         // if (!motor.GroundingStatus.IsStableOnGround && state.Stance is Stance.Slide)
         // {
         //     state.Stance = Stance.Stand;
         // } //TODO disallows sliding if you get launched off while sliding needs testing
    }

    public void AfterCharacterUpdate(float deltaTime)
    {
        if (!requestedCrouch && state.Stance is not Stance.Stand)
        {
            state.Stance = Stance.Stand;
            //if going to implement crouch change dimensions would be here for now "Crouch" is just a walk
        }
        state.Grounded = motor.GroundingStatus.IsStableOnGround;
        state.Velocity = motor.Velocity;
        lastState = tempState;
    }

    public bool IsColliderValidForCollisions(Collider coll)
    {
        return true;
    }

    public void OnGroundHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, ref HitStabilityReport hitStabilityReport)
    {
       
    }

    public void OnMovementHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint,
        ref HitStabilityReport hitStabilityReport)
    {
       
    }

    public void ProcessHitStabilityReport(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, Vector3 atCharacterPosition,
        Quaternion atCharacterRotation, ref HitStabilityReport hitStabilityReport)
    {
       
    }

    public void OnDiscreteCollisionDetected(Collider hitCollider)
    {
        
    }
    
    public void SetPosition(Vector3 position, bool killVelocity = true)
    {
        motor.SetPosition(position);
        if (killVelocity)
        {
            motor.BaseVelocity = Vector3.zero;
        }
    }
    
}
public enum CrouchInput
{
    None, Toggle
}

public struct CharacterInput
{
    public Quaternion Rotation;
    public Vector2 Move;
    public bool Jump;
    public bool JumpSustain;
    public CrouchInput Crouch;
}

public enum Stance
{
    Stand,
    Crouch,
    Slide
}

public struct CharacterState
{
    public bool Grounded;
    public Stance Stance;
    public Vector3 Velocity;
}