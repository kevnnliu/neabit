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
                stream.SendNext(health);
                stream.SendNext(isShielding);
            }
            else {
                this.isFiring = (bool)stream.ReceiveNext();
                this.health = (float)stream.ReceiveNext();
                this.isShielding = (bool)stream.ReceiveNext();
            }
        }

        #endregion

        #region Static Fields

        static bool ROTATIONAL_AIM_COMPENSATION = true;
        static float LASER_COMPENSATION_COEFF = 3f;

        static Dictionary<string, int> fieldIndex = new Dictionary<string, int>()
        {
            {"Nickname", 0},
            {"Kills", 1},
            {"Deaths", 2}
        };

        #endregion

        #region Private Fields

        [SerializeField]
        GameObject laserPrefab;

        [SerializeField]
        Transform[] shootFroms;

        [SerializeField]
        GameObject shieldObject;

        [SerializeField]
        Slider energyMeter;

        [SerializeField]
        Text[] gameDisplay;

        [SerializeField]
        Text[] scoreDisplay;

        bool isFiring;
        PlayerController playerController;
        GameManager gameManager;
        int actorNum;
        float laserTimeout = 0;
        float energy = 100f;
        float health = 100f;
        bool isShielding = false;
        GameObject playerCamera;
        Dictionary<string, string> playerScore = new Dictionary<string, string>();
        string playerID;
        Quaternion laserRotationalCompensation;

        #endregion

        #region Constants

        const float TOTAL_ENERGY = 100f;
        const float LASER_ENERGY_COST = 0.75f;
        const float SHIELD_ENERGY_COST = 25f;
        const float LASER_FIRERATE = 0.12f;
        const float FULL_HEALTH = 100f;
        const float ENERGY_CHARGE_RATE = 4f;

        #endregion

        #region Public Fields

        public static GameObject LocalPlayerInstance;
        public bool CONTROLS_ENABLED = true;
        public GameInfoDisplay gameInfoDisplay;
        public GameObject gameDisplayContainer;
        public GameObject scoreDisplayContainer;

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
            playerID = PhotonNetwork.LocalPlayer.UserId;
            
            initializeScore();
        }

        // Update is called once per frame
        void Update() {
            playerController.enabled = photonView.IsMine;

            energy += ENERGY_CHARGE_RATE * Time.deltaTime;

            energyMeter.value = energy / TOTAL_ENERGY;

            updateDisplay();

            if (laserTimeout > 0) {
                laserTimeout -= Time.deltaTime;
            }

            if (energy >= TOTAL_ENERGY) {
                energy = TOTAL_ENERGY;
            }

            if (isFiring && laserTimeout <= 0) {
                foreach (Transform shootFrom in shootFroms) {
                    laserTimeout = LASER_FIRERATE;
                    Quaternion compensatedAngle = shootFrom.rotation * laserRotationalCompensation;
                    Quaternion laserAngle = ROTATIONAL_AIM_COMPENSATION ? compensatedAngle : shootFrom.rotation;

                    GameObject laser = PhotonNetwork.Instantiate(laserPrefab.name, shootFrom.position, laserAngle, 0);
                    Laser laserComp = laser.GetComponent<Laser>();
                    laserComp.owner = this.gameObject;
                    energy -= LASER_ENERGY_COST;
                }
                isFiring = false;
            }

            if (!photonView.IsMine && PhotonNetwork.IsConnected) {
                if (playerCamera != null) {
                    playerCamera.SetActive(false);
                }
            }
        }

        #endregion

        #region Public Methods

        public void fire(Vector3 angularVelocity) {
            if (energy >= LASER_ENERGY_COST) {
                isFiring = true;
                laserRotationalCompensation = Quaternion.Euler(angularVelocity * LASER_COMPENSATION_COEFF);
            }
        }

        public void shield(bool amShielding) {
            if (energy > 0 && amShielding) {
                energy -= SHIELD_ENERGY_COST * Time.deltaTime;
                shieldUp();
            }
            else {
                shieldDown();
            }
        }

        public void takeDamage(float damage, GameObject attacker) {
            if (!isShielding) {
                if (health - damage < 0) {
                    attacker.GetComponent<PlayerManager>().addKill();
                    addDeath();

                    health = FULL_HEALTH;
                    playerController.rb.velocity = Vector3.zero;
                    playerController.rb.angularVelocity = Vector3.zero;
                    gameManager.respawn(this.gameObject, actorNum);
                } else {
                    health -= damage;
                }
            }  
        }

        public void addKill() {
            int kills = int.Parse(playerScore["Kills"]);
            kills += 1;
            playerScore["Kills"] = kills.ToString();
        }

        public void addDeath() {
            int deaths = int.Parse(playerScore["Deaths"]);
            deaths += 1;
            playerScore["Deaths"] = deaths.ToString();
        }

        public string getPlayerID() {
            return playerID;
        }

        public Dictionary<string, string> getPlayerScore() {
            return playerScore;
        }

        public void setCamera(GameObject cam) {
            playerCamera = cam;
        }

        #endregion

        #region Private Methods

        void updateDisplay() {
            gameDisplay[0].enabled = !isShielding;
            gameDisplay[1].enabled = isShielding;

            gameDisplay[2].enabled = !isFiring;
            gameDisplay[3].enabled = isFiring;

            Dictionary<string, Dictionary<string, string>> scoreboard = gameManager.getScoreboard();
            
            scoreDisplay[0].text = "Player Name:";
            scoreDisplay[1].text = "Kills:";
            scoreDisplay[2].text = "Deaths";

            foreach (string playerID in scoreboard.Keys) {
                Dictionary<string, string> playerScore = scoreboard[playerID];
                foreach (string field in playerScore.Keys) {
                    scoreDisplay[fieldIndex[field]].text += "\n" + playerScore[field];
                }
            }
        }

        void initializeScore() {
            playerScore["Nickname"] = PhotonNetwork.LocalPlayer.NickName;
            playerScore["Kills"] = "0";
            playerScore["Deaths"] = "0";
        }

        void leaveGame() {
            PlayerController.CONTROLS_ENABLED = false;
            GameObject.Find("Game Manager").GetComponent<GameManager>().LeaveRoom();
        }

        void shieldUp() {
            if (!isShielding) {
                shieldObject.SetActive(true);
                isShielding = true;
            }
        }

        void shieldDown() {
            if (isShielding) {
                shieldObject.SetActive(false);
                isShielding = false;
            }
        }

        #endregion
    }
}
