using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace com.tuth.neabit {
    public class RigWrapper : MonoBehaviour {

        #region Public Fields

        public GameObject headTrack;
        public GameObject leftTrack;
        public GameObject rightTrack;
        public Transform anchor;
        public Quaternion anchorYaw;

        #endregion

        #region Private Fields

        [SerializeField]
        Button leaveButton;

        Vector3 offset;
        PlayerManager playerManager;

        #endregion

        #region MonoBehaviour CallBacks

        // Start is called before the first frame update
        void Start() {
            offset = new Vector3(0f, 0f, 0.15f);
            anchorYaw = Quaternion.identity;
        }

        // Update is called once per frame
        void Update() {
            bool showInfo = OVRInput.Get(OVRInput.Button.Start, OVRInput.Controller.LTouch) || Input.GetKey(KeyCode.P);

            updateUI(showInfo);
        }

        void LateUpdate() {
            if (anchor != null) {
                transform.position = anchor.position;
                if (PlayerController.ASSISTED_CONTROL) {
                    transform.rotation = anchorYaw;
                } else {
                    transform.rotation = anchor.parent.rotation * Quaternion.Euler(-90f, 0f, 180f);
                }
            }
        }

        #endregion

        #region Private Methods

        void updateUI(bool showInfo) {
            leaveButton.interactable = showInfo;
            
            if (playerManager != null) {
                playerManager.scoreDisplayContainer.SetActive(showInfo);
                playerManager.gameDisplayContainer.SetActive(!showInfo);
            }
        }

        #endregion

        #region Public Methods

        public void onClickLeave() {
            GameObject gm = GameObject.Find("Game Manager");
            if (gm == null) {
                Debug.Log("Exiting game");
                Application.Quit(); // exit game if in main menu
            } else {
                gm.GetComponent<GameManager>().LeaveRoom();
            }
        }

        public void setPlayerManager(PlayerManager playerManager) {
            this.playerManager = playerManager;
        }

        #endregion
    }
}
