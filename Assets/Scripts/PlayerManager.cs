using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

namespace com.tuth.neabit {
    public class PlayerManager : MonoBehaviourPunCallbacks, IPunObservable {

        #region IPunObservable implementation

        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info) {
            if (stream.IsWriting) {
                stream.SendNext(isFiring);
                stream.SendNext(energy);
            }
            else {
                this.isFiring = (bool)stream.ReceiveNext();
                this.energy = (float)stream.ReceiveNext();
            }
        }

        #endregion

        #region Private Fields

        [SerializeField]
        GameObject boltPrefab;

        [SerializeField]
        Transform shootFrom;

        bool isFiring;
        int playerNumber;
        PlayerController playerController;
        GameManager gameManager;
        float energy = 100f;
        Slider energyMeter;

        #endregion

        #region Constants

        const int LAPS_TO_WIN = 3;
        const float TOTAL_ENERGY = 100f;
        const float BOLT_ENERGY_COST = 20f;

        #endregion

        #region Public Fields

        public static GameObject LocalPlayerInstance;
        public GameObject playerCamera;
        public bool CONTROLS_ENABLED = true;
        public GameInfoDisplay gameInfoDisplay;

        #endregion

        #region MonoBehaviour CallBacks

        void Awake() {
            if (photonView.IsMine) {
                PlayerManager.LocalPlayerInstance = this.gameObject;
            }

            DontDestroyOnLoad(this.gameObject);
        }

        // Start is called before the first frame update
        void Start() {
            playerController = GetComponent<PlayerController>();
            playerNumber = PhotonNetwork.LocalPlayer.ActorNumber - 1;
            Debug.Log("I am player number " + playerNumber + " with username " + PhotonNetwork.LocalPlayer.NickName);

            gameManager = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameManager>();
        }

        // Update is called once per frame
        void Update() {
            if (photonView.IsMine == false && PhotonNetwork.IsConnected == true) {
                playerController.enabled = false;
                if (playerCamera != null) {
                    playerCamera.SetActive(false);
                }
            }

            if (photonView.IsMine && energyMeter != null) {
                energyMeter.value = energy / TOTAL_ENERGY;

                energy += 3f * Time.deltaTime;

                if (energy >= TOTAL_ENERGY) {
                    energy = TOTAL_ENERGY;
                }
            }
        }

        void OnTriggerEnter(Collider other) {
            if (!photonView.IsMine) {
                return;
            }
            if (other.CompareTag("Bolt")) {
                EMPBolt bolt = other.GetComponent<EMPBolt>();
                if (bolt.owner != this.gameObject) {
                    if (energy - bolt.getDrain() < 0) {
                        energy = 0;
                    } else {
                        energy -= bolt.getDrain();
                    }
                    PhotonNetwork.Destroy(other.gameObject);
                }
            } else if (other.CompareTag("Checkpoint")) {
                Checkpoint checkpoint = other.GetComponent<Checkpoint>();
                if (checkpoint.laps >= LAPS_TO_WIN) {
                    gameManager.completedRaceEventCall(PhotonNetwork.LocalPlayer.UserId);
                } else {
                    if (checkpoint.isEnabled) {
                        checkpoint.isEnabled = false;
                        checkpoint.nextCheckpoint.isEnabled = true;
                        checkpoint.laps += 1;
                    }
                }
            }
        }

        #endregion

        #region Public Methods

        public void fire(Vector3 target) {
            if (energy >= BOLT_ENERGY_COST) {
                GameObject bolt = PhotonNetwork.Instantiate(boltPrefab.name, shootFrom.position, Quaternion.identity, 0);
                EMPBolt boltComp = bolt.GetComponent<EMPBolt>();
                boltComp.target = target;
                boltComp.owner = this.gameObject;
                energy -= BOLT_ENERGY_COST;
            }
        }

        public void setEnergyMeter(Slider meter) {
            energyMeter = meter;
        }

        #endregion

        #region Private Methods

        void leaveGame() {
            GameObject.Find("Game Manager").GetComponent<GameManager>().LeaveRoom();
        }

        #endregion
    }
}
