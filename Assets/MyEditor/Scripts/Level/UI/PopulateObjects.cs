using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Linq;

public class PopulateObjects : MonoBehaviour {

    [SerializeField] private GameObject Prefab;

    void Start() {
        Populate();
    }

    private void Populate() {
        foreach(string s in LevelEditorController.instance.objectsPath) {
            GameObject[] Prefabs = Resources.LoadAll<GameObject>(s);
            foreach (GameObject p in Prefabs) {
                GameObject go = Instantiate(Prefab, transform); 

                SpriteRenderer sr = p.GetComponent<SpriteRenderer>();
                if (sr == null)
                    sr = p.GetComponentInChildren<SpriteRenderer>();
                go.transform.GetChild(0).GetComponent<Image>().sprite = sr.sprite;

                string path = Path.Combine(s, p.name);
                go.GetComponent<ObjectController>().SetInformations(sr.sprite, path, s.Split('/').Last() == "Seats");
            }
        }        
    }
}
