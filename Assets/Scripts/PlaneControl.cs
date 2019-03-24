using UnityEngine;

public class PlaneControl : MonoBehaviour
{
    public float throttleCoeff;
    public float turnSpeed;
    public float dragCoeff;

    private Rigidbody rb;
    private bool throttle;

    void Start() {
        rb = GetComponent<Rigidbody>();
        throttle = false;
    }
    
    void Update() {
        transform.Rotate(Input.GetAxis("Vertical") * turnSpeed * Time.deltaTime, 
                            Input.GetAxis("Rudder") * turnSpeed * Time.deltaTime * 0.2f, 
                            -Input.GetAxis("Horizontal") * turnSpeed * Time.deltaTime);
                    
        throttle = Input.GetKey(KeyCode.Space);
    }

    void FixedUpdate() {
        print(rb.velocity.magnitude);

        if (throttle) {
            rb.AddForce(transform.forward * throttleCoeff);
        }

        rb.drag = -0.95f + Mathf.Pow(dragCoeff, rb.velocity.magnitude);
    }

}
