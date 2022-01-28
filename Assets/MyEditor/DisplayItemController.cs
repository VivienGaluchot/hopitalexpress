using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DisplayItemController : MonoBehaviour {

    [SerializeField] private Image DisplaySprite;
    [SerializeField] private Text DisplayTime;

    private EditorController ec;

    public void DisplayInformations(EditorController edCo, Sprite sprite, float time = 0f) {
        ec = edCo;
        DisplaySprite.sprite = sprite;
        DisplayTime.text = time.ToString();
    }

    public string GetSpriteName() {
        return DisplaySprite.sprite.name;
    }
    public void SendInformations() {
        ec.ClickedItem(this);
    }
}
