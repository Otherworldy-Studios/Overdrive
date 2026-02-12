using UnityEngine;

public class FlashingLight : MonoBehaviour
{
    [Header("Rotation Axes")]
    [Tooltip("Enable rotation around the X axis")]
    public bool rotateX = false;

    [Tooltip("Enable rotation around the Y axis")]
    public bool rotateY = false;

    [Tooltip("Enable rotation around the Z axis")]
    public bool rotateZ = false;

    [Header("Rotation Speed")]
    [Tooltip("Speed of rotation in degrees per second")]
    public float rotationSpeed = 90f;

    private void Update()
    {
        Vector3 rotation = Vector3.zero;

        if (rotateX)
            rotation.x = 1f;
        if (rotateY)
            rotation.y = 1f;
        if (rotateZ)
            rotation.z = 1f;

        // Apply rotation
        transform.Rotate(rotation * rotationSpeed * Time.deltaTime);
    }
}
