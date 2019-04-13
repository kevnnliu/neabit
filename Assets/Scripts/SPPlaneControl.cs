using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SPPlaneControl : MonoBehaviour
{
    public GameObject pilotCamera;
    public Transform planet;

    // PHYSICS values
    public Vector3 sphericalCoords;

    // SIMULATED CONTROLLER (Will be actual controller in VR)
    private float cyclicPositionX = 0;
    private float cyclicPositionY = 0;
    private float collectivePosition = 0;

    private bool hoverState = false;

    private Vector3 velocity = Vector3.zero;

    // How much keyboard inputs affect controller (will not be necessary in VR)
    public float controlRate = 2;
    public float turnRate = 30;

    // Control parameters
    public float cyclicPower = 10;
    public float collectivePower = 10;
    public float maximumVelocity = 10;
    public float dragForce = 1;
    public float turnDrag = 3;

    void Start()
    {
        pilotCamera.SetActive(true);
    }

    void Update()
    {
        // Update simulated controller position based on keyboard inputs
        cyclicPositionX = ShiftTowards(cyclicPositionX, Input.GetAxis("Roll"), controlRate);
        cyclicPositionY = ShiftTowards(cyclicPositionY, Input.GetAxis("Pitch"), controlRate);

        float throttle = Input.GetAxis("Throttle");
        if (throttle > 0)
        {
            collectivePosition = ShiftTowards(collectivePosition, 1, throttle * controlRate);
        }
        else if (throttle < 0)
        {
            collectivePosition = ShiftTowards(collectivePosition, 0, -throttle * controlRate);
        }
        hoverState = Input.GetButton("Engine Release");

        // Rotate based on inputs
        Vector3 accel;
        Vector3 turn;
        if (hoverState)
        {
            accel = new Vector3(
                cyclicPositionX * cyclicPower,
                (collectivePosition - 0.4f) * collectivePower,
                cyclicPositionY * cyclicPower
                );
            turn = Vector3.zero;
        }
        else
        {
            accel = Vector3.forward * collectivePosition * collectivePower;
            turn = Vector3.up * cyclicPositionX;
        }
        // Update physics manually
        ApplyAcceleration(accel, turn);
        ApplyVelocity();
        ApplyDrag();

        // Update inner (graphics only) physics
        SetOrientation();

        sphericalCoords = ToSpherical(velocity);

        // Highly temporary
        transform.GetChild(0).Find("Panel").Find("straight_lever").Find("Wheel").localRotation = Quaternion.Euler(
            0, -90, 90 - 75 * collectivePosition
            );
    }

    // Physics stuff
    void ApplyAcceleration(Vector3 accel, Vector3 turn)
    {
        float turnReduction = 1 + accel.sqrMagnitude / (dragForce * dragForce) * turnDrag;
        transform.Rotate(turn * turnRate * Time.deltaTime / turnReduction);
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
        Vector3 targetUp = transform.position - planet.position;
        Vector3 targetForward = Vector3.ProjectOnPlane(transform.forward, targetUp).normalized;
        Quaternion targetRotation = Quaternion.LookRotation(targetForward, targetUp);
        transform.rotation = ShiftTowards(transform.rotation, targetRotation, turnRate);
    }

    void SetOrientation()
    {
        float turnReduction = 1 + collectivePosition * turnDrag;
        transform.GetChild(0).localRotation = Quaternion.Slerp(
            transform.GetChild(0).localRotation,
            Quaternion.Euler(cyclicPositionY * 15, 0, -cyclicPositionX * 30 / turnReduction),
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
