using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.tuth.neabit {
    public class PlayerController : MonoBehaviour {

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

        #endregion

        #region Private Fields

        PlayerManager playerManager;

        #endregion

        #region MonoBehaviour CallBacks

        // Start is called before the first frame update
        void Start() {
            playerManager = GetComponent<PlayerManager>();
            rb = GetComponent<Rigidbody>();
        }

        // Update is called once per frame
        void Update() {
            if (Input.GetKeyDown(KeyCode.M)) {
                playerManager.fire();
            }
        }

        void FixedUpdate() {
            
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
        }

        #endregion
    }
}
