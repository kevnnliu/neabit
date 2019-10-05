using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Events;
using Photon.Pun;
using Photon.Realtime;

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

        [SerializeField]
        UnityEvent prepRace;
        
        [SerializeField]
        UnityEvent startRace;

        Room currentRoom;

        #endregion

        #region MonoBehaviour CallBacks

        void Start() {
            if (prepRace == null) {
                Debug.LogError("prepRace() event is <Color=Red><a>null</a></Color>", this);
                prepRace = new UnityEvent();
            }

            if (startRace == null) {
                Debug.LogError("startRace() event is <Color=Red><a>null</a></Color>", this);
                startRace = new UnityEvent();
            }

            Instance = this;
            if (playerPrefab == null) {
                Debug.LogError("<Color=Red><a>Missing</a></Color> playerPrefab reference", this);
            }
            else {
                if (PlayerManager.LocalPlayerInstance == null) {
                    Debug.LogFormat("Instantiating local player from {0}", SceneManagerHelper.ActiveSceneName);
                    PhotonNetwork.Instantiate(this.playerPrefab.name, new Vector3(0f, 5f, 0f), Quaternion.Euler(-90f, 0f, 0f), 0);
                }
                else {
                    Debug.LogFormat("Ignoring scene load for {0}", SceneManagerHelper.ActiveSceneName);
                }
            }

            currentRoom = PhotonNetwork.CurrentRoom;
            startCountdown = 6;

            // do we start the race?
            if (currentRoom.Players.Count == currentRoom.MaxPlayers) {
                prepareNewRace();
            }
        }

        void Update() {
            if (isGame && startCountdown > 0) {
                startCountdown -= Time.deltaTime;
                if (startCountdown <= 0) {
                    startNewRace();
                }
            }
        }

        #endregion

        #region Photon Callbacks

        public override void OnLeftRoom() {
            SceneManager.LoadScene(0);
        }

        #endregion

        #region Public Methods

        public void LeaveRoom() {
            Debug.Log("Leaving room " + PhotonNetwork.CurrentRoom);
            PhotonNetwork.LeaveRoom();
        }

        #endregion

        #region Private Methods

        void prepareNewRace() {
            isGame = true;

            if (prepRace != null) {
                prepRace.Invoke();
            } else {
                Debug.Log("Event prepRace is null!");
            }
        }

        void startNewRace() {
            if (startRace != null) {
                startRace.Invoke();
            }
        }

        #endregion
    }
}
