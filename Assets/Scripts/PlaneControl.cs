using UnityEngine;

public class PlaneControl : MonoBehaviour
{
    private Rigidbody rb;

    // PHYSICS values
    public float throttleCoeff = 1;
    public float pitchRate = 10;
    public float yawRate = 20;
    public float rollRate = 4;
    public float dragCoeff;

    // SIMULATED CONTROLLER (Will be actual controller in VR)
    private float pitchPosition;
    private float yawPosition;
    private float rollPosition;
    private float throttlePosition;

    // How much keyboard inputs affect controller (will not be necessary in VR)
    public float controlRate = 2;

    void Start() {
        rb = GetComponent<Rigidbody>();

        pitchPosition = 0;
        yawPosition = 0;
        rollPosition = 0;
        throttlePosition = 0;
    }
    
    void Update() {
        // Update simulated controller position based on keyboard inputs
        pitchPosition = ShiftTowards(pitchPosition, Input.GetAxis("Pitch"), controlRate);
        yawPosition = ShiftTowards(yawPosition, Input.GetAxis("Yaw"), controlRate);
        rollPosition = ShiftTowards(rollPosition, Input.GetAxis("Roll"), controlRate);

        float throttleInput = Input.GetAxis("Throttle");
        if (throttleInput > 0)
        {
            throttlePosition = ShiftTowards(throttlePosition, 1, throttleInput * controlRate);
        }
        else if (throttleInput < 0)
        {
            throttlePosition = ShiftTowards(throttlePosition, 0, throttleInput * controlRate);
        }
    }

    void FixedUpdate() {
        rb.AddRelativeTorque(new Vector3(pitchRate * pitchPosition, yawRate * yawPosition, -rollRate * rollPosition));
        rb.AddRelativeForce(0, 0, throttleCoeff * throttlePosition);
    }

    // Updates a value towards a target value, with a maximum delta per second
    float ShiftTowards(float value, float target, float delta)
    {
        if (value < target)
        {
            return Mathf.Min(value + Mathf.Abs(delta) * Time.deltaTime, target);
        }
        return Mathf.Max(value - Mathf.Abs(delta) * Time.deltaTime, target);
    }
}
