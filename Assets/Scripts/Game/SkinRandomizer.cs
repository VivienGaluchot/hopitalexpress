using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkinRandomizer : MonoBehaviour
{
    public int minSelected = 0;
    public int maxSelected = 0;

    void Start() {
        randomize();
    }

    void randomize() {
        GetComponent<SkinManager>().skinSelected = Random.Range(minSelected, maxSelected + 1);
    }
}
