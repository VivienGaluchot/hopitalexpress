using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PopulateGrid : MonoBehaviour {

    public GameObject prefab;

    private TreatmentEditorController ec;

    private void Start() {
        ec = GameObject.Find("TreatmentEditorController").GetComponent<TreatmentEditorController>();
        Populate();
    }

    private void Populate() {
        GameObject go;

        Sprite[] Sprites = Resources.LoadAll<Sprite>("Illustrations/Objets");
        foreach (Sprite sprite in Sprites) {
            go = Instantiate(prefab, transform);
            go.GetComponent<ContentItemController>().Display(ec, sprite, sprite.name, "EditorPrefabs/" + sprite.name);
        }
        Sprites = Resources.LoadAll<Sprite>("Illustrations/Pills");
        foreach (Sprite sprite in Sprites) {
            go = Instantiate(prefab, transform);
            go.GetComponent<ContentItemController>().Display(ec, sprite, sprite.name, "EditorPrefabs/" + sprite.name);
        }
    }
}
