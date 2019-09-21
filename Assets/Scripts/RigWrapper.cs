using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

        Vector3 offset;

        #endregion

        #region MonoBehaviour CallBacks

        // Start is called before the first frame update
        void Start() {
            offset = new Vector3(0f, 0f, 0.15f);
            anchorYaw = Quaternion.identity;
        }

        // Update is called once per frame
        void Update() {
            
        }

        void LateUpdate() {
            transform.position = anchor.position;
            if (anchor.parent.GetComponent<PlayerController>().ASSISTED_CONTROL) {
                transform.rotation = anchorYaw;
            } else {
                transform.rotation = anchor.parent.rotation * Quaternion.Euler(-90f, 0f, 180f);
            }
        }

        #endregion
    }
}
