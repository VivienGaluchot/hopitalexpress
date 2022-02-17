using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeBarController : MonoBehaviour {

    public GameObject fillRect;

    public float loadTime = 3;

    public float unloadTime = .5f;

    public float loadedSize = 1920;


    public float rate = 0;

    public bool isLoading = false;


    void Update() {
        if (isLoading) {
            rate += Time.deltaTime / loadTime;
        } else {
            rate -= Time.deltaTime / unloadTime;
        }
        rate = Mathf.Clamp(rate, 0, 1);
        if (fillRect != null && fillRect.GetComponent<RectTransform>() != null) {
            var y = fillRect.GetComponent<RectTransform>().offsetMax.y;
            fillRect.GetComponent<RectTransform>().offsetMax = Vector2.Lerp(new Vector2(0, y), new Vector2(loadedSize, y), rate);
        }
    }

    public bool IsFull() {
        return rate >= 1 ;
    }
}
