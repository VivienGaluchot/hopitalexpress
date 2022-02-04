using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.IO;

[Serializable]
public class LevelData {
	public LevelData(float size, int rows, int columns, List<LayerData> layers, List<string> diseases, Vector3 playerSpawn, Vector3 patientSpawn, string patientSpawnDirection)
		{ this.size = size; this.rows = rows; this.columns = columns; this.layers = layers; this.diseases = diseases; this.playerSpawn = playerSpawn; this.patientSpawn = patientSpawn; this.patientSpawnDirection = patientSpawnDirection; }

	public float size;
	public int rows;
	public int columns;
	public List<LayerData> layers;
	public List<string> diseases;
	public Vector3 playerSpawn;
	public Vector3 patientSpawn;
	public string patientSpawnDirection;
}

[Serializable]
public class LayerData {
	public LayerData(List<CellData> cells) { this.cells = cells; }

	public List<CellData> cells;
}

[Serializable]
public class CellData {
	public CellData(int x, int y, int value) { this.x = x; this.y = y; this.value = value; }
	public int x;
	public int y;
	public int value;
}

public class LevelDataController : MonoBehaviour {

	[SerializeField] private Dropdown FilesDropdown;
	[SerializeField] private Dropdown PatientSpawnDirectionDropdown;
	[SerializeField] private InputField FileNameInputField;
	[SerializeField] private string path;
	private LevelEditorController lec;
	private LevelDiseasesController ldc;

	private bool clickedDelete;

	private void Start() {
		lec = GetComponent<LevelEditorController>();
		ldc = GetComponent<LevelDiseasesController>();
		path = Path.Combine(Application.dataPath, path);
		FetchDDOptions();
	}

	public void DeleteSave(Text text) {
		if(!clickedDelete) {
			clickedDelete = true;
			text.text = "Sure?";
        } else {
			text.text = "Delete";
			clickedDelete = false;
			string filePath = Path.Combine(path, FilesDropdown.options[FilesDropdown.value].text + ".json");
			if (File.Exists(filePath))
				File.Delete(filePath);
			FetchDDOptions();
		}
    }

	private void FetchDDOptions() {
		string[] paths = System.IO.Directory.GetFiles(path);
		List<string> pathsList = new List<string>();
		foreach (string s in paths) {
			if (!s.EndsWith(".meta"))
				pathsList.Add(Path.GetFileNameWithoutExtension(s));
		}
		FilesDropdown.ClearOptions();
		FilesDropdown.AddOptions(pathsList);
	}

	public void SaveData() {
		lec.StopSelectingSpawns();
		LevelData ld = FetchDataToLevelData();
		if(ld != null) {
			WriteToFile(JsonUtility.ToJson(ld));
			FetchDDOptions();
		}
	}

	public LevelData FetchDataToLevelData() {
		List<LayerData> layers = new List<LayerData>();
		for(int i = 0; i < lec.grids.Length; i++) {
			List<CellData> cells = new List<CellData>();
			foreach (KeyValuePair<(int, int), LevelEditorController.Cell> cell in lec.grids[i]) {
				if (cell.Value.value > 0)
					cells.Add(new CellData(cell.Key.Item1, cell.Key.Item2, cell.Value.value));
			}

			layers.Add(new LayerData(cells));
		}

		if (lec.PlayerSpawn == null) {
			Debug.Log("Erreur pas de spawn Player");
			return null;
		}
			
		if (lec.PatientSpawn == null) {
			Debug.Log("Erreur pas de spawn Patient");
			return null;
		}

		return new LevelData(lec.size, lec.rows, lec.columns, layers, new List<string>(ldc.Elements.Keys), lec.PlayerSpawn.transform.position, lec.PatientSpawn.transform.position, PatientSpawnDirectionDropdown.options[PatientSpawnDirectionDropdown.value].text);
	}

	private void WriteToFile(string content) {
		StreamWriter sw = new StreamWriter(Path.Combine(path, FileNameInputField.text + ".json"));
		sw.WriteLine(content);
		sw.Close();
	}

	public void LoadData() {
		lec.StopSelectingSpawns();
		string filename = FilesDropdown.options[FilesDropdown.value].text + ".json";
		LevelData Data = JsonUtility.FromJson<LevelData>(ReadFromFile(filename));
		FileNameInputField.text = Path.GetFileNameWithoutExtension(filename);
		DisplayLoadedData(Data);
	}

	private void DisplayLoadedData(LevelData Data) {
		lec.ResizeGrid(Data.rows, Data.columns);
		lec.ClearAllGrid();
		for(int i = 0; i < Data.layers.Count; i++)
			foreach (CellData cell in Data.layers[i].cells)
				lec.grids[i][(cell.x, cell.y)].value = cell.value;
		
		lec.RefreshAllGrid();

		lec.SetPatientSpawn(Data.patientSpawn);
		lec.SetPlayerSpawn(Data.playerSpawn);
		PatientSpawnDirectionDropdown.value = PatientSpawnDirectionDropdown.options.FindIndex(o => o.text == Data.patientSpawnDirection);

		ldc.DeleteAll();
		foreach (string s in Data.diseases)
			ldc.TryAddDisease(s);
	}

	private string ReadFromFile(string fileName) {
		StreamReader sr = new StreamReader(Path.Combine(path, fileName));
		return sr.ReadToEnd();
	}
}