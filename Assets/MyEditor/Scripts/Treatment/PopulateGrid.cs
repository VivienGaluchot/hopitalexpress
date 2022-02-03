using UnityEngine;
using UnityEngine.UI;

public class PopulateGrid : MonoBehaviour {

    public GameObject Prefab;
    public TreatmentEditorController ec;

    private void Start() {
        Populate();
    }

    private void Populate() {
        GameObject[] Prefabs = Resources.LoadAll<GameObject>("EditorPrefabs/Traitement/");
        foreach (GameObject p in Prefabs) {
            GameObject go = Instantiate(Prefab, transform);
            string path = "EditorPrefabs/Traitement/" + p.name;
            go.GetComponent<Image>().sprite = p.GetComponentInChildren<SpriteRenderer>().sprite;
            go.GetComponent<ContentItemController>().SetInformations(ec, p, path);
        }
    }
}
