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
	private LevelDiseasesController ldc;

	private void Start() {
		lec = GetComponent<LevelEditorController>();
		ldc = GetComponent<LevelDiseasesController>();
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
	public class LevelData {
		public LevelData(int rows, int columns, List<CellData> cells, List<string> diseases) 
			{ this.rows = rows; this.columns = columns; this.cells = cells; this.diseases = diseases; }

		public int rows;
		public int columns;
		public List<CellData> cells;
		public List<string> diseases;
	}

	[Serializable]
	public class CellData {
		public CellData(int x, int y, int value) { this.x = x; this.y = y; this.value = value; }
		public int x;
		public int y;
		public int value;
    }

	public void SaveData() {
		WriteToFile(JsonUtility.ToJson(FetchDataToLevelData()));
		FetchDDOptions();
	}

	public LevelData FetchDataToLevelData() {
		List<CellData> cells = new List<CellData>();
		foreach(KeyValuePair<(int, int), LevelEditorController.Cell> cell in lec.grid) {
			if(cell.Value.value > 0)
				cells.Add(new CellData(cell.Key.Item1, cell.Key.Item2, cell.Value.value));
        }

		return new LevelData(lec.rows, lec.columns, cells, new List<string>(ldc.Elements.Keys));
	}

	private void WriteToFile(string content) {
		StreamWriter sw = new StreamWriter(Path.Combine(path, FileNameInputField.text + ".txt"));
		sw.WriteLine(content);
		sw.Close();
	}

	public void LoadData() {
		string filename = dd.options[dd.value].text;
		LevelData Data = JsonUtility.FromJson<LevelData>(ReadFromFile(filename));
		FileNameInputField.text = Path.GetFileNameWithoutExtension(filename);
		DisplayLoadedData(Data);
	}

	private void DisplayLoadedData(LevelData Data) {
		lec.ResizeGrid(Data.rows, Data.columns);
		lec.ClearGrid();
		foreach(CellData cell in Data.cells)
			lec.grid[(cell.x, cell.y)].value = cell.value;

		lec.RefreshGrid();

		ldc.DeleteAll();
		foreach (string s in Data.diseases)
			ldc.TryAddDisease(s);
	}

	private string ReadFromFile(string fileName) {
		StreamReader sr = new StreamReader(Path.Combine(path, fileName));
		return sr.ReadToEnd();
	}
}