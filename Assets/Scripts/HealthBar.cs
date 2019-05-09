using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

public class HealthBar : NetworkBehaviour
{
    public Image healthBar;
    public GameObject plane;

    public Color baseColor;
    public Color damageColor;

    private float factor = 0;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (transform.GetComponentInParent<PlaneControl>().isLocalPlayer)
        {
            healthBar.fillAmount = plane.GetComponent<PlaneControl>().hp / 100;
            healthBar.color = baseColor * (1 - factor) + damageColor * factor;

            factor = Mathf.Lerp(factor, 0, 1.5f * Time.deltaTime);
        }
    }

    public void TakeDamage()
    {
        factor = 1;
    }
}
