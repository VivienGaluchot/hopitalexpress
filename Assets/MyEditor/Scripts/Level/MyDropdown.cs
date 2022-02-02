using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;

public class MyDropdown : MonoBehaviour {

    [SerializeField] private string path;
    [SerializeField] private LevelDiseasesController ldc;
    [SerializeField] private LevelEditorController lec;

    private GameObject ElementsParent, TemplateElement;
    private float templateHeight;
    private Image Arrow;
    private bool isScrolledDown;

    private List<GameObject> Elements;

    private void Start() {
        isScrolledDown = false;
        Arrow = transform.Find("Arrow").GetComponent<Image>();
        ElementsParent = transform.Find("Elements").gameObject;
        TemplateElement = transform.Find("TemplateElement").gameObject;
        templateHeight = TemplateElement.GetComponent<RectTransform>().sizeDelta.y;
        path = Path.Combine(Application.dataPath, path);
        Elements = new List<GameObject>();
        FillElements();
    }

    private void FillElements() {
        string[] paths = System.IO.Directory.GetFiles(path);
        foreach (string s in paths) {
            if (!s.EndsWith(".meta")) {
                GameObject newElem = Instantiate(TemplateElement, ElementsParent.transform);
                newElem.transform.localPosition = new Vector3(0, templateHeight * 2f *  Elements.Count, 0f);
                newElem.GetComponentInChildren<Text>().text = Path.GetFileNameWithoutExtension(s);
                newElem.SetActive(true);
                Elements.Add(newElem);
            }
        }
    }

    public void ElementClicked(Text text) {
        ldc.TryAddDisease(text.text);
    }

    public void ScrollDropdown() {
        lec.StopSelectingSpawns();
        ElementsParent.SetActive(!isScrolledDown);
        isScrolledDown = !isScrolledDown;
    }

}
