using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectController : MonoBehaviour {

    private Sprite sprite;
    private string path;

    public bool isSeat { get; private set; }

    public void SetInformations(Sprite sprite, string path, bool isSeat = false) {
        this.sprite = sprite;
        this.path = path;
        this.isSeat = isSeat;
    }

    public void SendInformations() {
        LevelEditorController.instance.SetFollower(sprite, path, isSeat);
    }
}
