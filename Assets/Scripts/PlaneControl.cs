using UnityEngine;
using UnityEngine.Networking;

public class PlaneControl : NetworkBehaviour
{
    public GameObject pilotCamera;
    private Rigidbody rb;
    public GameObject blueLaserPrefab;
    public GameObject redLaserPrefab;

    [SyncVar]
    public float hp;

    // PHYSICS values
    public float throttleCoeff;
    public float pitchRate;
    public float yawRate;
    public float rollRate;
    public float dragCoeff;
    public float offset;
    public GameObject spawn;
    public bool keyboardControl;
    public string _ID;

    // SIMULATED CONTROLLER (Will be actual controller in VR)
    private float pitchPosition;
    private float rollPosition;
    private float yawPosition;
    private float throttlePosition;
    private bool throttleInput;
    private float fireRate;

    // How much keyboard inputs affect controller (will not be necessary in VR)
    public float controlRate = 2;

    // custom transform syncing
    [SyncVar]
    Vector3 realPosition = Vector3.zero;
    [SyncVar]
    Quaternion realRotation;
    private float updateInterval;

    void Start() {
        if (isLocalPlayer) {
            pilotCamera.SetActive(true);
        }
        rb = GetComponent<Rigidbody>();

        pitchPosition = 0;
        rollPosition = 0;
        throttlePosition = 1;
        throttleInput = false;
        fireRate = 0.5f;
        spawn = GameObject.Find("SpawnLocation");

        GameObject[] goArray = GameObject.FindGameObjectsWithTag("Player");
        int playerNum = goArray.Length - 1;

        _ID = "Player " + playerNum;
    }
    
    void Update() {
        if (this.isLocalPlayer) {
            OVRInput.Update();

            if (keyboardControl) {
                pitchPosition = ShiftTowards(pitchPosition, Input.GetAxis("Pitch"), controlRate);
                rollPosition = ShiftTowards(rollPosition, Input.GetAxis("Roll"), controlRate);
                yawPosition = ShiftTowards(yawPosition, Input.GetAxis("Yaw"), controlRate);
                throttleInput = Input.GetKey(KeyCode.Space);

                if (Input.GetKeyDown(KeyCode.P)) {
                    CmdShoot();
                    if (!this.isServer) {
                        ClientShoot(); //spawning for uniquely client-side
                    }
                }
            } else {
                pitchPosition = ShiftTowards(pitchPosition, OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick, OVRInput.Controller.LTouch).y, controlRate);
                rollPosition = ShiftTowards(rollPosition, OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick, OVRInput.Controller.LTouch).x, controlRate);
                yawPosition = ShiftTowards(yawPosition, OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick, OVRInput.Controller.RTouch).x, controlRate);
                throttleInput = OVRInput.Get(OVRInput.Axis1D.PrimaryHandTrigger, OVRInput.Controller.RTouch) > 0.5;

                if (fireRate > 0) {
                    fireRate -= Time.deltaTime;
                } else {
                    fireRate = 0;
                }

                if (OVRInput.Get(OVRInput.Axis1D.PrimaryIndexTrigger, OVRInput.Controller.RTouch) > 0.5 && fireRate == 0) {
                    fireRate = 0.4f;
                    CmdShoot();
                    if (!this.isServer) {
                        ClientShoot(); //spawning for uniquely client-side
                    }
                }
            }

            if (hp <= 0) {
                transform.position = spawn.transform.position;
                transform.rotation = Quaternion.identity;
                CmdRestoreHealth();
            }

            updateInterval += Time.deltaTime;
            if (updateInterval > 0.11f) // 9 times per second
            {
                updateInterval = 0;
                CmdSync(transform.position, transform.rotation);
            }
        } else {
            transform.position = Vector3.Lerp (transform.position, realPosition, 0.1f);
            transform.rotation = Quaternion.Lerp (transform.rotation, realRotation, 0.1f);
        }
    }

    void FixedUpdate() {
        OVRInput.FixedUpdate();
        if (this.isLocalPlayer) {
            rb.AddRelativeTorque(new Vector3(pitchRate * pitchPosition, yawRate * yawPosition, -rollRate * rollPosition));
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
    float ShiftTowards(float value, float target, float delta) {
        if (value < target) {
            return Mathf.Min(value + Mathf.Abs(delta) * Time.deltaTime, target);
        }
        return Mathf.Max(value - Mathf.Abs(delta) * Time.deltaTime, target);
    }

    [Command]
    void CmdSync(Vector3 position, Quaternion rotation) {
        realPosition = position;
        realRotation = rotation;
    }

    [Command]
    public void CmdShoot() {
        if (_ID == "Player 1") {
            GameObject l = Instantiate(blueLaserPrefab, rb.position + (transform.forward * offset), transform.rotation);
            l.GetComponent<Laser>().owner = _ID;
            NetworkServer.SpawnWithClientAuthority(l, this.gameObject);
        } else {
            GameObject l = Instantiate(redLaserPrefab, rb.position + (transform.forward * offset), transform.rotation);
            l.GetComponent<Laser>().owner = _ID;
            NetworkServer.SpawnWithClientAuthority(l, this.gameObject);
        }
    }

    [Client]
    public void ClientShoot() {
        if (_ID == "Player 1") {
            GameObject l = Instantiate(blueLaserPrefab, rb.position + (transform.forward * offset), transform.rotation);
            l.GetComponent<Laser>().owner = _ID;
        } else {
            GameObject l = Instantiate(redLaserPrefab, rb.position + (transform.forward * offset), transform.rotation);
            l.GetComponent<Laser>().owner = _ID;
        }
    }

    [Command]
    public void CmdPlayerShot(float dmg, GameObject laser) {
        if (laser != null) {
            hp -= dmg;
        }
        NetworkServer.Destroy(laser);
    }

    [Command]
    public void CmdRestoreHealth() {
        hp = 100;
    }
}
