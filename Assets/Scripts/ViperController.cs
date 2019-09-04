using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

namespace com.tuth.neabit {
    public class ViperController : MonoBehaviourPunCallbacks, IPunObservable {

        #region IPunObservable implementation

        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info) {
            if (stream.IsWriting) {
                stream.SendNext(fired);
                stream.SendNext(energy);
            }
            else {
                this.fired = (bool)stream.ReceiveNext();
                this.energy = (float)stream.ReceiveNext();
            }
        }

        #endregion

        #region Private Serializable Fields

        [SerializeField]
        Rigidbody rb;

        [SerializeField]
        float mainThruster;

        [SerializeField]
        float verticalThruster;

        [SerializeField]
        float pitchRate;

        [SerializeField]
        float rollRate;

        [SerializeField]
        float yawRate;

        [SerializeField]
        GameObject boltPrefab;

        #endregion

        #region Private Fields

        bool fired;

        #endregion

        #region Public Fields

        public float energy = 100f;

        #endregion

        #region MonoBehaviour CallBacks

        // Start is called before the first frame update
        void Start() {
            
        }

        // Update is called once per frame
        void Update() {
            if (Input.GetKeyDown(KeyCode.M)) {
                Instantiate(boltPrefab, transform.position, transform.rotation);
            }
        }

        void FixedUpdate() {
            if (photonView.IsMine == false && PhotonNetwork.IsConnected == true) {
                return;
            }

            #region Movement Inputs

            if (Input.GetKey(KeyCode.Space)) {
                rb.AddRelativeForce(Vector3.up * mainThruster, ForceMode.Acceleration);
            }
            if (Input.GetKey(KeyCode.LeftShift)) {
                rb.AddRelativeForce(Vector3.forward * verticalThruster, ForceMode.Acceleration);
            }
            if (Input.GetKey(KeyCode.LeftControl)) {
                rb.AddRelativeForce(Vector3.back * verticalThruster, ForceMode.Acceleration);
            }
            if (Input.GetKey(KeyCode.W)) {
                rb.AddRelativeTorque(Vector3.left * pitchRate, ForceMode.Acceleration);
            }
            if (Input.GetKey(KeyCode.S)) {
                rb.AddRelativeTorque(Vector3.right * pitchRate, ForceMode.Acceleration);
            }
            if (Input.GetKey(KeyCode.A)) {
                rb.AddRelativeTorque(Vector3.up * rollRate, ForceMode.Acceleration);
            }
            if (Input.GetKey(KeyCode.D)) {
                rb.AddRelativeTorque(Vector3.down * rollRate, ForceMode.Acceleration);
            }
            if (Input.GetKey(KeyCode.Q)) {
                rb.AddRelativeTorque(Vector3.back * pitchRate, ForceMode.Acceleration);
            }
            if (Input.GetKey(KeyCode.E)) {
                rb.AddRelativeTorque(Vector3.forward * pitchRate, ForceMode.Acceleration);
            }

            #endregion
        }

        void OnTriggerEnter(Collider other) {
            if (!photonView.IsMine) {
                return;
            }
            if (other.CompareTag("Bolt")) {
                energy -= 20;
                Destroy(other.gameObject);
            }
        }

        #endregion
    }
}
