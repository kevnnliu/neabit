using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

namespace com.tuth.neabit {
    public class EMPBolt : MonoBehaviourPun {

        #region Public Fields

        public GameObject owner;
        public Vector3 target;
        public const float drain = 20f;

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
            if (target != null) {
                transform.position += Vector3.Normalize(target - transform.position) * boltSpeed * Time.deltaTime;
                if (Vector3.Magnitude(transform.position - target) < boltSpeed * Time.deltaTime) {
                    Destroy(this.gameObject);
                }
            }
        }

	    #endregion

        public float getDrain() {
            return drain;
        }
    }
}
