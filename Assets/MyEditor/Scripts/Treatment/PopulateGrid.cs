using UnityEngine;
using UnityEngine.UI;
using System.IO;

public class PopulateGrid : MonoBehaviour {

    public GameObject Prefab;
    public TreatmentEditorController ec;

    private void Start() {
        Populate();
    }

    private void Populate() {
        foreach(string s in ec.treatmentPaths) {
            GameObject[] Prefabs = Resources.LoadAll<GameObject>(s);
            foreach (GameObject p in Prefabs) {
                GameObject go = Instantiate(Prefab, transform).transform.GetChild(0).gameObject;
                //string path = s + "/" + p.name;
                string path = Path.Combine(s, p.name);
                SpriteRenderer sr = p.GetComponent<SpriteRenderer>();
                if (sr == null)
                    sr = p.GetComponentInChildren<SpriteRenderer>();
                go.GetComponent<ContentItemController>().SetInformations(ec, sr.sprite, path);
            }
        }
        
    }
}
