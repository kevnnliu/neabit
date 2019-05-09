using UnityEngine;
using UnityEngine.Networking;
using System.Collections.Generic;
using UnityEngine.UI;

public class PlaneControl : NetworkBehaviour
{
    public GameObject pilotCamera;
    private Rigidbody rb;
    public GameObject blueLaserPrefab;
    public GameObject redLaserPrefab;
    public GameObject thruster;
    public GameObject blueModel;
    public GameObject redModel;
    public AudioSource thrusterSound;
    public AudioSource onHitSound;
    public GameObject explosionPrefab;
    public AudioSource leftLaser;
    public AudioSource rightLaser;
    public bool keyboardControl;
    public string set_ID;
    public float serverLag;
    public GameObject warning;
    public GameObject reticleNear;
    public GameObject reticleFar;
    public GameObject interior;
    public float bound;
    public float trackingThreshold;
    public float trackingRange;

    [SyncVar]
    public float hp;
    [SyncVar]
    public bool isDead;
    [SyncVar]
    public string _ID;
    [SyncVar]
    public bool isThrusting;

    // PHYSICS values
    public float throttleCoeff;
    public float pitchRate;
    public float yawRate;
    public float rollRate;
    public float verticalRate;

    // SIMULATED CONTROLLER (Will be actual controller in VR)
    private float pitchPosition;
    private float rollPosition;
    private float yawPosition;
    private float throttlePosition;
    
    private float verticalPosition;
    private bool throttleInput;
    private bool brakeInput;
    private float fireRate;
    private float gunFireSide = -1;
    private float respawnTime;
    private float outOfBoundsTime;
    private bool inSights;
    private float thrustMultiplier = 1;
    private GameObject trackingTarget;
    private Vector3 calibration = Vector3.zero;

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
        isDead = false;
        respawnTime = 0;
        outOfBoundsTime = 0;

        GameObject[] goArray = GameObject.FindGameObjectsWithTag("Player");
        int playerNum = goArray.Length - 1;

        _ID = "Player " + playerNum;

        if (_ID == "Player 2") {
            blueModel.SetActive(false);
            redModel.SetActive(true);
            thruster.GetComponent<ParticleSystem>().startColor = new Color(1, 0, 0, 1);
        } else {
            blueModel.SetActive(true);
        }

