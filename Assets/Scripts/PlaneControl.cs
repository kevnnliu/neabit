using UnityEngine;
using UnityEngine.Networking;

public class PlaneControl : NetworkBehaviour
{
    public GameObject pilotCamera;
    private Rigidbody rb;
    public GameObject laserPrefab;

    [SyncVar]
    public float hp;

    // PHYSICS values
    public float throttleCoeff = 1;
    public float pitchRate = 10;
    public float yawRate = 20;
    public float rollRate = 4;
    public float dragCoeff;
    public float offset;
    public GameObject spawn;

    // SIMULATED CONTROLLER (Will be actual controller in VR)
    private float pitchPosition;
    private float yawPosition;
    private float throttlePosition;
    private bool throttleInput;

    // How much keyboard inputs affect controller (will not be necessary in VR)
    public float controlRate = 2;

    void Start() {
        if (isLocalPlayer) {
            pilotCamera.SetActive(true);
        }
        rb = GetComponent<Rigidbody>();

        pitchPosition = 0;
        yawPosition = 0;
        throttlePosition = 1;
        throttleInput = false;
        spawn = GameObject.Find("SpawnLocation");
    }
    
    void Update() {
        // Update simulated controller position based on keyboard inputs
        if (this.isLocalPlayer) {
            pitchPosition = ShiftTowards(pitchPosition, Input.GetAxis("JoystickY"), controlRate);
            yawPosition = ShiftTowards(yawPosition, Input.GetAxis("JoystickX"), controlRate);

            throttleInput = Input.GetKey(KeyCode.Space);
            if (Input.GetKeyDown(KeyCode.P)) {
                RpcShoot();
            }
            if (hp <= 0) {
                transform.position = spawn.transform.position;
                hp = 100;
            }
        }
    }

    void FixedUpdate() {
        if (this.isLocalPlayer) {
            rb.AddRelativeTorque(new Vector3(pitchRate * pitchPosition, 0, -yawRate * yawPosition));
            if (throttleInput) {
                rb.AddRelativeForce(0, 0, throttleCoeff * throttlePosition);
            } else {
                if (rb.velocity.magnitude > 0) {
                    rb.AddRelativeForce(0.2f * -rb.velocity);
                }
            }
        }
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

    [ClientRpc]
    public void RpcShoot() {
        GameObject l = Instantiate(laserPrefab, rb.position + (transform.forward * offset), transform.rotation);
        l.GetComponent<Laser>().owner = this.gameObject;
    }
}
