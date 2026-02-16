using UnityEngine;

public class PlayerCamera : MonoBehaviour
{
    private Vector3 eulerAngles;
    [SerializeField] private float sensitivity = 0.1f;
    public void Initialize(Transform target)
    {
       transform.position = target.position;
       transform.eulerAngles = eulerAngles = target.eulerAngles;
       Cursor.lockState = CursorLockMode.Locked;
       Cursor.visible = false;
    }

    public void UpdateRotation(CameraInput input)
    {
        eulerAngles += new Vector3(-input.Look.y,input.Look.x,0f) * sensitivity;
        transform.eulerAngles = eulerAngles;
    }

    public void UpdatePosition(Transform target)
    {
        transform.position = target.position;
    }
}

public struct CameraInput
{
    public Vector2 Look;
}