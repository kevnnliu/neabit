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

        [SerializeField]
        GameObject pointer;

        #endregion

        #region Monobehaviour Callbacks

        void Start() {
            if (GameObject.FindGameObjectWithTag("Menu") != null) {
                pointer.SetActive(true);
            }
        }

        void FixedUpdate() {
            RaycastHit hit;
            if (Physics.Raycast(rightController.transform.position, rightController.transform.forward, out hit, 100f)) {
                if (hit.transform.gameObject.CompareTag("Button")) {
                    Button button = hit.transform.GetComponent<Button>();
                    if (OVRInput.Get(OVRInput.Axis1D.SecondaryIndexTrigger) > 0.3f && button.interactable) {
                        Debug.Log("Selected button: " + button.name);
                        button.onClick.Invoke();
                    }
                }
            }
        }

        #endregion
    }

}