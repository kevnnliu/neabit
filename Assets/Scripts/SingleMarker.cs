using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SingleMarker : MonoBehaviour
{
    public GameObject target;
    public float radiusFromCamera;

    private Image marker;

    // Start is called before the first frame update
    void Start()
    {
        marker = GetComponentInChildren<Image>();
    }

    // Update is called once per frame
    void Update()
    {
        if (target == null || target == transform.root.gameObject || target.GetComponent<PlaneControl>().isDead == true) {
            Destroy(this.gameObject);
            Debug.Log(target + " destroyed");
        }

        marker.enabled = true;
        Vector3 cameraPos = transform.root.GetComponent<PlaneControl>().pilotCamera.transform.position;
        Vector3 targetDir = target.transform.position - cameraPos;
        if (target.GetComponentInChildren<Renderer>().isVisible) {
            int layerMask = 1 << 8;
            layerMask = ~layerMask;
            if (!Physics.Raycast(cameraPos, targetDir, targetDir.magnitude, layerMask)) {
                Debug.Log("No Obstacles");
                marker.enabled = true;
            }
        }

        transform.position = cameraPos + (Vector3.Normalize(targetDir) * radiusFromCamera);
        transform.rotation = Quaternion.LookRotation(cameraPos - transform.position);
    }
}
