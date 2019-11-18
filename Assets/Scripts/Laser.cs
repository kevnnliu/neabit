using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

namespace com.tuth.neabit {
    public class Laser : MonoBehaviourPun {

        #region Public Fields

        public GameObject owner;
        public float damage = 15f;

        #endregion

        #region Private Fields

        [SerializeField]
        float boltSpeed;

        [SerializeField]
        float lifeSpan;

        #endregion

	    #region MonoBehaviour CallBacks

        // Start is called before the first frame update
        void Start() {

        }

        // Update is called once per frame
        void Update() {
            transform.position += transform.forward * boltSpeed * Time.deltaTime;
            lifeSpan -= Time.deltaTime;
            if (lifeSpan < 0) {
                PhotonNetwork.Destroy(this.gameObject);
            }

            RaycastHit hit;
            if (Physics.Raycast(transform.position, transform.forward, out hit, 1f)) {
                if (hit.transform.root.CompareTag("Player")) {
                    PlayerManager player = hit.transform.GetComponent<PlayerManager>();
                    if (owner != hit.transform.gameObject) {
                        player.takeDamage(damage);
                    }
                    PhotonNetwork.Destroy(this.gameObject);
                } else {
                    PhotonNetwork.Destroy(this.gameObject);
                }
            }
        }

	    #endregion
    }
}
