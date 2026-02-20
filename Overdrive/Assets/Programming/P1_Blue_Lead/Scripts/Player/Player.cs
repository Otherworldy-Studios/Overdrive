using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;

public class Player : MonoBehaviour
{
   
    [SerializeField]private PlayerCharacter playerCharacter;
    [SerializeField] private PlayerCamera playerCamera;
    [Space]
    [SerializeField] private CameraSpring cameraSpring;
    [SerializeField] private CameraLean cameraLean;
    [SerializeField] private Volume volume;
    [SerializeField] private StanceVignette stanceVignette;
    private InputSystem_Actions inputActions;
    void Start()
    {
        inputActions = new InputSystem_Actions();
        inputActions.Enable();
        
        playerCharacter.Initialize();
        playerCamera.Initialize(playerCharacter.GetCameraTarget());
        cameraSpring.Initialize();
        cameraLean.Initialize();
        stanceVignette.Initialize(volume.profile);
        
        
    }

    void OnDestroy()
    {
        inputActions.Dispose();
    }

    // Update is called once per frame
    void Update()
    {
        //get cam input and update rotation
        var input = inputActions.Player;
        CameraInput cameraInput = new CameraInput{Look = input.Look.ReadValue<Vector2>()};
        playerCamera.UpdateRotation(cameraInput);
        
        //get character input and update
        CharacterInput characterInput = new CharacterInput
        {
            Rotation    = playerCamera.transform.rotation,
            Move        = input.Move.ReadValue<Vector2>(),
            Jump        = input.Jump.WasPressedThisFrame(),
            JumpSustain = input.Jump.IsPressed(),
            Crouch       = input.Crouch.IsPressed() ? CrouchInput.Toggle : CrouchInput.None
        };
        playerCharacter.UpdateInput(characterInput);
        #if UNITY_EDITOR
        if (Keyboard.current.tKey.wasPressedThisFrame)
        {
            var ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                Teleport(hit.point);
            }
        }
        #endif

    }

    void LateUpdate()
    {
        float deltaTime = Time.deltaTime;
        Transform cameraTarget = playerCharacter.GetCameraTarget();
        CharacterState state = playerCharacter.GetState();
        
        playerCamera.UpdatePosition(cameraTarget);
        cameraSpring.UpdateSpring(deltaTime, cameraTarget.up);
        cameraLean.UpdateLean(deltaTime,state.Stance == Stance.Slide, state.Acceleration ,cameraTarget.up);
        
        stanceVignette.UpdateVignette(deltaTime, state.Stance);
    }

    public void Teleport(Vector3 position)
    {
        playerCharacter.SetPosition(position);
    }
}

