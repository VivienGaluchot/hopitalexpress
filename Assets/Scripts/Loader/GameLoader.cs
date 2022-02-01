using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;

public class GameLoader : MonoBehaviour {

	[SerializeField] private Transform LevelParent;
	[SerializeField] private Dropdown dd;
	[SerializeField] private string path;

	// WE SHOULD STOCK IT IN JSON FILES OR FIX IT
	private float size = 2f;

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

		foreach(CellData cell in Data.cells) {
			GameObject newGO = Instantiate(Walls[cell.value-1], LevelParent);
			newGO.transform.position = new Vector3(cell.y/size, -cell.x/size, 0f);
		}
		Camera.main.transform.position = new Vector3((Data.columns - 1) / 2f / size, (1 - Data.rows) / 2f / size, -10f);

		GetComponent<GameController>().StartGame();

		//GameObject newGO = new GameObject(i + "-" + j, typeof(SpriteRenderer), typeof(BoxCollider2D));
		//newGO.transform.SetParent(CellsParent);
		//newGO.AddComponent<CellController>();
		//newGO.GetComponent<CellController>().Setup(this, i, j);
		//newGO.transform.position = new Vector3(j / size, -i / size, 0);
		//SpriteRenderer sr = newGO.GetComponent<SpriteRenderer>();
		//sr.sprite = baseSprite;
		//sr.sortingOrder = -1;
		//sr.color = (i + j) % 2 == 0 ? Color.grey : Color.white;
		//newGO.GetComponent<BoxCollider2D>().size = new Vector2(1f / size, 1f / size);

		//grid.Add((i, j), new Cell(newGO, sr));

		//lec.ResizeGrid(Data.rows, Data.columns);
		//lec.ClearGrid();
		//foreach (CellData cell in Data.cells)
		//	lec.grid[(cell.x, cell.y)].value = cell.value;

		//lec.RefreshGrid();

		//ldc.DeleteAll();
		//foreach (string s in Data.diseases)
		//	ldc.TryAddDisease(s);

	}

	private string ReadFromFile(string fileName) {
		StreamReader sr = new StreamReader(Path.Combine(path, fileName));
		return sr.ReadToEnd();
	}
}
