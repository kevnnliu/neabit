﻿using UnityEngine;
using UnityEngine.Networking;

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
    }
    
    void Update() {
        if (this.isLocalPlayer) {
            OVRInput.Update();

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
                        fireRate = 0.1f;
                        CmdShoot(gunFireSide);
                        gunFireSide *= -1;
                    }
                } else {
                    pitchPosition = ShiftTowards(pitchPosition, OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick).y, controlRate);
                    rollPosition = ShiftTowards(rollPosition, OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick).x, controlRate);
                    yawPosition = ShiftTowards(yawPosition, OVRInput.Get(OVRInput.Axis2D.SecondaryThumbstick).x, controlRate);
                    throttleInput = OVRInput.Get(OVRInput.Axis1D.PrimaryHandTrigger) > 0.5;

                    if (fireRate > 0) {
                        fireRate -= Time.deltaTime;
                    } else {
                        fireRate = 0;
                    }

                    if (OVRInput.Get(OVRInput.Axis1D.SecondaryIndexTrigger) > 0.5 && fireRate == 0) {
                        fireRate = 0.1f;
                        CmdShoot(gunFireSide);
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
                CmdSync(rb.position, rb.rotation);
                CmdUpdateParticles(isThrusting);
            }
        } else {
            transform.position = Vector3.Lerp (transform.position, realPosition, 0.2f);
            transform.rotation = Quaternion.Lerp (transform.rotation, realRotation, 0.2f);
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
                } else {
                    if (thrusterSound.isPlaying) {
                        thrusterSound.Stop();
                        isThrusting = false;
                    }
                    if (rb.velocity.magnitude > 0) {
                        rb.AddRelativeForce(0.15f * -rb.velocity);
                    }
                }
            }
        } else if (this.isLocalPlayer) {
            OVRInput.FixedUpdate();
            if (thrusterSound.isPlaying) {
                thrusterSound.Stop();
            }
            if (rb.velocity.magnitude > 0) {
                rb.AddRelativeForce(0.5f * -rb.velocity);
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

    [ClientRpc]
    void RpcDisableModel() {
        blueModel.SetActive(false);
        redModel.SetActive(false);
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
    void CmdShoot(float gunSide) {
        if (_ID == "Player 1") {
            GameObject l1 = Instantiate(blueLaserPrefab, transform.position
                             + (transform.forward * 8) + (gunSide * (transform.right * 3)) + (transform.up * 0.3f), realRotation);
            l1.GetComponent<Laser>().owner = _ID;
            if (gunSide > 0) {
                rightLaser.Play();
            } else {
                leftLaser.Play();
            }
            NetworkServer.SpawnWithClientAuthority(l1, GetComponent<NetworkIdentity>().connectionToClient);
        } else {
            GameObject l1 = Instantiate(redLaserPrefab, transform.position
                             + (transform.forward * 8) + (gunSide * (transform.right * 3)) + (transform.up * 0.3f), realRotation);
            l1.GetComponent<Laser>().owner = _ID;
            if (gunSide > 0) {
                rightLaser.Play();
            } else {
                leftLaser.Play();
            }
            NetworkServer.SpawnWithClientAuthority(l1, GetComponent<NetworkIdentity>().connectionToClient);
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
    void CmdRestoreHealth() {
        hp = 100;
    }
}
