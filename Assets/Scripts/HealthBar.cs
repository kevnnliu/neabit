using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

public class HealthBar : NetworkBehaviour
{
    public Image healthBar;
    public GameObject pC;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (transform.GetComponentInParent<PlaneControl>().isLocalPlayer)
        {
            healthBar.fillAmount = pC.GetComponent<PlaneControl>().hp / 100;
        }
    }
}
