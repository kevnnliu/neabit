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

            prepRace.AddListener(setUpGame);

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
        }

        void Update() {
            if (currentRoom.Players.Count == currentRoom.MaxPlayers) {
                if (prepRace != null) {
                    prepRace.Invoke();
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

        void setUpGame() {
            
        }

        #endregion
    }
}
