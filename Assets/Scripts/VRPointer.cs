using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace com.tuth.neabit {
    public class VRPointer : MonoBehaviour
    {

        #region Private Serialize Fields

        [SerializeField]
        GameObject leftController;

        [SerializeField]
        GameObject rightController;

        #endregion

        #region Monobehaviour Callbacks

        private void FixedUpdate()
        {   
            RaycastHit hit;
            if (Physics.Raycast(leftController.transform.position, leftController.transform.forward, out hit, 10f)) {
                if (hit.transform.gameObject.CompareTag("Button")) {
                    Button button = hit.transform.GetComponent<Button>();
                    button.Select();
                    if (OVRInput.GetDown(OVRInput.Button.SecondaryIndexTrigger, OVRInput.Controller.RTouch)) {
                        button.onClick.Invoke();
                    }
                }
            }
        }

        #endregion
    }

}