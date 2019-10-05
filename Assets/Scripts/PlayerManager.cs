using System.Collections;
using System.Collections.Generic;
using UnityEngine;
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

        bool isFiring;
        PlayerController playerController;

        #endregion

        #region Public Fields

        public float energy = 100f;
        public static GameObject LocalPlayerInstance;
        public GameObject playerCamera;
        public bool CONTROLS_ENABLED = true;

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
        }

        // Update is called once per frame
        void Update() {
            if (photonView.IsMine == false && PhotonNetwork.IsConnected == true) {
                playerController.enabled = false;
                playerCamera.SetActive(false);
            }
        }

        void OnTriggerEnter(Collider other) {
            if (!photonView.IsMine) {
                return;
            }
            if (other.CompareTag("Bolt")) {
                EMPBolt bolt = other.GetComponent<EMPBolt>();
                if (bolt.owner != this.gameObject) {
                    if (energy - bolt.drain < 0) {
                        energy = 0;
                    } else {
                        energy -= bolt.drain;
                    }
                    PhotonNetwork.Destroy(other.gameObject);
                }
            } else if (other.CompareTag("Checkpoint")) {
                Checkpoint checkpoint = other.GetComponent<Checkpoint>();
                if (checkpoint.laps >= 3) {
                    leaveGame();
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

        public void fire() {
            GameObject bolt = PhotonNetwork.Instantiate(boltPrefab.name, transform.position, transform.rotation, 0);
            bolt.GetComponent<EMPBolt>().owner = this.gameObject;
        }

        #endregion

        #region Private Methods

        void leaveGame() {
            GameObject.Find("Game Manager").GetComponent<GameManager>().LeaveRoom();
        }

        #endregion
    }
}
