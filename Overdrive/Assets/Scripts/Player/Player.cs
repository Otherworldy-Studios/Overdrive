using UnityEngine;

public class Player : MonoBehaviour
{
   
    [SerializeField]private PlayerCharacter playerCharacter;
    [SerializeField] private PlayerCamera playerCamera;
    
    private Controls inputActions;
    void Start()
    {
        inputActions = new Controls();
        inputActions.Enable();
        
        playerCharacter.Initialize();
        playerCamera.Initialize(playerCharacter.GetCameraTarget());
        
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
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
            Rotation = playerCamera.transform.rotation,
            Move = input.Move.ReadValue<Vector2>(),
            Jump = input.Jump.WasPressedThisFrame()
        };
        playerCharacter.UpdateInput(characterInput);


    }

    void LateUpdate()
    {
        playerCamera.UpdatePosition(playerCharacter.GetCameraTarget());
    }
}
