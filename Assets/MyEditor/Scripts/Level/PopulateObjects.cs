using UnityEngine;
using UnityEngine.UI;

public class PopulateObjects : MonoBehaviour {

    [SerializeField] private LevelObjectsController loc;
    [SerializeField] private GameObject Prefab;

    void Start() {
        Populate();
    }

    private void Populate() {
        GameObject[] Prefabs = Resources.LoadAll<GameObject>("EditorPrefabs/Objets/");
        foreach(GameObject p in Prefabs) {
            GameObject go = Instantiate(Prefab, transform);
            go.GetComponent<Image>().sprite = p.GetComponentInChildren<SpriteRenderer>().sprite;
            go.GetComponent<ObjectController>().SetInformations(loc, p);
        }
    }

}
