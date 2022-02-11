using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;
using System;
using System.IO;

[Serializable]
public class LevelData {
	public LevelData(float camX, float camY, float camSize, float levelTime, int rows, int columns,
		List<CellData> floorCells, List<CellData> wallCells, List<string> diseases, LevelObjectsContainer loContainer,
		Vector3 playerSpawn, Vector3 patientSpawn, string patientSpawnDirection, int patientQueueSize)
		{ this.camX = camX; this.camY = camY; this.camSize = camSize;  this.levelTime = levelTime; this.rows = rows; this.columns = columns;
		this.floorCells = floorCells; this.wallCells = wallCells; this.diseases = diseases; this.loContainer = loContainer;
		this.playerSpawn = playerSpawn; this.patientSpawn = patientSpawn; this.patientSpawnDirection = patientSpawnDirection; this.patientQueueSize = patientQueueSize; }

	public float camX, camY, camSize;
	public float levelTime;
	public int rows, columns;
	public List<CellData> floorCells;
	public List<CellData> wallCells;
	public List<string> diseases;
	public Vector3 playerSpawn, patientSpawn;
	public string patientSpawnDirection;
	public int patientQueueSize;
	public LevelObjectsContainer loContainer;
}

[Serializable]
public class LevelObject {
	public LevelObject(Vector3 pos, string path, bool isSeat, bool isWelcomeSeat, int sortingOrder, List<LevelObject> childs = null) 
		{ this.pos = pos; this.path = path; this.isSeat = isSeat; this.isWelcomeSeat = isWelcomeSeat; 
		this.sortingOrder = sortingOrder; this.childs = childs; }

	public Vector3 pos;
	public string path;
	public bool isSeat;
	public bool isWelcomeSeat;
	public int sortingOrder;
	[NonSerialized] public List<LevelObject> childs;
}

[Serializable]
public class LevelObjectsContainer : ISerializationCallbackReceiver {

	public List<LevelObject> LevelObjects;
	public List<LevelObject> ChildLevelObjects;
	public List<serializationInfo> serializationInfos;

	public void OnBeforeSerialize() {
		serializationInfos = new List<serializationInfo>();
		foreach (LevelObject lo in LevelObjects) {
			serializationInfo info = new serializationInfo();
			info.parentIndex = LevelObjects.FindIndex(e => e == lo);
			info.childsIndex = new List<int>();
			foreach (LevelObject child in lo.childs)
				info.childsIndex.Add(ChildLevelObjects.FindIndex(e => e == child));

			serializationInfos.Add(info);
		}
	}

	public void OnAfterDeserialize() {
		foreach (serializationInfo info in serializationInfos) {
			List<LevelObject> childs = new List<LevelObject>();
			foreach (int index in info.childsIndex)
				childs.Add(ChildLevelObjects[index]);
			LevelObjects[info.parentIndex].childs = childs;
		}
	}

	[Serializable]
	public struct serializationInfo {
		public int parentIndex;
		public List<int> childsIndex;
	}
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

		LevelObjectsContainer loContainer = new LevelObjectsContainer();
		loContainer.LevelObjects = new List<LevelObject>();
		loContainer.ChildLevelObjects = new List<LevelObject>();
		foreach (GameObject go in lec.ObjectsList)
			loContainer.LevelObjects.Add(GameObjectToLevelObject(go, loContainer));

		// Abort if error was found
		if (error) return null;

