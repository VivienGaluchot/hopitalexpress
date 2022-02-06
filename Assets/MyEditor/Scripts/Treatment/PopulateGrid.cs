using UnityEngine;
using UnityEngine.UI;
using System.IO;

public class PopulateGrid : MonoBehaviour {

    public GameObject Prefab;

    private void Start() { Populate(); }

    private void Populate() {
        GameObject[] Prefabs = Resources.LoadAll<GameObject>(TreatmentEditorController.instance.treatmentPath);
        foreach (GameObject p in Prefabs) {
            GameObject go = Instantiate(Prefab, transform).transform.GetChild(0).gameObject;
            string path = Path.Combine("Icons/Traitements", p.name);
            SpriteRenderer sr = p.GetComponent<SpriteRenderer>();
            if (sr == null)
                sr = p.GetComponentInChildren<SpriteRenderer>();
            go.GetComponent<ContentItemController>().SetInformations(sr.sprite, path);
        }
    }
}
