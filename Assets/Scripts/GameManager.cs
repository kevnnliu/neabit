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
        RaiseEventOptions startRaceEventOptions;
        SendOptions startRaceSendOptions;

        [SerializeField]
        int launcherBuildIndex;

        Room currentRoom;

        #endregion

        #region MonoBehaviour CallBacks

        void Start() {
            startRaceEventCode = 1;
            startRaceEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All };
            startRaceSendOptions = new SendOptions { Reliability = true };

            Instance = this;
            if (playerPrefab == null) {
                Debug.LogError("<Color=Red><a>Missing</a></Color> playerPrefab reference", this);
            }

            currentRoom = PhotonNetwork.CurrentRoom;
            startCountdown = 6;

            spawnPlayer();
        }

        void Update() {

            Debug.Log("Current: " + currentRoom.Players.Count + ", Max: " + currentRoom.MaxPlayers);
            if (currentRoom.PlayerCount == currentRoom.MaxPlayers) {
                isGame = true;
            };

            if (isGame && startCountdown > 0) {
                startCountdown -= Time.deltaTime;
                Debug.Log(startCountdown);
                if (startCountdown <= 0) {
                    startRace();
                }
            }
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

            switch (eventCode) {
                case 1:
                    // start race
                    PlayerManager.LocalPlayerInstance.GetComponent<PlayerController>().CONTROLS_ENABLED = true;
                break;
                default:
                    // do nothing
                break;
            }
        }

        public override void OnEnable() {
            PhotonNetwork.NetworkingClient.EventReceived += OnEvent;
        }

        public override void OnDisable() {
            PhotonNetwork.NetworkingClient.EventReceived -= OnEvent;
        }   

        #endregion

        #region Private Methods

        void spawnPlayer() {
            int actorNum = PhotonNetwork.LocalPlayer.ActorNumber;
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

        void startRace() {
            PhotonNetwork.RaiseEvent(startRaceEventCode, null, startRaceEventOptions, startRaceSendOptions);
        }

        #endregion
    }
}
