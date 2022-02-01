using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LevelDiseasesController : MonoBehaviour {

    [SerializeField] private Transform parent;

    private GameObject TemplateElement;
    public Dictionary<string, GameObject> Elements { get; private set; }

    private LevelEditorController lec;

    private void Start() {
        Elements = new Dictionary<string, GameObject>();
        TemplateElement = parent.Find("TemplateElement").gameObject;
        lec = GetComponent<LevelEditorController>();
    }

    public void TryAddDisease(string name) {
        lec.StopSelectingSpawns();
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
        lec.StopSelectingSpawns();
        GameObject value;
        Elements.TryGetValue(text.text, out value);
        if(value) {
            Elements.Remove(text.text);
            Destroy(value);
        }       
    }
}
