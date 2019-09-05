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

        [SerializeField]
        GameObject playerCamera;

        bool isFiring;
        PlayerController playerController;

        #endregion

        #region Public Fields

        public float energy = 100f;
        public static GameObject LocalPlayerInstance;

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
            if (energy <= 0) {
                GameManager.Instance.LeaveRoom();
            }
        }

        void OnTriggerEnter(Collider other) {
            if (!photonView.IsMine) {
                return;
            }
            if (other.CompareTag("Bolt")) {
                if (other.GetComponent<EMPBolt>().owner != this.gameObject) {
                    energy -= 20;
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

        #endregion
    }
}
