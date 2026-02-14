using UnityEngine;

public class MoveAxis : MonoBehaviour
{
    public enum Axis { X, Y, Z }
    public enum Mode { Continuous, PingPong }

    [Header("Movement")]
    public Mode mode = Mode.Continuous;
    public Axis axis = Axis.Z;
    public float speed = 2f;          // units/sec for Continuous, cycles/sec for PingPong
    public float distance = 1f;       // travel distance for PingPong

    [Header("Physics (optional)")]
    public bool useFixedUpdate = false;   // tick in FixedUpdate for physics-driven objects

    private Vector3 _startLocalPos;

    private void Start()
    {
        _startLocalPos = transform.localPosition;
    }

    private void Update()
    {
        if (!useFixedUpdate) Tick(Time.deltaTime);
    }

    private void FixedUpdate()
    {
        if (useFixedUpdate) Tick(Time.fixedDeltaTime);
    }

    private void Tick(float dt)
    {
        Vector3 localAxis = GetLocalAxis(); // (1,0,0), (0,1,0) or (0,0,1)

        if (mode == Mode.Continuous)
        {
            // Move strictly along the chosen LOCAL axis
            transform.Translate(localAxis * (speed * dt), Space.Self);
        }
        else // Mode.PingPong
        {
            // Oscillate along the LOCAL axis around the start position
            float t = Mathf.PingPong(Time.time * speed, distance) - (distance * 0.5f);
            transform.localPosition = _startLocalPos + localAxis * t;
        }
    }

    private Vector3 GetLocalAxis()
    {
        switch (axis)
        {
            case Axis.X: return Vector3.right;
            case Axis.Y: return Vector3.up;
            default: return Vector3.forward; // Axis.Z
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        // Preview the path in the editor
        Vector3 start = Application.isPlaying ? _startLocalPos : transform.localPosition;
        Vector3 localAxis = GetLocalAxis();
        Vector3 worldStart = transform.parent ? transform.parent.TransformPoint(start) : start;
        Vector3 worldEnd   = worldStart + (transform.TransformDirection(localAxis) * distance);

        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(worldStart, worldEnd);
        Gizmos.DrawSphere(worldStart, 0.02f);
        Gizmos.DrawSphere(worldEnd, 0.02f);
    }
#endif
}
