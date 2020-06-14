using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameInfoDisplay : MonoBehaviour
{

    public Text gameText;

    bool startCounting;
    float count;

    // Start is called before the first frame update
    void Start()
    {
        startCounting = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (startCounting) {
            if (count > 0) {
                count -= Time.deltaTime;
                gameText.text = ((int)Mathf.Ceil(count)).ToString();
            }
            else {
                gameText.text = "Go!";
                gameText.CrossFadeAlpha(0f, 1f, false);
                startCounting = false;
            }
        }
    }

    public void beginCountdown(float startCount) {
        count = startCount;
        startCounting = true;
    }
}
