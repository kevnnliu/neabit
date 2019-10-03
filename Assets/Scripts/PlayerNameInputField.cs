using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;

namespace com.tuth.neabit {
    [RequireComponent(typeof(InputField))]
    public class PlayerNameInputField : MonoBehaviour {

        #region Private Constants

        const string playerNamePrefKey = "PlayerName";

        #endregion

        #region MonoBehaviour CallBacks

        // Start is called before the first frame update
        void Start() {

            string defaultName = string.Empty;
            InputField _inputField = this.GetComponent<InputField>();
            if (_inputField != null) {
                if (PlayerPrefs.HasKey(playerNamePrefKey)) {
                    defaultName = PlayerPrefs.GetString(playerNamePrefKey);
                    _inputField.text = defaultName;
                }
            }

            PhotonNetwork.NickName = defaultName;
        }

        // Update is called once per frame
        void Update() {
            
        }

        #endregion

        #region Public Methods

        public void SetPlayerName(string value) {

            if (string.IsNullOrEmpty(value)) {
                Debug.Log("Player name is null or empty");
                return;
            }
            PhotonNetwork.NickName = value;

            PlayerPrefs.SetString(playerNamePrefKey, value);
        }

        #endregion
    }
}
