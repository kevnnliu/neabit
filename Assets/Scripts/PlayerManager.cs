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
        int playerNumber;

        #endregion

        #region Public Fields

        public float energy = 100f;
        public static GameObject LocalPlayerInstance;
        public GameObject playerCamera;

        #endregion

        #region MonoBehaviour CallBacks

        void Awake() {
            if (photonView.IsMine) {
                PlayerManager.LocalPlayerInstance = this.gameObject;
            }

            playerNumber = PhotonNetwork.CurrentRoom.PlayerCount - 1;

            Debug.Log("I am player number " + playerNumber + " and my user id is " + PhotonNetwork.LocalPlayer.UserId);

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
            }
        }

        #endregion

        #region Public Methods

        public void fire() {
            GameObject bolt = PhotonNetwork.Instantiate(boltPrefab.name, transform.position, transform.rotation, 0);
            bolt.GetComponent<EMPBolt>().owner = this.gameObject;
        }

        public void prepRace() {
            GameManager gm = GameObject.Find("Game Manager").GetComponent<GameManager>();
            transform.position = gm.playerStarts[playerNumber].position;
            transform.rotation = gm.playerStarts[playerNumber].rotation;

            energy = 100;
            isFiring = false;

            GetComponent<PlayerController>().CONTROLS_ENABLED = false;
        }

        public void startRace() {
            GetComponent<PlayerController>().CONTROLS_ENABLED = true;
        }

        #endregion
    }
}
