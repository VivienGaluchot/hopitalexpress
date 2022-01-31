using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ContentItemController : MonoBehaviour {

    [SerializeField] private Image DisplaySprite;
    [SerializeField] private Text DisplayName;

    private EditorController ec;
    private string path;

    public void Display(EditorController edCo, Sprite sprite, string name, string _path) {
        ec = edCo;
        DisplaySprite.sprite = sprite;
        DisplayName.text = name;
        path = _path;
    }

    public void SendInformations() {
        ec.NewFollower(path);
    }
}
