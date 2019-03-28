using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SPCameraFollow : MonoBehaviour
{
    public GameObject target;
    public float distance = 10;
    public float angle = 50;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void LateUpdate()
    {
        // Calculate target location
        Vector3 up = target.transform.position.normalized;
        Vector3 right = Vector3.Cross(target.transform.forward, up).normalized;
        Vector3 forward = Vector3.Cross(up, right);

        Vector3 location = target.transform.position
            + up * distance * Mathf.Cos(Mathf.Deg2Rad * angle)
            - forward * distance * Mathf.Sin(Mathf.Deg2Rad * angle);

        transform.position = location;
        transform.LookAt(target.transform, up);
    }
}
