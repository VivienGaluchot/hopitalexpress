using UnityEngine;
using UnityEngine.UI;

public class ContentItemController : MonoBehaviour {

    private TreatmentEditorController ec;
    private string path;
    private Sprite sprite;

    public void SetInformations(TreatmentEditorController edCo, Sprite sprite, string path) {
        ec = edCo;
        this.path = path;
        this.sprite = sprite;
        GetComponent<Image>().sprite = sprite;
    }

    public void SendInformations() {
        ec.NewFollower(sprite, path);
    }
}
