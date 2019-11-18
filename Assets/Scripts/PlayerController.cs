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
        internal bool boosting;

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
                headTrack = playerRig.headTrack;
                leftTrack = playerRig.leftTrack;
                rightTrack = playerRig.rightTrack;
                GetComponent<PlayerManager>().playerCamera = playerCamera;
                playerRig.anchor = cameraAnchor.transform;
            }

            //
            forces = new Queue<PlayerMovement>();
            forces.Enqueue(new DragForce(this));
            forces.Enqueue(new ThrustForce(this));

            boosting = false;
            stunned = 0;
            rb.maxAngularVelocity = 0;
        }

        // Update is called once per frame
        void Update() {
            // control scheme switch
            if (Input.GetKeyDown(KeyCode.Q)) {
                KEYBOARD_CONTROL = !KEYBOARD_CONTROL;
                Debug.Log("Keyboard controls: " + KEYBOARD_CONTROL);
            }
            if (Input.GetKeyDown(KeyCode.E) || OVRInput.GetDown(OVRInput.Button.PrimaryThumbstick)) {
                ASSISTED_CONTROL = !ASSISTED_CONTROL;
                Debug.Log("Assisted controls: " + ASSISTED_CONTROL);
            }

            // networked firing
            if ((Input.GetKey(KeyCode.M) || getRightIndexTrigger()) && CONTROLS_ENABLED) {
                playerManager.fire();
            }

            // Stun
            stunned = Mathf.Clamp(stunned - Time.deltaTime, 0, 3600);

            boosting = false;

            // Non-networked movement?
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
            if (true)
            {
                float rollCoeff = -inputs.roll * maxRoll * turnSensitivity;
                float yawCoeff = inputs.yaw * maxYaw * turnSensitivity;
                float pitchCoeff = -inputs.pitch * maxPitch * turnSensitivity;
                transform.RotateAround(transform.position, transform.up, rollCoeff * Time.deltaTime);
                transform.RotateAround(transform.position, transform.forward, yawCoeff * Time.deltaTime);
                transform.RotateAround(transform.position, transform.right, pitchCoeff * Time.deltaTime);
                playerRig.anchorYaw = transform.rotation * Quaternion.Euler(-90f, -180f, 0f);
            }
            else
            {
                // Stunned turning
            }
        
        }

        private void OnTriggerEnter(Collider other)
        {
            GameObject obj = other.gameObject;
            if (obj.CompareTag("Boost"))
            {
                if (!boosting && Vector3.Dot(rb.velocity, -obj.transform.forward) < 0)
                {
                    Debug.Log("BOOOOOOST!");
                    boosting = true;
                    forces.Enqueue(new BoostForce(this, obj.transform.forward));
                }
            }
        }

        private void OnCollisionEnter(Collision collision)
        {
            stunned = 0.5f;
            
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

        Vector3 GetPhysicsMovement()
        {
            Vector3 forward = Vector3.Project(rb.velocity, transform.up);
            Vector3 normal = rb.velocity - forward;

            float forwardDrag = inputs.thrust ? dragCoeff / dragFactor : dragCoeff;
            float normalDrag = inputs.thrust ? dragCoeff * dragFactor : dragCoeff;

            float fmag = Mathf.Clamp(forward.magnitude - forwardDrag * Time.deltaTime, 0, Mathf.Infinity);
            float nmag = Mathf.Clamp(normal.magnitude - normalDrag * Time.deltaTime, 0, Mathf.Infinity);

            forward = forward.normalized * fmag;
            normal = normal.normalized * nmag;

            return forward + normal;
        }

        Vector3 GetDesiredMovement()
        {
            Vector3 forward = Vector3.Project(rb.velocity, transform.up);

            if (inputs.thrust)
            {
                float fmag = Mathf.Clamp(forward.magnitude + thrustRate * Time.deltaTime, 0, maxThrust);
                forward = transform.up * fmag;
            }

            return forward;
        }

        Inputs GetMovementInputs() {
            Vector3 leftTouchP = leftTrack.transform.localPosition;
            Vector3 rightTouchP = rightTrack.transform.localPosition;
            Vector3 headP = headTrack.transform.localPosition;

            Vector3 averageHandP = 0.5f * (leftTouchP + rightTouchP);
            Vector3 shoulderOffset = new Vector3(0f, 0.4f, 0f);

            float iB = KEYBOARD_CONTROL ? Input.GetAxis("Horizontal") * 90f : calculateTheta(leftTouchP, rightTouchP, false);
            float iP = KEYBOARD_CONTROL ? Input.GetAxis("Vertical") * 90f : -calculateTheta(averageHandP, headP - shoulderOffset, false);
            float iY = KEYBOARD_CONTROL ? Input.GetAxis("HorizontalAlt") * 90f : calculateTheta(averageHandP, headP - shoulderOffset, true);

            bool iT = KEYBOARD_CONTROL ? Input.GetKey(KeyCode.Space) : getLeftHandTrigger();

            if (float.IsNaN(iB) || float.IsNaN(iP) || float.IsNaN(iY)) {
                return new Inputs(0, 0, 0, false);
            }

            return new Inputs(iB, iP, iY, iT);
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

            public Inputs(float iR, float iP, float iY, bool iT)
            {
                roll = iR;
                pitch = iP;
                yaw = iY;
                thrust = iT;
            }
        }

        #endregion

        #region Public Methods

        #endregion
    }
}
