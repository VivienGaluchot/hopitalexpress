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
            string path = Path.Combine(TreatmentEditorController.instance.treatmentPath, p.name);
            go.GetComponent<ContentItemController>().SetInformations(p, path);
        }
    }
}
