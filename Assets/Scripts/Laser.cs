using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Laser : NetworkBehaviour
{
    public float damage;
    public float laserSpeed;
    public bool tracking = false;
    public GameObject trackedTarget;
    
    [SyncVar]
    public string owner;

    private float lifeTime;

    // Start is called before the first frame update
    void Start()
    {
        lifeTime = 2f;
    }

    // Update is called once per frame
    void Update()
    {
        if (this.isLocalPlayer) {
            Debug.Log("local");
        }
        lifeTime -= Time.deltaTime;
        if (lifeTime <= 0) {
            Destroy(this.gameObject);
        }
        RaycastHit hit;
        if (Physics.Raycast(transform.position - (transform.forward * 4), transform.forward, 
                                out hit, laserSpeed * Time.fixedDeltaTime)) {
            Transform tr = hit.transform;
            if (tr.tag == "Player") {
                if (tr.gameObject.GetComponent<PlaneControl>()._ID != owner) {
                    tr.gameObject.GetComponent<PlaneControl>().CmdPlayerShot(damage, this.gameObject);
                    Debug.Log("Damaged " + tr.gameObject.GetComponent<PlaneControl>()._ID);
                }
            } else {
                CmdDestroy(this.gameObject);
            }
        }
        if (tracking) {
            Vector3 trackDir = Vector3.Normalize(trackedTarget.transform.position - transform.position);
            transform.position += trackDir * laserSpeed * Time.deltaTime;
            transform.rotation = Quaternion.LookRotation(trackDir);
        } else {
            transform.position += transform.forward * laserSpeed * Time.deltaTime;
            transform.rotation = Quaternion.LookRotation(transform.forward);
        }
        if (isServer) {
            RpcLaserUpdate(transform.position, transform.rotation);
        }
    }

    [ClientRpc]
    void RpcLaserUpdate(Vector3 position, Quaternion rotation) {
        transform.position = position;
        transform.rotation = rotation;
    }

    [Command]
    public void CmdDestroy(GameObject go) {
        NetworkServer.Destroy(go);
    }
}
