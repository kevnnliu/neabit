using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace com.tuth.neabit {
    public class PlayerController : MonoBehaviour {

        #region Private Serialize Fields

        [SerializeField]
        GameObject cameraAnchor;

        [SerializeField]
        GameObject rigPrefab;

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

        [SerializeField]
        float turnSensitivity;

        // Physics

        [SerializeField]
        float dragCoeff;

        [SerializeField]
        float dragFactor;

        #endregion

        #region Private Fields

        PlayerManager playerManager;
        
        GameObject headTrack;
        GameObject leftTrack;
        GameObject rightTrack;
        RigWrapper playerRig;
        GameObject playerCamera;
        float yawCoeff;
        float rollCoeff;
        float pitchCoeff;

        Queue<PlayerMovement> forces;


        #endregion

        #region Constants

        const float MAX_BOLT_DISTANCE = 60f;
        const float TRIGGER_THRESHOLD = 0.3f;

        #endregion

        #region Internal Fields

        internal Rigidbody rb;

        internal Inputs inputs;

        internal float stunned;

        #endregion

        #region Public Fields

        public static bool KEYBOARD_CONTROL = false;
        public static bool ASSISTED_CONTROL = true;
        public static bool CONTROLS_ENABLED = false;

        #endregion

        #region MonoBehaviour CallBacks

        // Start is called before the first frame update
        void Start() {
            playerManager = GetComponent<PlayerManager>();
            rb = GetComponent<Rigidbody>();

            // setting up camera rig
            if (PlayerManager.LocalPlayerInstance.Equals(this.gameObject)) {
                playerCamera = Instantiate(rigPrefab, cameraAnchor.transform.position, Quaternion.identity);
                playerRig = playerCamera.GetComponent<RigWrapper>();
                playerRig.setPlayerManager(playerManager);
                headTrack = playerRig.headTrack;
                leftTrack = playerRig.leftTrack;
                rightTrack = playerRig.rightTrack;
                GetComponent<PlayerManager>().setCamera(playerCamera);
                playerRig.anchor = cameraAnchor.transform;
            }

            //
            forces = new Queue<PlayerMovement>();
            forces.Enqueue(new DragForce(this));
            forces.Enqueue(new ThrustForce(this));
            forces.Enqueue(new BoostForce(this));
            
            stunned = 0;
            rb.maxAngularVelocity = 0;
        }

        // Update is called once per frame
        void Update() {

            // control scheme switches
            if (Input.GetKeyDown(KeyCode.Q)) {
                KEYBOARD_CONTROL = !KEYBOARD_CONTROL;
                Debug.Log("Keyboard controls: " + KEYBOARD_CONTROL);
            }
            if (Input.GetKeyDown(KeyCode.E) || OVRInput.GetDown(OVRInput.Button.PrimaryThumbstick)) {
                ASSISTED_CONTROL = !ASSISTED_CONTROL;
                Debug.Log("Assisted controls: " + ASSISTED_CONTROL);
            }

            // Stun
            stunned = Mathf.Clamp(stunned - Time.deltaTime, 0, 3600);

            // movement calculations
            inputs = GetMovementInputs();

            if (CONTROLS_ENABLED) {
                Queue<PlayerMovement> newForces = new Queue<PlayerMovement>();
                foreach (PlayerMovement movement in forces) {
                    if (movement.GetStatus() == Status.ACTIVE)
                    {
                        rb.AddForce(movement.Force());
                    }
                    if (movement.GetStatus() != Status.REMOVE)
                    {
                        newForces.Enqueue(movement);
                    }
                }
                forces = newForces;
            }
                
            // free rotation (unclamped)
            if (true) {
                rollCoeff = -inputs.roll * maxRoll * turnSensitivity;
                yawCoeff = inputs.yaw * maxYaw * turnSensitivity;
                pitchCoeff = -inputs.pitch * maxPitch * turnSensitivity;
                transform.RotateAround(transform.position, transform.up, rollCoeff * Time.deltaTime);
                transform.RotateAround(transform.position, transform.forward, yawCoeff * Time.deltaTime);
                transform.RotateAround(transform.position, transform.right, pitchCoeff * Time.deltaTime);
                playerRig.anchorYaw = transform.rotation * Quaternion.Euler(-90f, -180f, 0f);
            }
            else {
                // Stunned turning
            }
            
            if (CONTROLS_ENABLED && stunned == 0) {
                // Boosting takes priority over all else
                playerManager.boost(inputs.boosting);
                // networked firing
                if (inputs.firing) {
                    Vector3 angularVelocity = new Vector3(pitchCoeff, -yawCoeff, 0f);
                    playerManager.fire(angularVelocity);
                    playerManager.shield(false);
                } // firing/shielding are exclusive actions, but firing takes priority
                else {
                    playerManager.shield(inputs.shielding);
                }
            }
            else
            {
                playerManager.boost(false);
            }
        
        }

        private void OnTriggerEnter(Collider other)
        {
            //GameObject obj = other.gameObject;
            //if (obj.CompareTag("Boost"))
            //{
            //    if (!boosting && Vector3.Dot(rb.velocity, -obj.transform.forward) < 0)
            //    {
            //        Debug.Log("BOOOOOOST!");
            //        boosting = true;
            //        forces.Enqueue(new BoostForce(this, obj.transform.forward));
            //    }
            //}
        }

        private void OnCollisionEnter(Collision collision)
        {
            stunned = 0.4f;
            
            // Vector3 normal = collision.GetContact(0).normal;
            // Vector3 ortho = Vector3.Project(transform.up, normal);
            // Vector3 forward = transform.up - ortho;
            // if (Vector3.Dot(normal, ortho) < 0)
            // {
            //     float turn = 2 * forward.magnitude * ortho.magnitude / Mathf.Sqrt(forward.sqrMagnitude + ortho.sqrMagnitude);
            //     float omag = Mathf.Clamp(ortho.magnitude - turn, 0, Mathf.Infinity);
            //     ortho = ortho.normalized * omag;

            //     Vector3 newUp = forward + ortho;
            //     Quaternion q = Quaternion.LookRotation(Vector3.Cross(transform.right, newUp), newUp);
            //     transform.rotation = q;
            // }
        }

        #endregion

        #region Private Methods

        Inputs GetMovementInputs() {
            Vector3 leftTouchP = leftTrack.transform.localPosition;
            Vector3 rightTouchP = rightTrack.transform.localPosition;
            Vector3 headP = headTrack.transform.localPosition;

            Vector3 averageHandP = 0.5f * (leftTouchP + rightTouchP);
            Vector3 shoulderOffset = new Vector3(0f, 0.4f, 0f);

            float iR = KEYBOARD_CONTROL ? Input.GetAxis("Horizontal") * 90f : calculateTheta(leftTouchP, rightTouchP, false);
            float iP = KEYBOARD_CONTROL ? Input.GetAxis("Vertical") * 90f : -calculateTheta(averageHandP, headP - shoulderOffset, false);
            float iY = KEYBOARD_CONTROL ? Input.GetAxis("HorizontalAlt") * 90f : calculateTheta(averageHandP, headP - shoulderOffset, true);

            bool iT = KEYBOARD_CONTROL ? Input.GetKey(KeyCode.Space) : getLeftHandTrigger();
            bool iB = KEYBOARD_CONTROL ? Input.GetKey(KeyCode.LeftShift) && iT : getRightHandTrigger() && iT;

            bool iF = KEYBOARD_CONTROL ? Input.GetKey(KeyCode.M) : getRightIndexTrigger();
            bool iS = KEYBOARD_CONTROL ? Input.GetKey(KeyCode.N) : getLeftIndexTrigger();

            if (float.IsNaN(iR) || float.IsNaN(iP) || float.IsNaN(iY)) {
                return new Inputs(0, 0, 0, false, false, false, false);
            }

            return new Inputs(iR, iP, iY, iT, iB, iF, iS);
        }

        // input processing, apply polynomial for control smoothing
        float inputSmoothing(float input) {
            return Mathf.Sign(input) * Mathf.Pow(Mathf.Abs(input), 2); // returns squared and signed input
        }

        float thrustProcessing(bool isThrusting, float rate) {
            if (isThrusting) {
                return rate;
            } else {
                return -rate * 2f;
            }
        }

        bool getLeftHandTrigger() {
            return OVRInput.Get(OVRInput.Axis1D.PrimaryHandTrigger) > TRIGGER_THRESHOLD;
        }

        bool getRightHandTrigger() {
            return OVRInput.Get(OVRInput.Axis1D.SecondaryHandTrigger) > TRIGGER_THRESHOLD;
        }

        bool getRightIndexTrigger() {
            return OVRInput.Get(OVRInput.Button.SecondaryIndexTrigger);
        }

        bool getLeftIndexTrigger() {
            return OVRInput.Get(OVRInput.Button.PrimaryIndexTrigger);
        }

        // for assisted control scheme
        float calculateTheta(Vector3 a, Vector3 b, bool yawCalc) {
            float euclidean = Vector3.Distance(a, b);
            float vDist = yawCalc ? a.x - b.x : a.y - b.y;

            float angle = Mathf.Asin(vDist / euclidean) * Mathf.Rad2Deg;

            return Mathf.Clamp(angle, -90f, 90f);
        }

        #endregion

        #region Structures

        public struct Inputs
        {
            public float roll;
            public float pitch;
            public float yaw;

            public bool thrust;
            public bool boosting;

            public bool firing;
            public bool shielding;

            public Inputs(float iR, float iP, float iY, bool iT, bool iB, bool iF, bool iS)
            {
                roll = iR;
                pitch = iP;
                yaw = iY;
                thrust = iT;
                boosting = iB;
                firing = iF;
                shielding = iS;
            }
        }

        #endregion

        #region Public Methods

        #endregion
    }
}
