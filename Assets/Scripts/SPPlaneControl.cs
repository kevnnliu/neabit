using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SPPlaneControl : MonoBehaviour
{
    public Transform planet;

    // PHYSICS values
    public Vector3 sphericalCoords;

    // SIMULATED CONTROLLER (Will be actual controller in VR)
    protected float joystickX = 0;
    protected float joystickY = 0;
    private float collectivePosition = 0;

    private Vector3 velocity = Vector3.zero;

    // How much keyboard inputs affect controller (will not be necessary in VR)
    public float controlRate = 2;
    public float turnRate = 30;

    // Control parameters
    public float cyclicPower = 10;
    public float throttlePower = 10;
    public float maximumVelocity = 10;
    public float dragForce = 1;
    public float turnDrag = 3;

    void Start() { }

    // Returns the X/Y position of the joystick (-1 to 1), from VR or keyboard
    protected void UpdateInputs()
    {
        Vector2 input = new Vector2(Input.GetAxis("JoystickX"), Input.GetAxis("JoystickY"));
        if (input.sqrMagnitude > 1)
        {
            input.Normalize();
        }
        joystickX = ShiftTowards(joystickX, input.x, controlRate);
        joystickY = ShiftTowards(joystickY, input.y, controlRate);
    }

    void Update()
    {
        // Update simulated controller position based on keyboard inputs
        UpdateInputs();

        // Rotate based on inputs
        Vector3 accel = Vector3.forward * joystickY * throttlePower;
        Vector3 turn = Vector3.up * joystickX;
        // Update physics manually
        ApplyAcceleration(accel, turn);
        ApplyVelocity();
        ApplyDrag();

        // Update inner (graphics only) physics
        SetOrientation(turn);

        sphericalCoords = ToSpherical(velocity);
    }

    // Physics stuff
    void ApplyAcceleration(Vector3 accel, Vector3 turn)
    {
        float turnReduction = 1 + accel.sqrMagnitude / (dragForce * dragForce) * turnDrag;
        transform.Rotate(turn * turnRate * Time.deltaTime);
        velocity += accel * Time.deltaTime;
    }

    void ApplyVelocity()
    {
        Vector3 up = transform.position.normalized;
        Vector3 right = Vector3.Cross(transform.forward, up).normalized;
        Vector3 forward = Vector3.Cross(up, right);

        Vector3 newPos = transform.position + (right * velocity.x + forward * velocity.z) * Time.deltaTime;
        newPos = newPos.normalized * transform.position.magnitude;
        transform.position = newPos + (up * velocity.y) * Time.deltaTime;
    }

    void ApplyDrag()
    {
        // Reduce floating-point error
        if (velocity.sqrMagnitude < 0.00001)
        {
            velocity = Vector3.zero;
        }
        float vFactor = velocity.sqrMagnitude / (maximumVelocity * maximumVelocity);
        velocity -= velocity.normalized * vFactor * dragForce * Time.deltaTime;
        // Reset view to flat
        /* Vector3 targetUp = transform.position - planet.position;
        Vector3 targetForward = Vector3.ProjectOnPlane(transform.forward, targetUp).normalized;
        Quaternion targetRotation = Quaternion.LookRotation(targetForward, targetUp);
        transform.rotation = ShiftTowards(transform.rotation, targetRotation, turnRate); */
    }

    void SetOrientation(Vector3 turn)
    {
        transform.GetChild(0).localRotation = Quaternion.Slerp(
            transform.GetChild(0).localRotation,
            Quaternion.Euler(joystickY * 5, 0, -joystickX * 20),
            0.1f
            );
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

    Vector3 ShiftTowards(Vector3 value, Vector3 target, float delta)
    {
        Vector3 diff = target - value;
        float dist = diff.magnitude;
        if (dist < delta * Time.deltaTime)
        {
            return target;
        }
        return value + diff / dist * delta * Time.deltaTime;
    }

    Quaternion ShiftTowards(Quaternion value, Quaternion target, float delta)
    {
        if (Quaternion.Angle(value, target) < delta * Time.deltaTime)
        {
            return target;
        }
        return Quaternion.Slerp(value, target, delta * Time.deltaTime / Quaternion.Angle(value, target));
    }

    // Converts spherical coordinates to Cartesian coordinates
    Vector3 ToCartesian(Vector3 vector)
    {
        return new Vector3(
            Mathf.Sin(vector.y) * Mathf.Cos(vector.z),
            Mathf.Sin(vector.y) * Mathf.Sin(vector.z),
            Mathf.Cos(vector.y)
            ) * vector.x;
    }

    Vector3 ToSpherical(Vector3 vector)
    {
        float r = vector.magnitude;
        return new Vector3(
            r,
            Mathf.Acos(vector.y / r),
            Mathf.Atan2(vector.z, vector.x)
            );
    }
}
