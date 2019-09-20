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
        float thrustRate;

        [SerializeField]
        float maxVThrust;

        [SerializeField]
        float maxYaw;

        [SerializeField]
        float maxRoll;

        [SerializeField]
        float maxPitch;

        #endregion

        #region Private Fields

        PlayerManager playerManager;

        float thrust;
        float vThrustRate;
        float yawRate;
        float rollRate;
        float pitchRate;

        #endregion

        public static bool KEYBOARD_CONTROL = true;
        public static bool ASSISTED_CONTROL = false;

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

            // main thrusting is independent of control scheme
            bool isThrusting = KEYBOARD_CONTROL ? Input.GetKey(KeyCode.Space) : handTriggerAverage() > 0.3f;
            thrust += thrustProcessing(isThrusting, thrustRate);
            thrust = Mathf.Clamp(thrust, 0, maxThrust);

            if (ASSISTED_CONTROL) {
                
                AssistedInputs movementInputs = GetAssistedMovementInputs();



            } else { // manual control scheme

                ManualInputs movementInputs = GetManualMovementInputs();

                // vertical thrusters
                vThrustRate = inputSmoothing(movementInputs.isVThrusting) * maxVThrust;

                // angular floats
                rollRate = inputSmoothing(movementInputs.isRolling) * maxRoll;
                yawRate = inputSmoothing(movementInputs.isYawing) * maxYaw;
                pitchRate = -inputSmoothing(movementInputs.isPitching) * maxPitch; // inverted pitch

                // input output
                transform.Translate( ( (Vector3.up * thrust) + (Vector3.forward * vThrustRate) ) * Time.deltaTime ); // extra spaces for readability
                transform.Rotate(new Vector3(pitchRate, -rollRate, yawRate) * Time.deltaTime); // rollRate needs to be inverted here, probably because prefab rotations

            }
        }

        private void OnCollisionEnter(Collision other)
        {
            rb.angularVelocity = Vector3.zero; // stops violent rotations from collisions
        }
        
        #endregion

        #region Private Methods

        struct ManualInputs {
            public float isVThrusting { get; }
            public float isRolling { get; }
            public float isPitching { get; }
            public float isYawing { get; }
            public ManualInputs(float iV, float iR, float iP, float iY) {
                isVThrusting = iV;
                isRolling = iR;
                isPitching = iP;
                isYawing = iY;
            }
        }

        ManualInputs GetManualMovementInputs() {
            float iV = KEYBOARD_CONTROL ? Input.GetAxis("VerticalAlt") : OVRInput.Get(OVRInput.Axis2D.SecondaryThumbstick).y;
            float iR = KEYBOARD_CONTROL ? Input.GetAxis("Horizontal") : OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick).x;
            float iP = KEYBOARD_CONTROL ? Input.GetAxis("Vertical") : OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick).y;
            float iY = KEYBOARD_CONTROL ? Input.GetAxis("HorizontalAlt") : OVRInput.Get(OVRInput.Axis2D.SecondaryThumbstick).x;
            return new ManualInputs(iV, iR, iP, iY);
        }

        struct AssistedInputs {
            public float isBanking { get; }
            public float isPitching { get; }
            public AssistedInputs(float iB, float iP) {
                isBanking = iB;
                isPitching = iP;
            }
        }

        AssistedInputs GetAssistedMovementInputs() {
            float maxBank = 60;
            Vector3 leftTouchP = OVRInput.GetLocalControllerPosition(OVRInput.Controller.LTouch);
            Vector3 rightTouchP = OVRInput.GetLocalControllerPosition(OVRInput.Controller.RTouch);

            float iB = calculateBank(leftTouchP, rightTouchP, maxBank);
            float iP = calculatePitch(leftTouchP, rightTouchP);

            return new AssistedInputs(iB, iP);
        }

        // input processing, apply polynomial for control smoothing
        float inputSmoothing(float input) {
            return Mathf.Sign(input) * Mathf.Pow(Mathf.Abs(input), 2); // returns squared and signed input
        }

        float thrustProcessing(bool isThrusting, float rate) {
            if (isThrusting) {
                return rate;
            } else {
                return -rate;
            }
        }

        float handTriggerAverage() {
            return 0.5f * ( OVRInput.Get(OVRInput.Axis1D.PrimaryHandTrigger) + OVRInput.Get(OVRInput.Axis1D.SecondaryHandTrigger) );
        }

        // for assisted control scheme
        float calculateBank(Vector3 leftTouchP, Vector3 rightTouchP, float maxBank) {
            float euclidean = Vector3.Distance(leftTouchP, rightTouchP);
            float vDist = leftTouchP.y - rightTouchP.y;

            float angle = Mathf.Asin(vDist / euclidean) * Mathf.Rad2Deg;

            return Mathf.Clamp(angle, -maxBank, maxBank) / maxBank;
        }

        // for assisted control scheme
        float calculatePitch(Vector3 leftTouchP, Vector3 rightTouchP) {
            Vector3 averageP = 0.5f * (leftTouchP + rightTouchP);
            OVRInput.
        }

        #endregion
    }
}
