using UnityEngine;

public class CameraLean : MonoBehaviour
{
    [SerializeField] private float attackDamping = 0.5f; //used when input acceleration is greater than damped acceleration
    [SerializeField] private float decayDamping = 0.3f; //used when target acceleration is less than damped acceleration
    [SerializeField] private float walkStrength = 0.075f;
    [SerializeField] private float slideStrength = 0.2f;
    [SerializeField] private float strengthResponse = 5f;
    
    private Vector3 dampedAcceleration; 
    private Vector3 dampedAccelerationVel; 
    
    private float smoothStrength;
    public void Initialize()
    {
        smoothStrength = walkStrength;
    }

    public void UpdateLean(float deltaTime,bool isSliding, Vector3 acceleration, Vector3 up)
    {
        Vector3 planarAcceleration = Vector3.ProjectOnPlane(acceleration, up);
        float damping = planarAcceleration.magnitude > dampedAcceleration.magnitude ? attackDamping : decayDamping;
        dampedAcceleration = Vector3.SmoothDamp
        (
            current: dampedAcceleration,
            target: planarAcceleration,
            currentVelocity: ref dampedAccelerationVel,
            smoothTime: damping,
            maxSpeed: float.PositiveInfinity,
            deltaTime: deltaTime
        );

        //get the rotation axis based on the acceleration vector
        var leanAxis = Vector3.Cross(dampedAcceleration.normalized, up).normalized;
        
        //reset the rotation to that of its parent 
        transform.localRotation = Quaternion.identity;
        
        var targetStrength = isSliding ? slideStrength : walkStrength;
        
        smoothStrength = Mathf.Lerp(smoothStrength, targetStrength, 1f - Mathf.Exp(-strengthResponse * deltaTime));
        //rotate the camera around the lean axis
        transform.rotation = Quaternion.AngleAxis(-dampedAcceleration.magnitude * targetStrength, leanAxis) * transform.rotation;
    }
}
