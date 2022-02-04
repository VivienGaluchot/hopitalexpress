using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LevelDiseasesController : MonoBehaviour {
    public static LevelDiseasesController instance;

    [SerializeField] private Transform parent;

    private GameObject TemplateElement;
    public Dictionary<string, GameObject> Elements { get; private set; }

    private void Start() {
        instance = this;
        Elements = new Dictionary<string, GameObject>();
        TemplateElement = parent.Find("TemplateElement").gameObject;
    }

    public void TryAddDisease(string name) {
        LevelEditorController.instance.StopSelectingSpawns();
        if (!Elements.ContainsKey(name)) {
            GameObject newGO = Instantiate(TemplateElement, parent);
            newGO.GetComponentInChildren<Text>().text = name;
            newGO.SetActive(true);
            Elements.Add(name, newGO);
        }
    }

    public void DeleteAll() {
        foreach(KeyValuePair<string, GameObject> e in Elements) {
            Destroy(e.Value);
        }
        Elements.Clear();
    }

    public void TryDeleteDisease(Text text) {
        LevelEditorController.instance.StopSelectingSpawns();
        GameObject value;
        Elements.TryGetValue(text.text, out value);
        if(value) {
            Elements.Remove(text.text);
            Destroy(value);
        }       
    }
}
