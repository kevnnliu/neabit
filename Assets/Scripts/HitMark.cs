using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitMark : MonoBehaviour
{

    private float lifeTime;

    // Start is called before the first frame update
    void Start()
    {
        lifeTime = 0.6f;
    }

    // Update is called once per frame
    void Update()
    {
        lifeTime -= Time.deltaTime;
        if (lifeTime < 0) {
            Destroy(this.gameObject);
        }
    }
}
