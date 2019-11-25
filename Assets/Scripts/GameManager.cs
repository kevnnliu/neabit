using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;


namespace com.tuth.neabit {
    public class GameManager : MonoBehaviourPunCallbacks {

        #region Public Fields

        public static GameManager Instance;
        public GameObject playerPrefab;
        public Transform[] playerStarts;
        public bool isGame = false;
        public float startCountdown;

        #endregion

        #region Private Fields

        byte startRaceEventCode;
        byte countdownStartEventCode;
        RaiseEventOptions startRaceEventOptions;
        SendOptions startRaceSendOptions;
        Dictionary<string, Dictionary<string, string>> scoreboard = new Dictionary<string, Dictionary<string, string>>();

        [SerializeField]
        int launcherBuildIndex;

        Room currentRoom;
        #endregion

        #region MonoBehaviour CallBacks

        void Awake() {
            PhotonNetwork.NetworkingClient.EventReceived += OnEvent;
        }

        void Start() {
            startRaceEventCode = 1;
            countdownStartEventCode = 2;
            startRaceEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All };
            startRaceSendOptions = new SendOptions { Reliability = true };

            Instance = this;
            if (playerPrefab == null) {
                Debug.LogError("<Color=Red><a>Missing</a></Color> playerPrefab reference", this);
            }

            currentRoom = PhotonNetwork.CurrentRoom;
            startCountdown = 6;

            spawnPlayer(PhotonNetwork.LocalPlayer.ActorNumber);

            Debug.Log("Current: " + currentRoom.Players.Count + ", Max: " + currentRoom.MaxPlayers);

            if (currentRoom.PlayerCount == currentRoom.MaxPlayers) {
                isGame = true;
                countdownStartEventCall();
                currentRoom.IsOpen = false;
                currentRoom.IsVisible = false;
            }
        }

        void Update() {

            if (isGame && startCountdown > 0) {
                startCountdown -= Time.deltaTime;
                // Debug.Log(startCountdown);
                if (startCountdown <= 0) {
                    startRaceEventCall();
                    isGame = false;
                }
            }

        }

        void OnDestroy() {
            PhotonNetwork.NetworkingClient.EventReceived -= OnEvent;
        }

        #endregion

        #region Photon Callbacks

        public override void OnLeftRoom() {
            SceneManager.LoadScene(launcherBuildIndex); // goes to launcher
        }

        #endregion

        #region Public Methods

        public void LeaveRoom() {
            Debug.Log("Leaving room " + PhotonNetwork.CurrentRoom);
            PhotonNetwork.LeaveRoom();
        }

        public void OnEvent(EventData photonEvent) {
            byte eventCode = photonEvent.Code;
            GameObject localPlayer = PlayerManager.LocalPlayerInstance;
            object eventData = photonEvent.CustomData;

            switch (eventCode) {
                case 1:
                    // start race
                    PlayerController.CONTROLS_ENABLED = true;
                    Debug.Log("Controls enabled");
                break;
                case 2:
                    // start countdown
                    localPlayer.GetComponent<PlayerManager>().gameInfoDisplay.beginCountdown(startCountdown);
                    Debug.Log("Countdown begin");
                break;
                default:
                    // do nothing
                break;
            }
        }

        public Dictionary<string, Dictionary<string, string>> getScoreboard() {
            return scoreboard;
        }

        public void UpdateScoreboardRPCCall(string id, string field, string value) {
            photonView.RPC("UpdateScoreboard", RpcTarget.AllBufferedViaServer, id, field, value);
        }

        public void AddPlayerScoreRPCCall(string id, Dictionary<string, string> playerScore) {
            photonView.RPC("AddPlayerScore", RpcTarget.AllBufferedViaServer, id, playerScore);
        }

        #endregion

        #region Private Methods

        [PunRPC]
        void UpdateScoreboard(string id, string field, string value) {
            scoreboard[id][field] = value;
        }

        [PunRPC]
        void AddPlayerScore(string id, Dictionary<string, string> playerScore) {
            scoreboard[id] = playerScore;
        }

        void spawnPlayer(int actorNum) {
            Vector3 spawnPos = playerStarts[actorNum].position;
            Quaternion spawnRot = playerStarts[actorNum].rotation;

            if (PlayerManager.LocalPlayerInstance == null) {
                Debug.LogFormat("Instantiating local player from {0}", SceneManagerHelper.ActiveSceneName);
                PhotonNetwork.Instantiate(this.playerPrefab.name, spawnPos, spawnRot, 0);
            }
            else {
                Debug.LogFormat("Ignoring scene load for {0}", SceneManagerHelper.ActiveSceneName);
            }
        }

        void startRaceEventCall() {
            PhotonNetwork.RaiseEvent(startRaceEventCode, null, startRaceEventOptions, startRaceSendOptions);
        }

        void countdownStartEventCall() {
            PhotonNetwork.RaiseEvent(countdownStartEventCode, null, startRaceEventOptions, startRaceSendOptions);
        }

        #endregion
    }
}
