using UnityEngine;
using UnityEngine.UI;
using System.IO;

public class PopulateGrid : MonoBehaviour {

    public GameObject Prefab;

    private void Start() { Populate(); }

    private void Populate() {
        foreach(string s in TreatmentEditorController.instance.treatmentPaths) {
            GameObject[] Prefabs = Resources.LoadAll<GameObject>(s);
            foreach (GameObject p in Prefabs) {
                GameObject go = Instantiate(Prefab, transform).transform.Find("Sprite").gameObject;
                string path = Path.Combine(s, p.name);
                go.GetComponent<ContentItemController>().SetInformations(p, path);
            }
        }
        
    }
}
