using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

namespace com.tuth.neabit {
    public class PlayerManager : MonoBehaviourPunCallbacks {

        #region Static Fields

        static bool ROTATIONAL_AIM_COMPENSATION = true;

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
        Slider healthMeter;

        [SerializeField]
        Text[] gameDisplay;

        [SerializeField]
        Text[] scoreDisplay;

        PlayerController playerController;
        GameManager gameManager;
        int actorNum;
        float laserTimeout = 0;
        float energy;
        float health = 100f;
        float regenTimer = 0;
        bool isFiring = false;
        bool isShielding = false;
        bool isBoosting = false;
        GameObject playerCamera;
        string playerID;
        Quaternion laserRotationalCompensation;

        #endregion

        #region Constants

        const float TOTAL_ENERGY = 1;
        const float LASER_ENERGY_COST = 0.02f;
        const float SHIELD_ENERGY_COST = 0.32f;
        const float BOOST_ENERGY_COST = 0.26f;
        const float LASER_FIRERATE = 0.16f;
        const float ENERGY_CHARGE_RATE = 0.14f;
        const float REGEN_DELAY = 0.4f;
        const float FULL_HEALTH = 100f;
        const float LASER_COMPENSATION_COEFF = 0.12f;

        #endregion

        #region Public Fields

        public static GameObject LocalPlayerInstance;
        public bool CONTROLS_ENABLED = true;
        public GameInfoDisplay gameInfoDisplay;
        public GameObject gameDisplayContainer;
        public GameObject scoreDisplayContainer;
        public AudioSource boostSound;
        public AudioSource hitmarkerSound;

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
            gameManager = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameManager>();
            actorNum = PhotonNetwork.LocalPlayer.ActorNumber;

            if (photonView.IsMine) {
                Debug.Log("I am player number " + actorNum + " with username " + PhotonNetwork.LocalPlayer.NickName);
                playerID = PhotonNetwork.LocalPlayer.UserId;
                gameManager.AddPlayerScoreRPCCall(playerID, initializeScore());
            }

            energy = TOTAL_ENERGY;

            playerController.enabled = photonView.IsMine;
        }

        // Update is called once per frame
        void Update() {
            if (!photonView.IsMine) {
                if (playerCamera != null) {
                    playerCamera.SetActive(false);
                }
                return;
            }

            regenTimer = Mathf.Max(regenTimer - Time.deltaTime, 0);

            if (regenTimer == 0)
            {
                energy += ENERGY_CHARGE_RATE * Time.deltaTime;
            }
            energy = Mathf.Clamp(energy, 0, TOTAL_ENERGY);

            energyMeter.value = energy / TOTAL_ENERGY;
            healthMeter.value = health / FULL_HEALTH;

            updateDisplay();

            if (isBoosting && !boostSound.isPlaying) {
                boostSound.Play();
            }
            else if (!isBoosting && boostSound.isPlaying) {
                boostSound.Stop();
            }

            if (laserTimeout > 0) {
                laserTimeout -= Time.deltaTime;
            }

            if (isFiring && laserTimeout <= 0) {
                foreach (Transform shootFrom in shootFroms) {
                    laserTimeout = LASER_FIRERATE;
                    Quaternion compensatedAngle = shootFrom.rotation * laserRotationalCompensation;
                    Quaternion laserAngle = ROTATIONAL_AIM_COMPENSATION ? compensatedAngle : shootFrom.rotation;

                    GameObject laser = PhotonNetwork.Instantiate(laserPrefab.name, shootFrom.position, laserAngle, 0);
                    Laser laserComp = laser.GetComponent<Laser>();
                    laserComp.setOwner(this.gameObject);
                    energy -= LASER_ENERGY_COST;
                }
                regenTimer = REGEN_DELAY;
                isFiring = false;
            }

            if (photonView.IsMine) {
                playerID = PhotonNetwork.LocalPlayer.UserId;
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
                regenTimer = REGEN_DELAY;
                if (!isShielding) {
                    photonView.RPC("ShieldUp", RpcTarget.All);
                    isShielding = true;
                }
            }
            else if (isShielding) {
                regenTimer = REGEN_DELAY;
                photonView.RPC("ShieldDown", RpcTarget.All);
                isShielding = false;
            }
        }

        public void boost(bool amBoosting)
        {
            if (energy > 0 && amBoosting)
            {
                energy -= BOOST_ENERGY_COST * Time.deltaTime;
                regenTimer = REGEN_DELAY;
                isBoosting = true;
            }
            else
            {
                isBoosting = false;
            }
        }

        public void takeDamage(float damage, PlayerManager player, PlayerManager attacker) {
            if (!isShielding) {
                if (health - damage <= 0) {
                    player.AddToFieldRPCCall("Deaths", 1);
                    attacker.AddToFieldRPCCall("Kills", 1);
                    photonView.RPC("Respawn", RpcTarget.All);
                } else {
                    photonView.RPC("TakeDamageRPC", RpcTarget.All, damage);
                }
            }  
        }

        public void setCamera(GameObject cam) {
            playerCamera = cam;
        }

        public void AddToFieldRPCCall(string fieldName, int amount) {
            photonView.RPC("AddToField", RpcTarget.All, fieldName, amount);
        }

        #endregion

        #region Private Methods

        [PunRPC]
        void TakeDamageRPC(float damage) {
            health -= damage;
        }

        [PunRPC]
        void AddToField(string fieldName, int amount) {
            if (photonView.IsMine) {
                Dictionary<string, string> entry = gameManager.getScoreboard()[playerID];
                int value = int.Parse(entry[fieldName]) + amount;
                gameManager.UpdateScoreboardRPCCall(playerID, fieldName, value.ToString());
            }
        }

        [PunRPC]
        void Respawn() {
            health = FULL_HEALTH;
            if (photonView.IsMine) {
                Transform spawn = gameManager.playerStarts[PhotonNetwork.LocalPlayer.ActorNumber];
                transform.position = spawn.position;
                transform.rotation = spawn.rotation;
            }
        }

        void updateDisplay() {
            gameDisplay[0].enabled = !isShielding && !isFiring;
            gameDisplay[1].enabled = isShielding;

            gameDisplay[2].enabled = !isFiring && !isShielding && !isBoosting;
            gameDisplay[3].enabled = isFiring;

            gameDisplay[4].enabled = !isBoosting && !isFiring;
            gameDisplay[5].enabled = isBoosting;

            Dictionary<string, Dictionary<string, string>> scoreboard = gameManager.getScoreboard();
            
            scoreDisplay[0].text = "Player Name";
            scoreDisplay[1].text = "Kills";
            scoreDisplay[2].text = "Deaths";

            foreach (string playerID in scoreboard.Keys) {
                Dictionary<string, string> playerScore = scoreboard[playerID];
                foreach (string field in playerScore.Keys) {
                    scoreDisplay[fieldIndex[field]].text += "\n" + playerScore[field];
                }
            }
            
        }

        Dictionary<string, string> initializeScore() {
            Dictionary<string, string> playerScore = new Dictionary<string, string>();
            playerScore["Nickname"] = PhotonNetwork.LocalPlayer.NickName;
            playerScore["Kills"] = "0";
            playerScore["Deaths"] = "0";
            return playerScore;
        }

        void leaveGame() {
            PlayerController.CONTROLS_ENABLED = false;
            GameObject.Find("Game Manager").GetComponent<GameManager>().LeaveRoom();
        }

        [PunRPC]
        void ShieldUp() {
            shieldObject.SetActive(true);
        }

        [PunRPC]
        void ShieldDown() {
            shieldObject.SetActive(false);
        }

        #endregion
    }
}