		return new LevelData(camParams.x, camParams.y, camParams.size, Int32.Parse(lec.levelTime.text), lec.rows, lec.columns,
			floorCells, wallCells, new List<string>(LevelDiseasesController.instance.Elements.Keys), loContainer,
			lec.PlayerSpawn.transform.position, lec.PatientSpawn.transform.position,
			PatientSpawnDirectionDropdown.options[PatientSpawnDirectionDropdown.value].text, Int32.Parse(PatientQueueSize.text));
	}

	private LevelObject GameObjectToLevelObject(GameObject go, LevelObjectsContainer container) {

		LevelObjectController loc = go.GetComponent<LevelObjectController>();

		List<LevelObject> childs = new List<LevelObject>();
		foreach (LevelObjectController child in loc.childs) {
			LevelObject newLO = new LevelObject(child.transform.position, child.path, child.isSeat, child.isWelcomeSeat,
			child.transform.Find("Sprite").GetComponent<SpriteRenderer>().sortingOrder);

			childs.Add(newLO);
            container.ChildLevelObjects.Add(newLO);
		}

		LevelObject lo = new LevelObject(go.transform.position, loc.path, loc.isSeat, loc.isWelcomeSeat, 
			go.transform.Find("Sprite").GetComponent<SpriteRenderer>().sortingOrder, childs);

		return lo;
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

		lec.levelTime.text = Data.levelTime.ToString();

		lec.SetPlayerSpawn(Data.playerSpawn);
		lec.SetPatientSpawn(Data.patientSpawn);
		PatientSpawnDirectionDropdown.value = PatientSpawnDirectionDropdown.options.FindIndex(o => o.text == Data.patientSpawnDirection);

		LevelDiseasesController.instance.DeleteAll();
		foreach (string s in Data.diseases)
			LevelDiseasesController.instance.TryAddDisease(s);

		LevelCameraController.instance.SetCamParams(Data.camX, Data.camY, Data.camSize);
		PatientQueueSize.text = Data.patientQueueSize.ToString();

		// Load level objects
		lec.ClearLevelObjects();

        foreach (LevelObject lo in Data.loContainer.LevelObjects) {
			GameObject loaded = Resources.Load<GameObject>(lo.path);
			if (loaded) {
                GameObject newGO = CreateLevelObject(loaded, lo);
				newGO.transform.SetParent(lec.ObjectsParent);
				lec.ObjectsList.Add(newGO);

				if (lo.childs != null && lo.childs.Count > 0) {
					// Add the SortingGroup to each parent
					newGO.AddComponent<SortingGroup>();
					newGO.transform.Find("Sprite").GetComponent<SpriteRenderer>().sortingOrder = -10000;

					foreach(LevelObject child in lo.childs) {
						GameObject childLoaded = Resources.Load<GameObject>(child.path);
						if(childLoaded) {
							GameObject newChild = CreateLevelObject(childLoaded, child);
							newChild.transform.SetParent(newGO.transform);
							newGO.GetComponent<LevelObjectController>().childs.Add(newChild.GetComponent<LevelObjectController>());
							newChild.transform.Find("Sprite").GetComponent<SpriteRenderer>().sortingOrder = Mathf.RoundToInt(newChild.transform.position.y * -100);

						} else {
							Debug.Log("ERREUR CHARGEMENT DE CHILD " + child.path);
						}
					}
				}
			} else {
				Debug.Log("ERREUR CHARGEMENT DE " + lo.path);
			}
		}
	}
	private GameObject CreateLevelObject(GameObject loaded, LevelObject lo) {

		GameObject FollowerChild = loaded.transform.Find("Sprite").gameObject;

		// Create main object
		GameObject newGO = new GameObject(loaded.name, typeof(BoxCollider2D), typeof(LevelObjectController));
		newGO.layer = LayerMask.NameToLayer("LevelObjects");
		newGO.transform.position = lo.pos;
		newGO.GetComponent<LevelObjectController>().SetParams(lo.path, lo.isSeat, lo.isWelcomeSeat);

		// Create child to get spriterenderer (because we may want to offset it)
		GameObject childForSprite = new GameObject("Sprite", typeof(SpriteRenderer));
		childForSprite.transform.SetParent(newGO.transform);
        childForSprite.GetComponent<SpriteRenderer>().sprite = FollowerChild.GetComponent<SpriteRenderer>().sprite;
        childForSprite.transform.localPosition = FollowerChild.transform.localPosition;

		// Adjust boxcollider2D size to sprite size and position
		BoxCollider2D bc2D = newGO.GetComponent<BoxCollider2D>();
		bc2D.size = childForSprite.GetComponent<SpriteRenderer>().size;
		Sprite sprite = childForSprite.GetComponent<SpriteRenderer>().sprite;
		Vector2 pivot = new Vector2(sprite.pivot.x / sprite.rect.width, sprite.pivot.y / sprite.rect.height);
		Vector2 offset = new Vector2(.5f, .5f) - pivot;
		bc2D.offset = (Vector2)childForSprite.transform.localPosition + offset * childForSprite.GetComponent<SpriteRenderer>().size;

		return newGO;
	}
}