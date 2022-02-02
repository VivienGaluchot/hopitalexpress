using UnityEngine;

public class PopulateGrid : MonoBehaviour {

    public GameObject prefab;
    public TreatmentEditorController ec;

    private void Start() {
        Populate();
    }

    private void Populate() {
        GameObject go;

        Sprite[] Sprites = Resources.LoadAll<Sprite>("Illustrations/Traitements/Items");
        foreach (Sprite sprite in Sprites) {
            go = Instantiate(prefab, transform);
            go.GetComponent<ContentItemController>().Display(ec, sprite, sprite.name, "EditorPrefabs/Traitements/" + sprite.name);
        }
        Sprites = Resources.LoadAll<Sprite>("Illustrations/Traitements/Machines");
        foreach (Sprite sprite in Sprites) {
            go = Instantiate(prefab, transform);
            go.GetComponent<ContentItemController>().Display(ec, sprite, sprite.name, "EditorPrefabs/Traitements/" + sprite.name);
        }
    }
}
