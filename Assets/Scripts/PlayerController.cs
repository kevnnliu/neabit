using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.tuth.neabit {
    public class PlayerController : MonoBehaviour {

        #region Private Serialize Fields

        [SerializeField]
        Rigidbody rb;

        [SerializeField]
        float maxThrust;

        [SerializeField]
        float thrust;

        [SerializeField]
        float thrustRate;

        [SerializeField]
        float maxVThrust;

        [SerializeField]
        float vThrust;

        [SerializeField]
        float vThrustRate;

        [SerializeField]
        float maxYaw;

        [SerializeField]
        float yaw;

        [SerializeField]
        float yawRate;

        [SerializeField]
        float maxRoll;

        [SerializeField]
        float roll;

        [SerializeField]
        float rollRate;

        [SerializeField]
        float maxPitch;

        [SerializeField]
        float pitch;

        [SerializeField]
        float pitchRate;

        #endregion

        #region Private Fields

        PlayerManager playerManager;

        #endregion

        public static bool KEYBOARD_CONTROL = true;

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

            CrossPlatformInputs movementInputs = GetInputs();
        }
        
        #endregion

        #region Private Methods

        struct CrossPlatformInputs {
            bool isThrusting { get; }
            float isVThrusting { get; }
            float isRolling { get; }
            float isPitching { get; }
            float isYawing { get; }
            public CrossPlatformInputs(bool iT, float iV, float iR, float iP, float iY) {
                isThrusting = iT;
                isVThrusting = iV;
                isRolling = iR;
                isPitching = iP;
                isYawing = iY;
            }
        }

        CrossPlatformInputs GetInputs() {
            bool iT = KEYBOARD_CONTROL ? Input.GetKey(KeyCode.Space) : Input.GetAxis("Oculus_CrossPlatform_PrimaryHandTrigger") > 0.3f;
            float iV = KEYBOARD_CONTROL ? Input.GetAxis("VerticalAlt") : Input.GetAxis("Oculus_CrossPlatform_SecondaryThumbstickVertical");
            float iR = KEYBOARD_CONTROL ? Input.GetAxis("Horizontal") : Input.GetAxis("Oculus_CrossPlatform_PrimaryThumbstickHorizontal");
            float iP = KEYBOARD_CONTROL ? Input.GetAxis("Vertical") : Input.GetAxis("Oculus_CrossPlatform_PrimaryThumbstickVertical");
            float iY = KEYBOARD_CONTROL ? Input.GetAxis("HorizontalAlt") : Input.GetAxis("Oculus_CrossPlatform_SecondaryThumbstickHorizontal");
            return new CrossPlatformInputs(iT, iV, iR, iP, iY);
        }

        #endregion
    }
}
