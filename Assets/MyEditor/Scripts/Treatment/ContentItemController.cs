using UnityEngine;
using UnityEngine.UI;

public class ContentItemController : MonoBehaviour {

    private string path;
    private Sprite sprite;

    public void SetInformations(Sprite sprite, string path) {
        this.path = path;
        this.sprite = sprite;
        GetComponent<Image>().sprite = sprite;
    }

    public void SendInformations() {
        TreatmentEditorController.instance.NewFollower(sprite, path);
    }
}
