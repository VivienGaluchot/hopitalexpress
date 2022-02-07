using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectController : MonoBehaviour {

    private Sprite sprite;
    private string path;

    public void SetInformations(Sprite sprite, string path) {
        this.sprite = sprite;
        this.path = path;
    }

    public void SendInformations() {
        LevelEditorController.instance.SetFollower(sprite, path);
    }
}
