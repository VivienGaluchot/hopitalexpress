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

public class LevelDataController : DataController {

	[SerializeField] private Dropdown PatientSpawnDirectionDropdown;

	public override void SaveData() {
		LevelData ld = FetchDataToLevelData();
		if(ld != null) {
			WriteToFile(JsonUtility.ToJson(ld));
			FetchFilesNamesToLoad();
		}
	}

	public LevelData FetchDataToLevelData() {
		List<LayerData> layers = new List<LayerData>();
		for(int i = 0; i < LevelEditorController.instance.grids.Length; i++) {
			List<CellData> cells = new List<CellData>();
			foreach (KeyValuePair<(int, int), Cell> cell in LevelEditorController.instance.grids[i]) {
				if (cell.Value.value > 0)
					cells.Add(new CellData(cell.Key.Item1, cell.Key.Item2, cell.Value.value));
			}

			layers.Add(new LayerData(cells));
		}

		if (LevelEditorController.instance.PlayerSpawn == null) {
			Debug.Log("Erreur pas de spawn Player");
			return null;
		}
			
		if (LevelEditorController.instance.PatientSpawn == null) {
			Debug.Log("Erreur pas de spawn Patient");
			return null;
		}

		return new LevelData(LevelEditorController.instance.size, LevelEditorController.instance.rows, LevelEditorController.instance.columns, layers, new List<string>(LevelDiseasesController.instance.Elements.Keys), LevelEditorController.instance.PlayerSpawn.transform.position, LevelEditorController.instance.PatientSpawn.transform.position, PatientSpawnDirectionDropdown.options[PatientSpawnDirectionDropdown.value].text);
	}

	public override void LoadData() {
		if (FilesDropdown.options.Count == 0) {
			Debug.LogWarning("Aucun fichier à charger");
			return;
		}
		string filename = FilesDropdown.options[FilesDropdown.value].text + ".json";
		LevelData Data = JsonUtility.FromJson<LevelData>(ReadFromFile(filename));
		FileNameInputField.text = Path.GetFileNameWithoutExtension(filename);
		DisplayLoadedData(Data);
	}

	private void DisplayLoadedData(LevelData Data) {
		LevelEditorController.instance.ResizeGrid(Data.rows, Data.columns);
		LevelEditorController.instance.ClearAllGrid();
		for(int i = 0; i < Data.layers.Count; i++)
			foreach (CellData cell in Data.layers[i].cells) {
				LevelEditorController.instance.grids[i][(cell.x, cell.y)].value = cell.value; 
				if (i == 1 && cell.value > 1) 
					LevelEditorController.instance.fullWallsGrid[(cell.x, cell.y)].sprite = LevelEditorController.instance.fullWallSprites[cell.value - 2];
			}



		LevelEditorController.instance.RefreshAllGrid();

		LevelEditorController.instance.SetPlayerSpawn(Data.playerSpawn);
		LevelEditorController.instance.SetPatientSpawn(Data.patientSpawn);
		PatientSpawnDirectionDropdown.value = PatientSpawnDirectionDropdown.options.FindIndex(o => o.text == Data.patientSpawnDirection);

		LevelDiseasesController.instance.DeleteAll();
		foreach (string s in Data.diseases)
			LevelDiseasesController.instance.TryAddDisease(s);
	}
}