using UnityEngine;

public class CameraSpring : MonoBehaviour
{
    private bool springEnabled;

    private Vector3 springPosition;
    private Vector3 springVelocity;
    
    [SerializeField] private float springHalfLife = 0.075f;
    [Space] [SerializeField] private float springFrequency = 18f;
    [Space] [SerializeField] private float angularDisplacement = 2f;
    [SerializeField] private float linearDisplacement = 0.05f;
    

    public void Initialize()
    {
        springPosition = transform.position;
        springVelocity = Vector3.zero;
    }

    public void UpdateSpring(float deltaTime, Vector3 up)
    {
        Spring(ref springPosition, ref springVelocity, transform.position, springHalfLife, springFrequency, deltaTime);
        
        Vector3 localSpringPosition = springPosition - transform.position;
        var springHeight = Vector3.Dot(localSpringPosition, up);
        
        transform.eulerAngles = new Vector3(-springHeight * angularDisplacement,0f,0f);
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawLine(transform.position, springPosition);
        Gizmos.DrawSphere(springPosition, 0.1f);
        
    }

    /// <summary>
    /// Updates a spring-damper system simulation for smooth interpolation towards a target position.
    /// This implements a semi-implicit Euler integration scheme for a damped harmonic oscillator.
    /// </summary>
    /// <param name="currentPosition">Current position (will be updated by the function)</param>
    /// <param name="currentVelocity">Current velocity (will be updated by the function)</param>
    /// <param name="targetPosition">The target position the spring is trying to reach</param>
    /// <param name="halfLife">Time for the spring to reduce error by half (in seconds)</param>
    /// <param name="frequency">Oscillation frequency of the spring (in Hz)</param>
    /// <param name="timeStep">Time step for this simulation update (in seconds)</param>
    private static void Spring(ref Vector3 currentPosition, ref Vector3 currentVelocity, Vector3 targetPosition,
        float halfLife, float frequency, float timeStep)
    {
        // Calculate damping ratio based on half-life and frequency
        // This controls how quickly oscillations are damped out
        float dampingRatio = -Mathf.Log(0.5f) / (frequency * halfLife);

        // Precompute constants for the integration matrix
        float f = 1.0f + 2.0f * timeStep * dampingRatio * frequency;
        float omegaSquared = frequency * frequency; // ω² - natural frequency squared
        float hOmegaSquared = timeStep * omegaSquared; // h * ω²
        float hhOmegaSquared = timeStep * hOmegaSquared; // h² * ω²

        // Compute determinant of the integration matrix and its inverse
        float determinant = f + hhOmegaSquared;
        float determinantInverse = 1.0f / determinant;

        // Solve for next position using the integration matrix:
        // [position_next] = 1/det * [ f       h ] [position_current + h²ω²*target]
        // [velocity_next]         [ -hω²    f ] [velocity_current + hω²*(target - position_current)]
        Vector3 determinantX = f * currentPosition + timeStep * currentVelocity + hhOmegaSquared * targetPosition;
        Vector3 determinantV = currentVelocity + hOmegaSquared * (targetPosition - currentPosition);

        // Update position and velocity for the next time step
        currentPosition = determinantX * determinantInverse;
        currentVelocity = determinantV * determinantInverse;
    }
}