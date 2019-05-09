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
        lifeTime = 3.0f;
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
            transform.position += Vector3.Normalize(trackedTarget.transform.position - transform.position)
                                                     * laserSpeed * Time.deltaTime;
            transform.rotation = Quaternion.LookRotation(transform.forward);
        } else {
            transform.position += transform.forward * laserSpeed * Time.deltaTime;
            transform.rotation = Quaternion.LookRotation(transform.forward);
        }
        if (isServer) {
            RpcUpdateVector(transform.rotation, transform.position);
        }
    }

    [ClientRpc]
    void RpcUpdateVector(Quaternion rotation, Vector3 position) {
        transform.position = position;
        transform.rotation = rotation;
    }

    [Command]
    public void CmdDestroy(GameObject go) {
        NetworkServer.Destroy(go);
    }
}
