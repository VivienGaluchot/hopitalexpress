using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectController : MonoBehaviour {

    private Sprite sprite;

    public void SetInformations(Sprite sprite) {
        this.sprite = sprite;
    }

    public void SendInformations() {
        LevelEditorController.instance.SetFollower(sprite);
    }
}
