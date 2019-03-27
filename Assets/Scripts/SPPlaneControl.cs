using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SPPlaneControl : MonoBehaviour
{
    public GameObject pilotCamera;
    public Transform planet;
    private Rigidbody rb;

    // PHYSICS values
    public Vector3 sphericalCoords;

    // SIMULATED CONTROLLER (Will be actual controller in VR)
    private float cyclicPositionX = 0;
    private float cyclicPositionY = 0;
    private float collectivePosition = 0;

    private Vector3 velocity = Vector3.zero;

    // How much keyboard inputs affect controller (will not be necessary in VR)
    public float controlRate = 2;
    public float turnRate = 30;

    // Control parameters
    public float cyclicPower = 10;
    public float collectivePower = 10;
    public float maximumVelocity = 10;

    void Start()
    {
        pilotCamera.SetActive(true);
        rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;
    }

    void Update()
    {
        // Update simulated controller position based on keyboard inputs
        cyclicPositionX = ShiftTowards(cyclicPositionX, Input.GetAxis("Roll"), controlRate);
        cyclicPositionY = ShiftTowards(cyclicPositionY, Input.GetAxis("Pitch"), controlRate);
        collectivePosition = ShiftTowards(collectivePosition, Input.GetAxis("Throttle"), controlRate);

        Vector3 accel = Vector3.forward * collectivePosition * maximumVelocity;
        velocity = Vector3.Lerp(velocity, accel, 0.05f);
        // Update physics manually
        ApplyVelocity(velocity);

        // Reset view to flat
        Vector3 targetUp = transform.position - planet.position;
        Vector3 targetForward = Vector3.ProjectOnPlane(transform.forward, targetUp).normalized;
        Quaternion targetRotation = Quaternion.LookRotation(targetForward, targetUp);
        transform.rotation = ShiftTowards(transform.rotation, targetRotation, turnRate);

        sphericalCoords = ToSpherical(velocity);
    }

    // Physics stuff
    void ApplyVelocity(Vector3 velocity)
    {
        Vector3 up = transform.position.normalized;
        Vector3 right = Vector3.Cross(transform.forward, up).normalized;
        Vector3 forward = Vector3.Cross(up, right);

        Vector3 newPos = transform.position + right * velocity.x + forward * velocity.z;
        newPos = newPos.normalized * transform.position.magnitude;
        transform.position = newPos + up * velocity.y;
    }
    
    Vector3 ApplyAcceleration(Vector3 velocity, Vector3 accel, float maxVelocity)
    {
        float v = velocity.magnitude / maxVelocity;
        Vector3 dV = accel * Time.deltaTime;
        Vector3 target = velocity + dV;
        float vtarget = target.magnitude;
        if (vtarget > maxVelocity)
        {
            return target * maxVelocity / vtarget;
        }
        return target;
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
