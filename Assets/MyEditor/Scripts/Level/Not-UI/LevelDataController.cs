using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.IO;

[Serializable]
public class LevelData {
	public LevelData(float camX, float camY, float camSize, int rows, int columns,
		List<CellData> floorCells, List<CellData> wallCells, List<string> diseases, List<LevelObject> LevelObjects,
		Vector3 playerSpawn, Vector3 patientSpawn, string patientSpawnDirection, int patientQueueSize)
		{ this.camX = camX; this.camY = camY; this.camSize = camSize;  this.rows = rows; this.columns = columns;
		this.floorCells = floorCells; this.wallCells = wallCells; this.diseases = diseases; this.LevelObjects = LevelObjects;
		this.playerSpawn = playerSpawn; this.patientSpawn = patientSpawn; this.patientSpawnDirection = patientSpawnDirection; this.patientQueueSize = patientQueueSize; }

	public float camX, camY, camSize;
	public int rows, columns;
	public List<CellData> floorCells;
	public List<CellData> wallCells;
	public List<string> diseases;
	public Vector3 playerSpawn, patientSpawn;
	public string patientSpawnDirection;
	public int patientQueueSize;
	public List<LevelObject> LevelObjects;
}

[Serializable]
public class LevelObject {
	public LevelObject(Vector3 pos, string path, bool isSeat, bool isWelcomeSeat) 
		{ this.pos = pos; this.path = path; this.isSeat = isSeat; this.isWelcomeSeat = isWelcomeSeat; }

	public Vector3 pos;
	public string path;
	public bool isSeat;
	public bool isWelcomeSeat;
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
		bool error = false;

		List<CellData> floorCells = new List<CellData>();
		foreach (KeyValuePair<(int, int), Cell> cell in lec.floorGrid) {
			if (cell.Value.value > 0)
				floorCells.Add(new CellData(cell.Key.Item1, cell.Key.Item2, cell.Value.value));
		}
		List<CellData> wallCells = new List<CellData>();
		foreach (KeyValuePair<(int, int), Cell> cell in lec.wallGrid) {
			if (cell.Value.value > 0)
				wallCells.Add(new CellData(cell.Key.Item1, cell.Key.Item2, cell.Value.value));
		}

		if (lec.PlayerSpawn == null) {
			Debug.LogWarning("Erreur pas de spawn Player");
			error = true;
		}
			
		if (lec.PatientSpawn == null) {
			Debug.LogWarning("Erreur pas de spawn Patient");
			error = true;
		}

		(float x, float y, float size) camParams = LevelCameraController.instance.GetCamParams();

		List<LevelObject> LevelObjects = new List<LevelObject>();
		foreach(KeyValuePair<GameObject, LevelObjectController> lo in lec.ObjectsList) {
			LevelObjects.Add(new LevelObject(lo.Key.transform.position, lo.Value.path, lo.Value.isSeat, lo.Value.isWelcomeSeat));
        }

		// Abort if error was found
		if (error) return null;

		return new LevelData(camParams.x, camParams.y, camParams.size, lec.rows, lec.columns,
			floorCells, wallCells, new List<string>(LevelDiseasesController.instance.Elements.Keys), LevelObjects,
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

		foreach (CellData cell in Data.floorCells)
			lec.floorGrid[(cell.x, cell.y)].value = cell.value;
		foreach (CellData cell in Data.wallCells)
			lec.wallGrid[(cell.x, cell.y)].value = cell.value;

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
				lec.ObjectsList.Add(newGO, new LevelObjectController(lo.path, lo.isSeat, lo.isWelcomeSeat));
			} else {
				Debug.Log("ERREUR CHARGEMENT DE " + lo.path);
			}
        }
	}
}