using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.IO;
using System.Globalization;

public class LevelDataController : MonoBehaviour {

	[SerializeField] private Dropdown dd;
	[SerializeField] private InputField FileNameInputField;
	[SerializeField] private string path;
	private LevelEditorController lec;

	private void Start() {
		lec = GetComponent<LevelEditorController>();
		path = Path.Combine(Application.dataPath, path);
		FetchDDOptions();
	}

	private void FetchDDOptions() {
		string[] paths = System.IO.Directory.GetFiles(path);
		List<string> pathsList = new List<string>();
		foreach (string s in paths) {
			if (!s.EndsWith(".meta"))
				pathsList.Add(Path.GetFileName(s));
		}
		dd.ClearOptions();
		dd.AddOptions(pathsList);
	}

	[Serializable]
	public class GridData {
		public GridData(List<CellData> cells) { this.cells = cells;  }
		public List<CellData> cells;
	}

	[Serializable]
	public class CellData {
		public CellData(int x, int y, int value) { this.x = x; this.y = y; this.value = value; }
		public int x;
		public int y;
		public int value;
    }

	public void SaveData() {
		WriteToFile(JsonUtility.ToJson(FetchDataToGridData()));
		FetchDDOptions();
	}

	public GridData FetchDataToGridData() {
		List<CellData> cells = new List<CellData>();
		foreach(KeyValuePair<(int, int), LevelEditorController.Cell> cell in lec.grid) {
			cells.Add(new CellData(cell.Key.Item1, cell.Key.Item2, cell.Value.value));
        }
		return new GridData(cells);
	}

	private void WriteToFile(string content) {
		StreamWriter sw = new StreamWriter(Path.Combine(path, FileNameInputField.text + ".txt"));
		sw.WriteLine(content);
		sw.Close();
	}

	public void LoadData() {
		string filename = dd.options[dd.value].text;
		GridData Data = JsonUtility.FromJson<GridData>(ReadFromFile(filename));
		FileNameInputField.text = Path.GetFileNameWithoutExtension(filename);
		DisplayLoadedData(Data);
	}

	private void DisplayLoadedData(GridData Data) {
		foreach(CellData cell in Data.cells) {
			lec.grid[(cell.x, cell.y)].value = cell.value;
        }
		lec.RefreshGrid();
	}

	private string ReadFromFile(string fileName) {
		StreamReader sr = new StreamReader(Path.Combine(path, fileName));
		return sr.ReadToEnd();
	}
}