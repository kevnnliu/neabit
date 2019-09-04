﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

namespace com.tuth.neabit {
    public class Launcher : MonoBehaviourPunCallbacks {

        #region Private Serializable Fields

        [SerializeField]
        private byte maxPlayersPerRoom = 4;

        [SerializeField]
        private GameObject controlPanel;

        [SerializeField]
        private GameObject progressLabel;

        #endregion

        #region Private Fields

        string gameVersion = "0.0.1";
        bool isConnecting;

        #endregion

        #region MonoBehaviour CallBacks

        void Awake() {
            PhotonNetwork.AutomaticallySyncScene = true;
        }

        // Start is called before the first frame update
        void Start() {
            progressLabel.SetActive(false);
            controlPanel.SetActive(true);
        }

        // Update is called once per frame
        void Update() {
            
        }

        #endregion
        
        #region Public Methods

        public void Connect() {
            isConnecting = true;
            progressLabel.SetActive(true);
            controlPanel.SetActive(false);

            if (PhotonNetwork.IsConnected) {
                PhotonNetwork.JoinRandomRoom();
            }
            else {
                PhotonNetwork.GameVersion = gameVersion;
                PhotonNetwork.ConnectUsingSettings();
            }
        }

        #endregion

        #region MonoBehaviourPunCallbacks Callbacks

        public override void OnConnectedToMaster() {
            Debug.Log("OnConnectedToMaster() called by PUN");
            if (isConnecting) {
                PhotonNetwork.JoinRandomRoom();
            }
        }

        public override void OnJoinRandomFailed(short returnCode, string message) {
            Debug.Log("OnJoinRandomFailed() called by PUN, no random room available, attempting to create one");

            PhotonNetwork.CreateRoom(null, new RoomOptions {MaxPlayers = maxPlayersPerRoom});
        }

        public override void OnJoinedRoom() {
            Debug.Log("OnJoinedRoom() called by PUN");

            if (PhotonNetwork.CurrentRoom.PlayerCount == 1) {
                Debug.Log("Load 'Room for 1'");

                PhotonNetwork.LoadLevel("Room for 1");
            }
        }

        public override void OnDisconnected(DisconnectCause cause) {
            progressLabel.SetActive(false);
            controlPanel.SetActive(true);
            Debug.LogWarningFormat("OnDisconnect() called by PUN with reason {0}", cause);
        }

        #endregion
    }
}

