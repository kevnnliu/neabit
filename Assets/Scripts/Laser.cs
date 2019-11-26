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
            if (!photonView.IsMine) {
                return;
            }

            transform.position += transform.forward * boltSpeed * Time.deltaTime;
            lifeSpan -= Time.deltaTime;
            if (lifeSpan < 0) {
                PhotonNetwork.Destroy(this.gameObject);
            }

            RaycastHit hit;
            if (Physics.Raycast(transform.position, transform.forward, out hit, 1f)) {
                if (hit.transform.root.CompareTag("Player")) {
                    // Debug.Log("Laser hit");
                    PlayerManager player = hit.transform.GetComponent<PlayerManager>();
                    PlayerManager attacker = owner.GetComponent<PlayerManager>();
                    if (owner != hit.transform.gameObject) {
                        // Debug.Log("Damage registered");
                        attacker.hitmarkerSound.Play();
                        player.takeDamage(damage, player, attacker);
                    }
                }
                PhotonNetwork.Destroy(this.gameObject);
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
