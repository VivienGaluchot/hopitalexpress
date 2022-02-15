using UnityEngine;
using UnityEngine.UI;

public class ContentItemController : MonoBehaviour {

    private string path;
    private GameObject prefab;

    public void SetInformations(GameObject prefab, string path) {
        this.path = path;
        this.prefab = prefab;

        Transform SpriteTransform = prefab.transform.Find("Sprite");
        if(!SpriteTransform) {
            Debug.LogWarning("NO 'SPRITE' CHILD FOUND FROM " + path);
            return;
        }

        SpriteRenderer sr = SpriteTransform.GetComponent<SpriteRenderer>();
        if (!sr) {
            Debug.LogWarning("UNABLE TO LOAD SPRITE FROM " + path);
            return;
        }

        GetComponent<Image>().sprite = sr.sprite;
    }

    public void SendInformations() {
        TreatmentEditorController.instance.NewFollower(prefab, path);
    }
}
