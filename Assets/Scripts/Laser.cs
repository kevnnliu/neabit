using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Laser : NetworkBehaviour
{
    public float damage;
    public float laserSpeed;
    public string owner;
    
    private Rigidbody rb;
    private float lifeTime;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        lifeTime = 5.0f;
    }

    // Update is called once per frame
    void Update()
    {
        lifeTime -= Time.deltaTime;
        if (lifeTime <= 0) {
            Destroy(this.gameObject);
        }
        RaycastHit hit;
        if (Physics.Raycast(rb.position, transform.forward, out hit, 1.5f * rb.velocity.magnitude * Time.deltaTime)) {
            Transform tr = hit.transform;
            if (tr.tag == "Player") {
                if (tr.gameObject.GetComponent<PlaneControl>()._ID != owner) {
                    tr.gameObject.GetComponent<PlaneControl>().CmdPlayerShot(damage);
                    RpcDestroy(this.gameObject);
                }
            } else {
                RpcDestroy(this.gameObject);
            }
        }
    }

    void FixedUpdate() {
        rb.velocity = transform.forward * laserSpeed;
    }

    [ClientRpc]
    public void RpcDestroy(GameObject go) {
        Destroy(go);
    }
}