        trackingCalculation();
    }
    
    void Update() {
        if (this.isLocalPlayer) {
            OVRInput.Update();

            if (Mathf.Abs(transform.position.x) > bound || Mathf.Abs(transform.position.y) > bound
                    || Mathf.Abs(transform.position.z) > bound) {
                BoundsWarning();
            } else {
                warning.SetActive(false);
                outOfBoundsTime = 0;
            }

            if (_ID != "Player 1" && _ID != "Player 2") {
                CmdUpdatePlayerID();
            }

            if (isDead) {
                respawnTime += Time.deltaTime;
                if (respawnTime >= 5) {
                    CmdRespawn();
                }
            }

            if (!isDead) {
                if (inSights) {
                    reticleNear.GetComponentInChildren<Image>().color = Color.yellow;
                } else {
                    reticleNear.GetComponentInChildren<Image>().color = Color.white;
                }

                if (Input.GetKeyDown(KeyCode.Q)) {
                    keyboardControl = !keyboardControl;
                }

                bool tracking = false;

                if (keyboardControl) {
                    pitchPosition = ShiftTowards(pitchPosition, Input.GetAxis("Pitch"), controlRate);
                    rollPosition = ShiftTowards(rollPosition, Input.GetAxis("Roll"), controlRate);
                    yawPosition = ShiftTowards(yawPosition, Input.GetAxis("Yaw"), controlRate);
                    verticalPosition = ShiftTowards(verticalPosition, Input.GetAxis("Vertical"), controlRate);
                    throttleInput = Input.GetKey(KeyCode.Space);
                    brakeInput = Input.GetKey(KeyCode.LeftShift);
                    tracking = Input.GetKey(KeyCode.U);

                    if (fireRate > 0) {
                        fireRate -= Time.deltaTime;
                    } else {
                        fireRate = 0;
                    }

                    if (Input.GetKey(KeyCode.P) && fireRate == 0) {
                        float rate = 0.05f;
                        if (tracking) {
                            rate = 0.5f;
                        }
                        fireRate = rate;
                        if (gunFireSide > 0) {
                            rightLaser.Play();
                        } else {
                            leftLaser.Play();
                        }
                        CmdShoot(rate, inSights && tracking, trackingTarget, gunFireSide, _ID == "Player 2");
                        gunFireSide *= -1;
                    }
                } else {
                    pitchPosition = ShiftTowards(pitchPosition, OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick).y, controlRate);
                    rollPosition = ShiftTowards(rollPosition, OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick).x, controlRate);
                    yawPosition = ShiftTowards(yawPosition, OVRInput.Get(OVRInput.Axis2D.SecondaryThumbstick).x, controlRate);
                    verticalPosition = ShiftTowards(verticalPosition, OVRInput.Get(OVRInput.Axis2D.SecondaryThumbstick).y, controlRate);
                    throttleInput = OVRInput.Get(OVRInput.Axis1D.PrimaryHandTrigger) > 0.5;
                    brakeInput = OVRInput.Get(OVRInput.Axis1D.PrimaryIndexTrigger) > 0.5;
                    tracking = OVRInput.Get(OVRInput.Axis1D.SecondaryHandTrigger) > 0.5;
                    bool shooting = OVRInput.Get(OVRInput.Axis1D.SecondaryIndexTrigger) > 0.5;

                    if (OVRInput.Get(OVRInput.Button.PrimaryThumbstick)) {
                        calibration += 0.1f * Vector3.up * Time.deltaTime;
                        pilotCamera.transform.localPosition += 0.1f * Vector3.up * Time.deltaTime;
                    }
                    if (OVRInput.Get(OVRInput.Button.SecondaryThumbstick)) {
                        calibration += 0.1f * Vector3.down * Time.deltaTime;
                        pilotCamera.transform.localPosition += 0.1f * Vector3.down * Time.deltaTime;
                    }

                    if (fireRate > 0) {
                        fireRate -= Time.deltaTime;
                    } else {
                        fireRate = 0;
                    }

                    if (shooting && fireRate == 0) {
                        float rate = 0.05f;
                        if (tracking) {
                            rate = 0.5f;
                        }
                        fireRate = rate;
                        if (gunFireSide > 0) {
                            rightLaser.Play();
                        } else {
                            leftLaser.Play();
                        }
                        CmdShoot(rate, inSights && tracking, trackingTarget, gunFireSide, _ID == "Player 2");
                        gunFireSide *= -1;
                    }
                }

                if (tracking) {
                    reticleFar.GetComponentInChildren<Image>().color = Color.yellow;
                } else {
                    reticleFar.GetComponentInChildren<Image>().color = Color.white;
                }
            }

            if (hp <= 0 || Input.GetKeyDown(KeyCode.R)) {
                if (!isDead) {
                    CmdExplode();
                }
            }

            updateInterval += Time.deltaTime;
            if (updateInterval > 0.11f) // 9 times per second
            {
                updateInterval = 0;
                CmdSync(transform.position, transform.rotation);
                CmdUpdateParticles(isThrusting);
            }
        } else {
            transform.position = Vector3.Lerp (transform.position, realPosition, 0.1f);
            transform.rotation = Quaternion.Lerp (transform.rotation, realRotation, 0.1f);
            var em = thruster.GetComponent<ParticleSystem>().emission;
            em.enabled = isThrusting;
        }
    }

    void FixedUpdate() {
        if (this.isLocalPlayer) {
            OVRInput.FixedUpdate();
            var em = thruster.GetComponent<ParticleSystem>().emission;
            em.enabled = isThrusting;

            if (!isDead) {
                if (brakeInput) {
                    thrustMultiplier = 1.8f;
                } else {
                    thrustMultiplier = 1;
                }
                rb.AddRelativeTorque(new Vector3(pitchRate * pitchPosition, yawRate * yawPosition
                                        , -rollRate * rollPosition) * thrustMultiplier);
                rb.AddRelativeForce(transform.up * verticalPosition * verticalRate);
                if (throttleInput) {
                    rb.AddRelativeForce(0, 0, throttleCoeff * throttlePosition
                                         / thrustMultiplier / thrustMultiplier);
                    if (!thrusterSound.isPlaying) {
                        thrusterSound.Play();
                        isThrusting = true;
                    }
                    OVRInput.SetControllerVibration(1, 1, OVRInput.Controller.LTouch);
                } else {
                    if (thrusterSound.isPlaying) {
                        thrusterSound.Stop();
                        isThrusting = false;
                    }
                    if (rb.velocity.magnitude > 0) {
                        rb.AddRelativeForce(0.15f * -rb.velocity / thrustMultiplier / thrustMultiplier);
                    }
                    OVRInput.SetControllerVibration(0, 0, OVRInput.Controller.LTouch);
                }

                trackingCalculation();
            }
        } else if (this.isLocalPlayer) {
            OVRInput.FixedUpdate();
            if (thrusterSound.isPlaying) {
                thrusterSound.Stop();
            }
            if (rb.velocity.magnitude > 0) {
                rb.velocity = Vector3.zero;
            }
        }
    }

    private void OnCollisionEnter(Collision col) {
        GameObject other = col.transform.root.gameObject;
        if (other.tag == "Structure") {
            CmdExplode();
        } else if (other.tag == "Player") {
            CmdExplode();
            if (!other.GetComponent<PlaneControl>().isDead) {
                other.GetComponent<PlaneControl>().CmdExplode();
            }
        }
    }

    void trackingCalculation() {
        // is target in reticles?
        List<PlaneControl> playerList = new List<PlaneControl>();
        GameObject[] playerArray = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject go in playerArray) {
            if (go != null && go.GetComponent<PlaneControl>() != null) {
                playerList.Add(go.GetComponent<PlaneControl>());
            }
        }
        inSights = false;
        if (playerList.Count > 1) {
            foreach (PlaneControl p in playerList) {
                if (p._ID == _ID) {
                    continue;
                }
                Vector3 direction = p.transform.position - reticleNear.transform.position;
                float angle = Vector3.Angle(direction, transform.forward);
                if (angle < trackingThreshold && Vector3.Distance(p.transform.position, reticleNear.transform.position) < trackingRange) {
                    inSights = true;
                    trackingTarget = p.gameObject;
                    Debug.Log("Target in sights");
                    break;
                }
            }
        }
    }

    void BoundsWarning() {
        warning.SetActive(true);
        outOfBoundsTime += Time.deltaTime;
        if (outOfBoundsTime >= 10) {
            CmdExplode();
            outOfBoundsTime = -50;
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
    void CmdShoot(float rate, bool shouldTrack, GameObject trackTarget, float gunSide, bool red) {
        if (red) {
            RpcShootRed(_ID, shouldTrack, trackTarget, gunSide);
        } else {
            RpcShootBlue(_ID, shouldTrack, trackTarget, gunSide);
        }
    }

    [ClientRpc]
    void RpcShootBlue(string playerID, bool shouldTrack, GameObject trackTarget, float gunSide) {
        Vector3 position = new Vector3(gunSide * 3, 0.3f, 8);
        position = transform.position + (transform.rotation * position);

        GameObject laser = Instantiate(blueLaserPrefab, position, transform.rotation);

        Laser laserComponent = laser.GetComponent<Laser>();
        laserComponent.owner = playerID;
        laserComponent.tracking = shouldTrack;
        if (shouldTrack) {
            laserComponent.trackedTarget = trackTarget;
        }
    }

    [ClientRpc]
    void RpcShootRed(string playerID, bool shouldTrack, GameObject trackTarget, float gunSide) {
        Vector3 position = new Vector3(gunSide * 3, 0.3f, 8);
        position = transform.position + (transform.rotation * position);

        GameObject laser = Instantiate(redLaserPrefab, position, transform.rotation);

        Laser laserComponent = laser.GetComponent<Laser>();
        laserComponent.owner = playerID;
        laserComponent.tracking = shouldTrack;
        if (shouldTrack) {
            laserComponent.trackedTarget = trackTarget;
        }
    }

    [ClientRpc]
    void RpcDisableModel() {
        pilotCamera.transform.localPosition -= 69f * Vector3.forward;
        blueModel.SetActive(false);
        interior.SetActive(false);
        redModel.SetActive(false);
        thruster.SetActive(false);
        GetComponent<BoxCollider>().enabled = false;
    }

    [Command]
    void CmdSync(Vector3 position, Quaternion rotation) {
        realPosition = position;
        realRotation = rotation;
    }

    [Command]
    void CmdUpdateParticles(bool thrust) {
        isThrusting = thrust;
    }

    [Command]
    void CmdUpdatePlayerID() {
        _ID = set_ID;
        if (_ID == "Player 2") {
            blueModel.SetActive(false);
            redModel.SetActive(true);
            thruster.GetComponent<ParticleSystem>().startColor = new Color(1, 0, 0, 1);
        } else {
            blueModel.SetActive(true);
        }
    }

    [Command]
    void CmdExplode() {
        GameObject e = Instantiate(explosionPrefab, transform.position, transform.rotation);
        NetworkServer.SpawnWithClientAuthority(e, GetComponent<NetworkIdentity>().connectionToClient);
        isDead = true;
        GetComponent<BoxCollider>().enabled = false;
        hp = 0;
        thruster.SetActive(false);
        rb.velocity = Vector3.zero;
        RpcDisableModel();
    }

    [Command]
    void CmdRespawn(){
        CmdRestoreHealth();
        var spawn = NetworkManager.singleton.GetStartPosition();
        var newPlayer = (GameObject) Instantiate(NetworkManager.singleton.playerPrefab, spawn.position, spawn.rotation);
        newPlayer.GetComponent<PlaneControl>().pilotCamera.transform.localPosition += calibration;
        newPlayer.GetComponent<PlaneControl>().calibration = calibration;
        newPlayer.GetComponent<PlaneControl>().set_ID = _ID;
        NetworkServer.Destroy(this.gameObject);
        NetworkServer.ReplacePlayerForConnection(this.connectionToClient, newPlayer, this.playerControllerId);
    }

    [Command]
    public void CmdPlayerShot(float dmg, GameObject laser) {
        if (laser != null) {
            hp -= dmg;
            GetComponentInChildren<HealthBar>().TakeDamage();
            onHitSound.Play();
        }
    }

    [Command]
    void CmdRestoreHealth() {
        hp = 100;
    }
}
