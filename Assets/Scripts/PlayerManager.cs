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
        GameObject laserPrefab;

        [SerializeField]
        Transform[] shootFroms;

        bool isFiring;
        PlayerController playerController;
        GameManager gameManager;
        int actorNum;
        float laserTimeout = 0;
        float energy = 100f;
        float health = 100f;

        #endregion

        #region Constants

        const float TOTAL_ENERGY = 100f;
        const float LASER_ENERGY_COST = 0.5f;
        const float LASER_FIRERATE = 0.12f;
        const float FULL_HEALTH = 100f;

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
            actorNum = PhotonNetwork.LocalPlayer.ActorNumber;
            Debug.Log("I am player number " + actorNum + " with username " + PhotonNetwork.LocalPlayer.NickName);

            gameManager = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameManager>();
        }

        // Update is called once per frame
        void Update() {
            playerController.enabled = photonView.IsMine;

            if (photonView.IsMine == false && PhotonNetwork.IsConnected == true) {
                if (playerCamera != null) {
                    playerCamera.SetActive(false);
                }
            }

            if (photonView.IsMine) {
                energy += 3f * Time.deltaTime;

                if (laserTimeout > 0) {
                    laserTimeout -= Time.deltaTime;
                }

                if (energy >= TOTAL_ENERGY) {
                    energy = TOTAL_ENERGY;
                }
            }
        }

        #endregion

        #region Public Methods

        public void fire() {
            if (energy >= LASER_ENERGY_COST && laserTimeout <= 0) {
                foreach (Transform shootFrom in shootFroms) {
                    laserTimeout = LASER_FIRERATE;
                    GameObject laser = PhotonNetwork.Instantiate(laserPrefab.name, shootFrom.position, shootFrom.rotation, 0);
                    Laser laserComp = laser.GetComponent<Laser>();
                    laserComp.owner = this.gameObject;
                    energy -= LASER_ENERGY_COST;
                }
            }
        }

        public void takeDamage(float damage) {
            if (health - damage < 0) {
                health = FULL_HEALTH;
                playerController.rb.velocity = Vector3.zero;
                playerController.rb.angularVelocity = Vector3.zero;
                gameManager.respawn(this.gameObject, actorNum);
            } else {
                health -= damage;
            }
        }

        #endregion

        #region Private Methods

        void leaveGame() {
            PlayerController.CONTROLS_ENABLED = false;
            GameObject.Find("Game Manager").GetComponent<GameManager>().LeaveRoom();
        }

        #endregion
    }
}
