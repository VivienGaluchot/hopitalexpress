using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;

public class GameLoader : MonoBehaviour {

	[SerializeField] private Transform LevelParent;
	[SerializeField] private Dropdown dd;
	[SerializeField] private string path;

	[SerializeField] private GameObject[] Walls;

	private void Start() {
		path = Path.Combine(Application.dataPath, path);
		FetchDDOptions();
	}

	private void FetchDDOptions() {
		string[] paths = System.IO.Directory.GetFiles(path);
		List<string> pathsList = new List<string>();
		foreach (string s in paths) {
			if (!s.EndsWith(".meta"))
				pathsList.Add(Path.GetFileNameWithoutExtension(s));
		}
		dd.ClearOptions();
		dd.AddOptions(pathsList);
	}

	public void LoadLevel() {
		string filename = dd.options[dd.value].text + ".json";
		LevelData Data = JsonUtility.FromJson<LevelData>(ReadFromFile(filename));

		//foreach(CellData cell in Data.cells) {
		//	GameObject newGO = Instantiate(Walls[cell.value-1], LevelParent);
		//	newGO.transform.position = new Vector3(cell.y/Data.size, -cell.x/Data.size, 0f);
		//}
		Camera.main.transform.position = new Vector3((Data.columns - 1) / 2f / Data.size, (1 - Data.rows) / 2f / Data.size, -10f);

		GetComponent<GameController>().StartGame();
	}

	private string ReadFromFile(string fileName) {
		StreamReader sr = new StreamReader(Path.Combine(path, fileName));
		return sr.ReadToEnd();
	}
}
