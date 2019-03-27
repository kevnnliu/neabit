using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SPPlaneControl : MonoBehaviour
{
    public GameObject pilotCamera;
    public Transform planet;
    private Rigidbody rb;

    // PHYSICS values

    // SIMULATED CONTROLLER (Will be actual controller in VR)
    private float cyclicPositionX = 0;
    private float cyclicPositionY = 0;
    private float collectivePosition = 0;

    // How much keyboard inputs affect controller (will not be necessary in VR)
    public float controlRate = 2;
    public float turnRate = 2;

    // Pitch parameters (Increasing/Decreasing altitude)

    void Start()
    {
        pilotCamera.SetActive(true);
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        // Update simulated controller position based on keyboard inputs
        cyclicPositionX = ShiftTowards(cyclicPositionX, Input.GetAxis("Roll"), controlRate);
        cyclicPositionY = ShiftTowards(cyclicPositionY, Input.GetAxis("Pitch"), controlRate);
        collectivePosition = ShiftTowards(collectivePosition, Input.GetAxis("Throttle"), controlRate);

        transform.position += transform.forward * collectivePosition;

        // 
        Vector3 targetUp = transform.position - planet.position;
        Vector3 targetForward = Vector3.ProjectOnPlane(transform.forward, targetUp).normalized;

        Quaternion targetRotation = Quaternion.LookRotation(targetForward, targetUp);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 0.1f);
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
