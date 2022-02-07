using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.IO;

[Serializable]
public class LevelData {
	public LevelData(float camX, float camY, float camSize, int rows, int columns,
		List<LayerData> layers, List<string> diseases, List<LevelObject> LevelObjects,
		Vector3 playerSpawn, Vector3 patientSpawn, string patientSpawnDirection, int patientQueueSize)
		{ this.camX = camX; this.camY = camY; this.camSize = camSize;  this.rows = rows; this.columns = columns; 
		this.layers = layers; this.diseases = diseases; this.LevelObjects = LevelObjects;
		this.playerSpawn = playerSpawn; this.patientSpawn = patientSpawn; this.patientSpawnDirection = patientSpawnDirection; this.patientQueueSize = patientQueueSize; }

	public float camX, camY, camSize;
	public int rows, columns;
	public List<LayerData> layers;
	public List<string> diseases;
	public Vector3 playerSpawn, patientSpawn;
	public string patientSpawnDirection;
	public int patientQueueSize;
	public List<LevelObject> LevelObjects;
}

[Serializable]
public class LevelObject {
	public LevelObject(Vector3 pos, string path) { this.pos = pos; this.path = path; }

	public Vector3 pos;
	public string path;
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
	[SerializeField] private InputField PatientQueueSize;

	private LevelEditorController lec;

	private void Start() { lec = LevelEditorController.instance; }

	public override void SaveData() {
		LevelData ld = FetchDataToLevelData();
		if(ld != null) {
			WriteToFile(JsonUtility.ToJson(ld));
			FetchFilesNamesToLoad();
		}
	}

	public LevelData FetchDataToLevelData() {
		List<LayerData> layers = new List<LayerData>();
		for(int i = 0; i < lec.grids.Length; i++) {
			List<CellData> cells = new List<CellData>();
			foreach (KeyValuePair<(int, int), Cell> cell in lec.grids[i]) {
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

		(float x, float y, float size) camParams = LevelCameraController.instance.GetCamParams();

		List<LevelObject> LevelObjects = new List<LevelObject>();
		foreach(KeyValuePair<GameObject, string> lo in lec.ObjectsList) {
			LevelObjects.Add(new LevelObject(lo.Key.transform.position, lo.Value));
        }

		return new LevelData(camParams.x, camParams.y, camParams.size, lec.rows, lec.columns,
			layers, new List<string>(LevelDiseasesController.instance.Elements.Keys), LevelObjects,
			lec.PlayerSpawn.transform.position, lec.PatientSpawn.transform.position,
			PatientSpawnDirectionDropdown.options[PatientSpawnDirectionDropdown.value].text, Int32.Parse(PatientQueueSize.text));
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
		lec.ResizeGrid(Data.rows, Data.columns);
		for(int i = 0; i < Data.layers.Count; i++)
			foreach (CellData cell in Data.layers[i].cells) {
				lec.grids[i][(cell.x, cell.y)].value = cell.value; 
				if (i == 1 && cell.value > 1) 
					lec.fullWallsGrid[(cell.x, cell.y)].GetComponent<SpriteRenderer>().sprite = lec.fullWallSprites[cell.value - 2];
			}

		lec.RefreshAllGrid();

		lec.SetPlayerSpawn(Data.playerSpawn);
		lec.SetPatientSpawn(Data.patientSpawn);
		PatientSpawnDirectionDropdown.value = PatientSpawnDirectionDropdown.options.FindIndex(o => o.text == Data.patientSpawnDirection);

		LevelDiseasesController.instance.DeleteAll();
		foreach (string s in Data.diseases)
			LevelDiseasesController.instance.TryAddDisease(s);

		LevelCameraController.instance.SetCamParams(Data.camX, Data.camY, Data.camSize);
		PatientQueueSize.text = Data.patientQueueSize.ToString();

		foreach(LevelObject lo in Data.LevelObjects) {
			GameObject loaded = Resources.Load<GameObject>(lo.path);
			if(loaded) {
				GameObject newGO = Instantiate(loaded, lo.pos, Quaternion.identity, lec.ObjectsParent);
				newGO.AddComponent<BoxCollider2D>();
				newGO.layer = LayerMask.NameToLayer("LevelObjects");
			} else {
				Debug.Log("ERREUR CHARGEMENT DE " + lo.path);
			}
        }
	}
}