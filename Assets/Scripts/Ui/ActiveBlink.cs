using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActiveBlink : MonoBehaviour
{
    public float blinkPeriod = 1;
    public bool isBlinking = true;
    public GameObject target;

    private float lastBlink = 0;

    void Update() {
        if (!isBlinking) {
            target.SetActive(true);
        }
        lastBlink += Time.deltaTime;
        if (lastBlink > blinkPeriod){
            if (isBlinking) {
                target.SetActive(!target.activeSelf);
            }
            lastBlink = 0;
        }
    }
}
