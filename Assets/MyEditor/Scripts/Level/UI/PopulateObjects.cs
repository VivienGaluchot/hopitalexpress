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
				SpriteRenderer sr = null;
				Vector3 childPos = Vector3.zero;
				for(int i = 0; i < p.transform.childCount; i++) {
					if(p.transform.GetChild(i).name == "Sprite") {
						sr = p.transform.GetChild(i).GetComponent<SpriteRenderer>();
						childPos = p.transform.GetChild(i).localPosition;
						break;
					}
				}
				if(sr == null) {
					Debug.LogWarning(p.name + " error while getting SpriteRenderer from Sprite child");
					continue;
				}

				GameObject go = Instantiate(Prefab, transform);
				go.transform.GetChild(0).GetComponent<Image>().sprite = sr.sprite;

				string path = Path.Combine(s, p.name);
				go.GetComponent<ObjectController>().SetInformations(sr.sprite, path, s.Split('/').Last() == "Seats", childPos);
			}
		}        
	}
}
