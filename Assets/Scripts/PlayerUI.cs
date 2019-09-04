using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace com.tuth.neabit {
    public class PlayerUI : MonoBehaviour {

        #region Private Fields

        [SerializeField]
        private Text playerNameText;

        [SerializeField]
        private Slider playerEnergySlider;

        private PlayerManager target;

        #endregion

        #region MonoBehaviour CallBacks

        void Awake() {
            this.transform.SetParent(GameObject.Find("Canvas").GetComponent<Transform>(), false);
        }

        // Start is called before the first frame update
        void Start() {
            
        }

        // Update is called once per frame
        void Update() {
            if (playerEnergySlider != null) {
                playerEnergySlider.value = target.energy;
            }
            if (target == null) {
                Destroy(this.gameObject);
                return;
            }
        }

        #endregion

        #region Public Methods

        public void SetTarget(PlayerManager _target) {
            if (_target == null) {
                Debug.LogError("<Color=Red><a>Missing</a></Color> PlayerManager target for PlayerUI.SetTarget", this);
                return;
            }

            target = _target;
            if (playerNameText != null) {
                playerNameText.text = target.photonView.Owner.NickName;
            }
        }

        #endregion
    }
}
