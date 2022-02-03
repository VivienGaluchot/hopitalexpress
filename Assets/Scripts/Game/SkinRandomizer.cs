using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkinRandomizer : MonoBehaviour
{
    void Start() {
        randomize();
    }

    void randomize() {
        var manager = GetComponent<SkinManager>();
        manager.skinSelected = Random.Range(manager.skinMin, manager.skinMax + 1);
    }
}
