using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Laser : NetworkBehaviour
{
    public float damage;
    public float laserSpeed;
    public GameObject owner;
    
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
    }

    void FixedUpdate() {
        rb.velocity = transform.forward * laserSpeed;
    }

    void OnTriggerEnter(Collider col) {
        if (col.tag != "GlowyBois") {
            if (col.tag == "Player") {
                if (col.gameObject != owner) {
                    col.gameObject.GetComponent<PlaneControl>().hp -= damage;
                }
            }
            if (col.gameObject != owner) {
                Destroy(this.gameObject);
            }
        }
    }
}
