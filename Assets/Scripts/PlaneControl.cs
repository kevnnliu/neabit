using UnityEngine;
using UnityEngine.Networking;
using System.Collections.Generic;

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
    public GameObject explosionPrefab;
    public AudioSource leftLaser;
    public AudioSource rightLaser;
    public bool keyboardControl;
    public string set_ID;
    public float serverLag;
    public GameObject warning;
    public GameObject reticleNear;
    public GameObject interior;

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

    // SIMULATED CONTROLLER (Will be actual controller in VR)
    private float pitchPosition;
    private float rollPosition;
    private float yawPosition;
    private float throttlePosition;
    private bool throttleInput;
    private float fireRate;
    private float gunFireSide = -1;
    private float respawnTime;
    private float outOfBoundsTime;
    private bool inSights;
    private GameObject trackingTarget;

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

            if (Mathf.Abs(transform.position.x) > 2500 || Mathf.Abs(transform.position.y) > 2500
                    || Mathf.Abs(transform.position.z) > 2500) {
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
                if (keyboardControl) {
                    pitchPosition = ShiftTowards(pitchPosition, Input.GetAxis("Pitch"), controlRate);
                    rollPosition = ShiftTowards(rollPosition, Input.GetAxis("Roll"), controlRate);
                    yawPosition = ShiftTowards(yawPosition, Input.GetAxis("Yaw"), controlRate);
                    throttleInput = Input.GetKey(KeyCode.Space);

                    if (fireRate > 0) {
                        fireRate -= Time.deltaTime;
                    } else {
                        fireRate = 0;
                    }

                    if (Input.GetKey(KeyCode.P) && fireRate == 0) {
                        shoot(0.03f, false, trackingTarget);
                        gunFireSide *= -1;
                    }
                } else {
                    pitchPosition = ShiftTowards(pitchPosition, OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick).y, controlRate);
                    rollPosition = ShiftTowards(rollPosition, OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick).x, controlRate);
                    yawPosition = ShiftTowards(yawPosition, OVRInput.Get(OVRInput.Axis2D.SecondaryThumbstick).x, controlRate);
                    throttleInput = OVRInput.Get(OVRInput.Axis1D.PrimaryHandTrigger) > 0.5;
                    bool tracking = OVRInput.Get(OVRInput.Axis1D.SecondaryHandTrigger) > 0.5;
                    bool shooting = OVRInput.Get(OVRInput.Axis1D.SecondaryIndexTrigger) > 0.5;

                    if (OVRInput.Get(OVRInput.Button.PrimaryThumbstick)) {
                        pilotCamera.transform.localPosition += 0.1f * Vector3.up * Time.deltaTime;
                    }
                    if (OVRInput.Get(OVRInput.Button.SecondaryThumbstick)) {
                        pilotCamera.transform.localPosition += 0.1f * Vector3.down * Time.deltaTime;
                    }

                    if (fireRate > 0) {
                        fireRate -= Time.deltaTime;
                    } else {
                        fireRate = 0;
                    }

                    if (shooting && fireRate == 0) {
                        shoot(0.03f, inSights && tracking, trackingTarget);
                        gunFireSide *= -1;
                    }
                }
            }

            if (hp <= 0 || Input.GetKeyDown(KeyCode.R)) {
                if (!isDead) {
                    CmdExplode();
                }
            }

            updateInterval += Time.deltaTime;
            if (updateInterval > 0.05f) // 20 times per second
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
                rb.AddRelativeTorque(new Vector3(pitchRate * pitchPosition, yawRate * yawPosition, -rollRate * rollPosition));
                if (throttleInput) {
                    rb.AddRelativeForce(0, 0, throttleCoeff * throttlePosition);
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
                        rb.AddRelativeForce(0.15f * -rb.velocity);
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
        List<GameObject> playerList = new List<GameObject>();
        GameObject[] playerArray = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject go in playerArray) {
            if (go != null) {
                playerList.Add(go);
            }
        }
        if (playerList.Count > 1) {
            foreach (GameObject p in playerList) {
                if (p.GetComponent<PlaneControl>()._ID != _ID) {
                    trackingTarget = p.gameObject;
                    RaycastHit hit;
                    if (Physics.Raycast(reticleNear.transform.position
                        , p.transform.position - reticleNear.transform.position, out hit, 3f)) {
                        if (hit.transform.gameObject.name == "Reticle Far") {
                            inSights = true;
                            break;
                        }
                    }
                }
                inSights = false;
            }
        }
    }

    void BoundsWarning() {
        warning.SetActive(true);
        outOfBoundsTime += Time.deltaTime;
        if (outOfBoundsTime >= 5) {
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

    [Client]
    void shoot(float rate, bool shouldTrack, GameObject trackTarget) {
        fireRate = rate;
        if (gunFireSide > 0) {
            rightLaser.Play();
        } else {
            leftLaser.Play();
        }
        Vector3 position = new Vector3(gunFireSide * 3, 0.3f, 8);
        Quaternion rotation = transform.rotation;

        if (!isServer) { // predicts where laser should be, ideally serverLag is updated in real time
            Vector3 shipPosition = transform.position + rb.velocity * Time.deltaTime * serverLag;
            Quaternion shipRotation = Quaternion.Euler(rb.angularVelocity * Time.deltaTime * serverLag)
                                         * transform.rotation;
            position = shipPosition + shipRotation * position;
        } else {
            position = transform.position + transform.rotation * position;
        }

        if (_ID == "Player 1") {
            CmdShootBlue(position, rotation, shouldTrack, trackTarget); // two versions because spawnable prefabs (don't clean)
        } else {
            CmdShootRed(position, rotation, shouldTrack, trackTarget);
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
        newPlayer.GetComponent<PlaneControl>().set_ID = _ID;
        NetworkServer.Destroy(this.gameObject);
        NetworkServer.ReplacePlayerForConnection(this.connectionToClient, newPlayer, this.playerControllerId);
    }

    [Command]
    void CmdShootBlue(Vector3 position, Quaternion rotation, bool shouldTrack, GameObject trackTarget) {
        GameObject laser = Instantiate(blueLaserPrefab, position, transform.rotation);
        Laser laserComponent = laser.GetComponent<Laser>();
        laserComponent.owner = _ID;
        laserComponent.tracking = shouldTrack;
        if (shouldTrack) {
            laserComponent.trackedTarget = trackTarget;
        }
        NetworkServer.SpawnWithClientAuthority(laser, GetComponent<NetworkIdentity>().connectionToClient);
    }

    [Command]
    void CmdShootRed(Vector3 position, Quaternion rotation, bool shouldTrack, GameObject trackTarget) {
        GameObject laser = Instantiate(redLaserPrefab, position, transform.rotation);
        Laser laserComponent = laser.GetComponent<Laser>();
        laserComponent.owner = _ID;
        laserComponent.tracking = shouldTrack;
        if (shouldTrack) {
            laserComponent.trackedTarget = trackTarget;
        }
        NetworkServer.SpawnWithClientAuthority(laser, GetComponent<NetworkIdentity>().connectionToClient);
    }

    [Command]
    public void CmdPlayerShot(float dmg, GameObject laser) {
        if (laser != null) {
            hp -= dmg;
        }
        NetworkServer.Destroy(laser);
    }

    [Command]
    void CmdRestoreHealth() {
        hp = 100;
    }
}
