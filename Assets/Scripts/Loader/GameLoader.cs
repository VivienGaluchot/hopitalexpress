using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;

public class GameLoader : MonoBehaviour {

	[SerializeField] private Dropdown dd;
	[SerializeField] private string path;

	[SerializeField] private Transform FloorParent;
	[SerializeField] private Transform WallsParent;
	[SerializeField] private GameObject[] Walls;
	[SerializeField] private GameObject[] Floor;

	private Transform[] PrefabsParents;
	private GameObject[][] Prefabs;

	private void Start() {
		PrefabsParents = new Transform[2] { FloorParent, WallsParent };
		Prefabs = new GameObject[2][] { Floor, Walls };
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

		for(int i = 0; i < Data.layers.Count; i++) {
            foreach (CellData cell in Data.layers[i].cells) {
                GameObject newGO = Instantiate(Prefabs[i][cell.value - 1], PrefabsParents[i]);
				newGO.GetComponent<SpriteRenderer>().sortingOrder = i-2;
                newGO.transform.position = new Vector3(cell.y / Data.size, -cell.x / Data.size, 0f);
            }
        }

		Camera.main.transform.position = new Vector3((Data.columns - 1) / 2f / Data.size, (1 - Data.rows) / 2f / Data.size, -10f);

		GetComponent<GameController>().StartGame(Data.playerSpawn, Data.patientSpawn, Data.patientSpawnDirection);
	}

	private string ReadFromFile(string fileName) {
		StreamReader sr = new StreamReader(Path.Combine(path, fileName));
		return sr.ReadToEnd();
	}
}
