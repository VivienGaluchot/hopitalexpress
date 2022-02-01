using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LevelDiseasesController : MonoBehaviour {

    private GameObject TemplateElement;
    private List<string> Elements;

    private void Start() {
        Elements = new List<string>();
        TemplateElement = transform.Find("TemplateElement").gameObject;
    }

    public void TryAddDisease(string name) {
        if(!Elements.Exists(n => n == name)) {
            Elements.Add(name);
            GameObject newGO = Instantiate(TemplateElement, transform);
            newGO.GetComponentInChildren<Text>().text = name;
            newGO.SetActive(true);
        }
    }

    public void TryDeleteDisease(GameObject go) {
        Elements.Remove(go.GetComponentInChildren<Text>().text);
        Destroy(go);        
    }
}
