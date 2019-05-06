using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUI : MonoBehaviour
{

    public GameObject plane;
    public Text text;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (transform.GetComponentInParent<PlaneControl>().isLocalPlayer)
        {
            text.text = plane.GetComponent<PlaneControl>()._ID;
        }
    }
}
