using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;

public class DiseasesDropdown : MonoBehaviour {

	[SerializeField] private string path;

	private GameObject ElementsParent, TemplateElement;
	private float templateHeight;
	private bool isScrolledDown;

	private List<GameObject> Elements;

	private void Awake() {
		isScrolledDown = false;
		ElementsParent = transform.Find("Elements").gameObject;
		TemplateElement = transform.Find("TemplateElement").gameObject;
		templateHeight = TemplateElement.GetComponent<RectTransform>().sizeDelta.y;
		path = Path.Combine(Application.dataPath, path);
		FillElements();
	}

	private void FillElements() {
		if (Elements != null) {
			foreach (GameObject go in Elements)
				Destroy(go);
			Elements.Clear();
		} else Elements = new List<GameObject>();

		string[] paths = Directory.GetFiles(path);
		foreach (string s in paths) {
			if (!s.EndsWith(".meta")) {
				GameObject newElem = Instantiate(TemplateElement, ElementsParent.transform);
				newElem.transform.localPosition = new Vector3(0, -templateHeight * Elements.Count, 0f);
				newElem.GetComponentInChildren<Text>().text = Path.GetFileNameWithoutExtension(s);
				newElem.SetActive(true);
				Elements.Add(newElem);
			}
		}
	}

	public void ElementClicked(Text text) {
		LevelDiseasesController.instance.TryAddDisease(text.text);
	}

	public void ScrollDropdown() {
		ElementsParent.SetActive(!isScrolledDown);
		isScrolledDown = !isScrolledDown;
	}

	private void OnEnable() {
		FillElements();
	}
}
