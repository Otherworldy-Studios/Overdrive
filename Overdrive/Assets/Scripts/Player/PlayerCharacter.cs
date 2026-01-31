using KinematicCharacterController;
using UnityEngine;

public class PlayerCharacter : MonoBehaviour , ICharacterController
{
    [SerializeField] private KinematicCharacterMotor motor;
    [SerializeField] private Transform cameraTarget;

    [Space]
    [SerializeField] private float walkSpeed = 20f;
    [SerializeField] private float jumpSpeed = 20f;
    [SerializeField] private float gravity = -90f;
   
    private Quaternion requestedRotation;
    private Vector3 requestedMovement;
    private bool requestedJump;
    public void Initialize()
    {
        motor.CharacterController = this;
    }
    
    public Transform GetCameraTarget() => cameraTarget;
    
    public void UpdateInput(CharacterInput input)
    {
        requestedRotation = input.Rotation;
        requestedMovement = new Vector3(input.Move.x, 0,input.Move.y);
        requestedMovement.Normalize();
        requestedMovement = input.Rotation * requestedMovement;
        
        //become true on jump pressed and stay true if jump was pressed since update input is called multiple
        //times until next physics tick so this keeps it in the queue(jump buffering)
        requestedJump = requestedJump || input.Jump;
    }
    
    public void UpdateVelocity(ref Vector3 currentVelocity, float deltaTime)
    {
        if (motor.GroundingStatus.IsStableOnGround)
        {
            //snap to movement dir of angle that player is walking on
            Vector3 groundedMovement = motor.GetDirectionTangentToSurface(requestedMovement, motor.GroundingStatus.GroundNormal) * requestedMovement.magnitude;
            currentVelocity = groundedMovement * walkSpeed;
        }
        else
        {
            currentVelocity += motor.CharacterUp * gravity * deltaTime;
        }

        if (requestedJump && motor.GroundingStatus.IsStableOnGround)
        {
            requestedJump = false;
            motor.ForceUnground(0f);
            
            //set minimum vertical speed to jump speed, so if pressing jump while falling you dont just fall slower
            float currentVerticalSpeed = Vector3.Dot(currentVelocity, motor.CharacterUp);
            float targetVerticalSpeed = Mathf.Max(currentVerticalSpeed, jumpSpeed);
            currentVelocity += motor.CharacterUp * (targetVerticalSpeed - currentVerticalSpeed);
        }
        
    }
   
    public void UpdateRotation(ref Quaternion currentRotation, float deltaTime)
    { 
       Vector3 forward = Vector3.ProjectOnPlane(requestedRotation * Vector3.forward, motor.CharacterUp);
       currentRotation = Quaternion.LookRotation(forward, motor.CharacterUp);
    }

    public void BeforeCharacterUpdate(float deltaTime)
    {
       
    }

    public void PostGroundingUpdate(float deltaTime)
    {
        
    }

    public void AfterCharacterUpdate(float deltaTime)
    {
        
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
}

public struct CharacterInput
{
    public Quaternion Rotation;
    public Vector2 Move;
    public bool Jump;
}