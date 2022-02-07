using UnityEngine;
using UnityEngine.UI;

public class ContentItemController : MonoBehaviour {

    private string path;
    private GameObject prefab;

    public void SetInformations(GameObject prefab, string path) {
        this.path = path;
        this.prefab = prefab;
        GetComponent<Image>().sprite = prefab.GetComponent<SpriteRenderer>().sprite;
    }

    public void SendInformations() {
        TreatmentEditorController.instance.NewFollower(prefab, path);
    }
}
