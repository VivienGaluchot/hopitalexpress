using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PopulateGrid : MonoBehaviour {

    public GameObject prefab;

    private EditorController ec;

    private void Start() {
        ec = GameObject.Find("EditorController").GetComponent<EditorController>();
        Populate();
    }

    private void Populate() {
        GameObject go;

        Sprite[] Sprites = Resources.LoadAll<Sprite>("Sprites/Medoc");
        foreach (Sprite sprite in Sprites) {
            go = Instantiate(prefab, transform);
            go.GetComponent<ContentItemController>().Display(ec, sprite, sprite.name, "Prefabs/Treatments/Items/" + sprite.name);
        }
        Sprites = Resources.LoadAll<Sprite>("Sprites/Machines");
        foreach (Sprite sprite in Sprites) {
            go = Instantiate(prefab, transform);
            go.GetComponent<ContentItemController>().Display(ec, sprite, sprite.name, "Prefabs/Treatments/Machines/" + sprite.name);
        }
    }
}
