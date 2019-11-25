using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

namespace com.tuth.neabit {
    public class Laser : MonoBehaviourPun {

        #region Private Fields

        [SerializeField]
        float boltSpeed;

        [SerializeField]
        float lifeSpan;

        [SerializeField]
        float damage = 15f;

        GameObject owner;

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
                    PlayerManager attacker = owner.GetComponent<PlayerManager>();
                    player.addToField("Deaths", 1);
                    attacker.hitmarkerSound.Play();
                    attacker.addToField("Kills", 1);
                    if (owner != hit.transform.gameObject) {
                        player.takeDamage(damage, owner);
                    }
                    PhotonNetwork.Destroy(this.gameObject);
                } else {
                    PhotonNetwork.Destroy(this.gameObject);
                }
            }
        }

	    #endregion

        #region Public Methods

        public void setOwner(GameObject player) {
            owner = player;
        }

        public GameObject getOwner() {
            return owner;
        }

        #endregion
    }
}
