using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

namespace com.tuth.neabit {
    public class EMPBolt : MonoBehaviourPun {

        #region Public Fields

        public GameObject owner;
        public int drain = 20;

        #endregion

        #region Private Fields

        [SerializeField]
        float boltSpeed;

        #endregion

	    #region MonoBehaviour CallBacks

        // Start is called before the first frame update
        void Start() {
            
        }

        // Update is called once per frame
        void Update() {
            transform.position += transform.up * boltSpeed * Time.deltaTime;
        }

	    #endregion
    }
}
