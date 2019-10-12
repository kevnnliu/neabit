using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.tuth.neabit {
    public class Checkpoint : MonoBehaviour
    {

        #region Public Fields

        public bool isEnabled = false;
        public bool isEnd = false;
        public int laps = 0;
        public Checkpoint nextCheckpoint;

        #endregion

        #region Monobehaviour Callbacks

        // Start is called before the first frame update
        void Start()
        {
            
        }

        // Update is called once per frame
        void Update()
        {
            GetComponent<MeshRenderer>().enabled = isEnabled;
        }

        #endregion
    }
}
