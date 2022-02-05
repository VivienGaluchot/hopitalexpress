using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelGridController : MonoBehaviour {
    public static LevelGridController instance;

    private void Awake() {
        instance = this;
    }
}
