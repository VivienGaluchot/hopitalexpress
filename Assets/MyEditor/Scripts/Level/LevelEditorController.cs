using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public enum DrawType { 
	floor,
	walls,
	fillFloor,
	fillWalls,
	playerSpawn,
	patientSpawn,
	levelObject,
	none
}

public class Cell {
	public Cell(GameObject go) { this.go = go; sr = this.go.GetComponent<SpriteRenderer>(); value = 0; }
	public Cell(GameObject go, SpriteRenderer sr) { this.go = go; this.sr = sr; value = 0; }

	public GameObject go;
	public int value;
	public SpriteRenderer sr;
}

public class LevelEditorController : MonoBehaviour {
	public static LevelEditorController instance;

	public float size;
	public int columns, rows;
	public DrawType drawType;

	public Sprite[] floorSprites, wallSprites, fullWallSprites;
	public Sprite[][] sprites;
	public Dictionary<(int, int), Cell>[] grids { get; private set; }
	public Dictionary<(int, int), SpriteRenderer> fullWallsGrid { get; private set; }

	private Transform[] CellsParents, WallsParents;
	private Transform ObjectsParent;
	
	private enum WallVisibility {
		none,
		flat,
		not_flat
	}
	private WallVisibility wallVisibility;

	public GameObject PlayerSpawn { get; set; }
	public GameObject PatientSpawn { get; set; }
	public Sprite PlayerSpawnSprite, PatientSpawnSprite;

	//Objects
	public string[] objectsPath;
	private GameObject Follower;
	private SpriteRenderer followerSR;

	private void Awake() { 
		instance = this;
		drawType = DrawType.none;
		sprites = new Sprite[2][] { floorSprites, wallSprites };
		grids = new Dictionary<(int, int), Cell>[2];
		CellsParents = new Transform[2];
		CellsParents[0] = transform.Find("FloorLayer");
		CellsParents[1] = transform.Find("WallsLayer");

		WallsParents = new Transform[2];
		WallsParents[0] = CellsParents[1];
		WallsParents[1] = transform.Find("FullWallsLayer");
		fullWallsGrid = new Dictionary<(int, int), SpriteRenderer>();
		for (int i = 0; i < rows; i++) {
			for (int j = 0; j < columns; j++) {
				GameObject newCell = new GameObject(i + "-" + j, typeof(SpriteRenderer));
				newCell.transform.SetParent(WallsParents[1]);
				newCell.transform.position = new Vector3(j / size, -i / size, 0);
				fullWallsGrid.Add((i, j), newCell.GetComponent<SpriteRenderer>());
			}
		}

		wallVisibility = WallVisibility.flat;
		SwitchWallsVisibility();

		PlayerSpawn = new GameObject("PlayerSpawn", typeof(SpriteRenderer));
		PlayerSpawn.transform.SetParent(transform);
		PlayerSpawn.GetComponent<SpriteRenderer>().sprite = PlayerSpawnSprite;
		PlayerSpawn.SetActive(false);
		PatientSpawn = new GameObject("PatientSpawn", typeof(SpriteRenderer));
		PatientSpawn.transform.SetParent(transform);
		PatientSpawn.GetComponent<SpriteRenderer>().sprite = PatientSpawnSprite;
		PatientSpawn.SetActive(false);

		ObjectsParent = transform.Find("ObjectsLayer");
		Follower = new GameObject("Follower", typeof(SpriteRenderer));
		Follower.transform.SetParent(transform);
		followerSR = Follower.GetComponent<SpriteRenderer>();
	}
	private void Start() {
		InitGrid();
	}

	private void Update() {
		if (Input.GetMouseButtonDown(0) && !GlobalFunctions.DoesHitUI()) {
			Vector3 worldMousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition) + new Vector3(0f, 0f, 10f);
			switch (drawType) {
				case DrawType.playerSpawn:
					if (!PlayerSpawn.activeSelf)
						PlayerSpawn.SetActive(true);
					SetPlayerSpawn(worldMousePos);
					break;
				case DrawType.patientSpawn:
					if (!PatientSpawn.activeSelf)
						PatientSpawn.SetActive(true);
					SetPatientSpawn(worldMousePos);
					break;
				case DrawType.levelObject:
					Follower.transform.position = worldMousePos;
					Instantiate(Follower, ObjectsParent);
					break;
			}
		}
	}

	public void SetPlayerSpawn(Vector3 pos) { PlayerSpawn.transform.position = pos; }
	public void SetPatientSpawn(Vector3 pos) { PatientSpawn.transform.position = pos; }

	public void SwitchObjectsVisibility() {
		ObjectsParent.gameObject.SetActive(!ObjectsParent.gameObject.activeSelf);
    }

	public void SwitchWallsVisibility() {
		// Enum.GetNames(typeof(WallVisibility)).Length
		wallVisibility = (WallVisibility)(((int)wallVisibility + 1) % 3);
		switch (wallVisibility) {
			case WallVisibility.none:
				WallsParents[1].gameObject.SetActive(false);
				break;
			case WallVisibility.flat:
				WallsParents[0].gameObject.SetActive(true);
				break;
			case WallVisibility.not_flat:
				WallsParents[0].gameObject.SetActive(false);
				WallsParents[1].gameObject.SetActive(true);
				break;
		}
	}

	public void ClickedCell(int i, int j, bool clicked = false, bool reset = false) {
		if (drawType == DrawType.floor || drawType == DrawType.walls || drawType == DrawType.fillFloor || drawType == DrawType.fillWalls) {
			int targetLayer = 0;
			if(drawType == DrawType.walls || drawType == DrawType.fillWalls) {
				targetLayer = 1;
				if (wallVisibility == WallVisibility.none) SwitchWallsVisibility();
            }
			if (grids[targetLayer].TryGetValue((i, j), out Cell cell)) {
				if (!reset) {
					if (drawType == DrawType.fillWalls) {
						WallFill(i, j);
					} else if (drawType == DrawType.fillFloor) {
						FloorFill(i, j, cell);
					} else if ((cell.value != 0 || clicked)) {
						int newValue = ComputeCellValue(i, j, targetLayer);
						if (newValue != cell.value) {
							cell.value = newValue;
							ChangeSprite(cell, targetLayer);
							if (targetLayer == 1 && newValue > 0) fullWallsGrid[(i, j)].sprite = fullWallSprites[newValue - 1];
							Propagate(i, j);
						}
					}
				} else if (cell.value != 0) {
					cell.value = 0;
					ChangeSprite(cell, targetLayer);
					if (targetLayer == 1) fullWallsGrid[(i, j)].sprite = null;
					Propagate(i, j);
				}
			}
		}
	}

	private void FloorFill(int i, int j, Cell cell) {
		// Onnly work if we clicked on an empty cell
		if (cell.value > 0)
			return;

		int newValue = ComputeCellValue(i, j, 0);
		if (cell.value != newValue) {
			cell.value = newValue;
			ChangeSprite(cell, 0);

			Propagate(i, j);
		}

		RefreshGrid(0, true);
	}

	int startingX, startingY;
	bool first;
	private void WallFill(int i, int j) {
		// If we clicked on void cell, abort mission
		if (!grids[0].TryGetValue((i, j), out Cell cell) || cell.value == 0)
			return;

		int x = i-1;
		while(grids[0].TryGetValue((x, j), out cell) && cell.value > 0) { x--; }
		// We found an edge!!
		grids[1][(x+1, j)].value = 1;
		startingX = x + 1;
		startingY = j;
		first = true;
		StepByStep(x + 1, j, -1, -1);
		RefreshGrid(1, true);
	}


	private void StepByStep(int i, int j, int oldI, int oldJ) {
		if(i == startingX && j == startingY) {
			if (first)
				first = false;
			else
				return;
		}
		if (!grids[0].TryGetValue((i+1, j), out Cell cell) || cell.value == 0) {
			tryCells((i, j-1), (i, j+1), (i-1, j), i, j, oldI, oldJ);
		} else if (!grids[0].TryGetValue((i, j-1), out cell) || cell.value == 0) {
			tryCells((i+1, j), (i-1, j), (i, j+1), i, j, oldI, oldJ);
		} else if (!grids[0].TryGetValue((i-1, j), out cell) || cell.value == 0) {
			tryCells((i, j-1), (i, j+1), (i+1, j), i, j, oldI, oldJ);
		} else if (!grids[0].TryGetValue((i, j+1), out cell) || cell.value == 0) {
			tryCells((i-1, j), (i+1, j), (i, j+1), i, j, oldI, oldJ);
		} else {
			CheckDiagonals(i, j, oldI, oldJ);
		}
	}

	private void CheckDiagonals(int i, int j, int oldI, int oldJ) {
		if (grids[0].TryGetValue((i+1, j+1), out Cell cell) && cell.value == 0) {
			if(!tryDirection(i+1, j, oldI, oldJ, i, j))
				tryDirection(i, j+1, oldI, oldJ, i, j);
		} else if (grids[0].TryGetValue((i+1, j-1), out cell) && cell.value == 0) {
			if (!tryDirection(i + 1, j, oldI, oldJ, i, j))
				tryDirection(i, j - 1, oldI, oldJ, i, j);
		} else if (grids[0].TryGetValue((i - 1, j - 1), out cell) && cell.value == 0) {
			if (!tryDirection(i - 1, j, oldI, oldJ, i, j))
				tryDirection(i, j - 1, oldI, oldJ, i, j);
		} else if (grids[0].TryGetValue((i - 1, j + 1), out cell) && cell.value == 0) {
			if (!tryDirection(i - 1, j, oldI, oldJ, i, j))
				tryDirection(i, j + 1, oldI, oldJ, i, j);
		}
	}

	private void tryCells((int i, int j) first, (int i, int j) second, (int i, int j) third, int i, int j, int oldI, int oldJ) {
		if (!grids[0].TryGetValue((first.i, first.j), out Cell cell) || cell.value == 0 || !tryDirection(first.i, first.j, oldI, oldJ, i, j))
			if (!grids[0].TryGetValue((second.i, second.j), out cell) || cell.value == 0 || !tryDirection(second.i, second.j, oldI, oldJ, i, j))
				if (!grids[0].TryGetValue((third.i, third.j), out cell) && cell.value == 0 || !tryDirection(third.i, third.j, oldI, oldJ, i, j))
					Debug.Log("No correct path found");
	}

	private bool tryDirection(int newI, int newJ, int oldI, int oldJ, int currentI, int currentJ) {
		if(newI != oldI || newJ != oldJ) {
			grids[1][(newI, newJ)].value = 1;
			StepByStep(newI, newJ, currentI, currentJ);
			return true;
		}
		return false;
	}

	private void Propagate(int i, int j) {
		ClickedCell(i + 1, j, false, false);
		ClickedCell(i - 1, j, false, false);
		ClickedCell(i, j + 1, false, false);
		ClickedCell(i, j - 1, false, false);
	}

	private void ChangeSprite(Cell cell, int layer) {
		switch(cell.value) {
			case 0:
				cell.sr.sprite = null;
				break;
			case 1:
				cell.sr.sprite = sprites[layer][0];
				break;
			case 2:
				cell.sr.sprite = sprites[layer][1];
				cell.sr.transform.rotation = Quaternion.identity;
				break;
			case 3:
				cell.sr.sprite = sprites[layer][1];
				cell.sr.transform.rotation = Quaternion.Euler(0f, 0f, 270f);
				break;
			case 4:
				cell.sr.sprite = sprites[layer][1];
				cell.sr.transform.rotation = Quaternion.Euler(0f, 0f, 180f);
				break;
			case 5:
				cell.sr.sprite = sprites[layer][1];
				cell.sr.transform.rotation = Quaternion.Euler(0f, 0f, 90f);
				break;
			case 6:
				cell.sr.sprite = sprites[layer][2];
				cell.sr.transform.rotation = Quaternion.identity;
				break;
			case 7:
				cell.sr.sprite = sprites[layer][2];
				cell.sr.transform.rotation = Quaternion.Euler(0f, 0f, 90f);
				break;
			case 8:
				cell.sr.sprite = sprites[layer][3];
				cell.sr.transform.rotation = Quaternion.identity;
				break;
			case 9:
				cell.sr.sprite = sprites[layer][3];
				cell.sr.transform.rotation = Quaternion.Euler(0f, 0f, 270f);
				break;
			case 10:
				cell.sr.sprite = sprites[layer][3];
				cell.sr.transform.rotation = Quaternion.Euler(0f, 0f, 180f);
				break;
			case 11:
				cell.sr.sprite = sprites[layer][3];
				cell.sr.transform.rotation = Quaternion.Euler(0f, 0f, 90f);
				break;
			case 12:
				cell.sr.sprite = sprites[layer][4];
				cell.sr.transform.rotation = Quaternion.identity;
				break;
			case 13:
				cell.sr.sprite = sprites[layer][4];
				cell.sr.transform.rotation = Quaternion.Euler(0f, 0f, 270f);
				break;
			case 14:
				cell.sr.sprite = sprites[layer][4];
				cell.sr.transform.rotation = Quaternion.Euler(0f, 0f, 180f);
				break;
			case 15:
				cell.sr.sprite = sprites[layer][4];
				cell.sr.transform.rotation = Quaternion.Euler(0f, 0f, 90f);
				break;
			case 16:
				cell.sr.sprite = sprites[layer][5];
				break;
			case 17:
				cell.sr.sprite = sprites[layer][6];
				cell.sr.transform.rotation = Quaternion.identity;
				break;
			case 18:
				cell.sr.sprite = sprites[layer][6];
				cell.sr.transform.rotation = Quaternion.Euler(0f, 0f, 90f);
				break;
			case 19:
				cell.sr.sprite = sprites[layer][6];
				cell.sr.transform.rotation = Quaternion.Euler(0f, 0f, 180f);
				break;
			case 20:
				cell.sr.sprite = sprites[layer][6];
				cell.sr.transform.rotation = Quaternion.Euler(0f, 0f, 270f);
				break;
		}
	}

	// 0    empty					// 8    L
	// 1    alone					// 9    L 270
	// 2    one						// 10   L 180°
	// 3    one 270°				// 11   L 90
	// 4    one 180°				// 12   T
	// 5    one 90°					// 13   T 270
	// 6    I vertical				// 14   T 180°
	// 7    I horizontal			// 15   T 90

	// 16   X	// 17 X empty NE // 18 X Empty 270 // 19 X empty 180 // 20 X empty 90

	private int ComputeCellValue(int i, int j, int layer) {
		// ça peut être sympa de faire un joli algo ici
		Cell cell;
		int neighbours = CountNeighbours(i, j, layer);
		switch (neighbours) {
			case 0:
				return 1;
			case 1:
				if (grids[layer].TryGetValue((i + 1, j), out cell) && cell.value > 0) return 2;
				if (grids[layer].TryGetValue((i, j - 1), out cell) && cell.value > 0) return 3;
				if (grids[layer].TryGetValue((i - 1, j), out cell) && cell.value > 0) return 4;
				return 5;
			case 2:
				if (grids[layer].TryGetValue((i + 1, j), out cell) && cell.value > 0 && grids[layer].TryGetValue((i - 1, j), out cell) && cell.value > 0) return 6;
				if (grids[layer].TryGetValue((i, j - 1), out cell) && cell.value > 0 && grids[layer].TryGetValue((i, j + 1), out cell) && cell.value > 0) return 7;
				if (grids[layer].TryGetValue((i - 1, j), out cell) && cell.value > 0 && grids[layer].TryGetValue((i, j + 1), out cell) && cell.value > 0) return 8;
				if (grids[layer].TryGetValue((i + 1, j), out cell) && cell.value > 0 && grids[layer].TryGetValue((i, j + 1), out cell) && cell.value > 0) return 9;
				if (grids[layer].TryGetValue((i + 1, j), out cell) && cell.value > 0 && grids[layer].TryGetValue((i, j - 1), out cell) && cell.value > 0) return 10;
				return 11;
			case 3:
				if (grids[layer].TryGetValue((i + 1, j), out cell) && cell.value > 0 && grids[layer].TryGetValue((i, j + 1), out cell) && cell.value > 0 && grids[layer].TryGetValue((i, j - 1), out cell) && cell.value > 0) return 12;
				if (grids[layer].TryGetValue((i + 1, j), out cell) && cell.value > 0 && grids[layer].TryGetValue((i - 1, j), out cell) && cell.value > 0 && grids[layer].TryGetValue((i, j - 1), out cell) && cell.value > 0) return 13;
				if (grids[layer].TryGetValue((i, j + 1), out cell) && cell.value > 0 && grids[layer].TryGetValue((i - 1, j), out cell) && cell.value > 0 && grids[layer].TryGetValue((i, j - 1), out cell) && cell.value > 0) return 14;
				return 15;
			case 4:
				if(layer == 1)
					return 16;

				// Check diag neighbours to see if empty corner
				// We can't NOT find diag neighbours because cell has 4 neighbours and grid is rect-shape
				if (grids[layer][(i - 1, j + 1)].value == 0)
					return 17;
				if (grids[layer][(i - 1, j - 1)].value == 0)
					return 18;
				if (grids[layer][(i + 1, j - 1)].value == 0)
					return 19;
				if (grids[layer][(i + 1, j + 1)].value == 0)
					return 20;
				return 16;
		}
		return 0;
	}

	private int CountNeighbours(int i, int j, int layer) {
		Cell cell;
		int sum = 0;
		if (grids[layer].TryGetValue((i, j + 1), out cell) && cell.value != 0) sum++;
		if (grids[layer].TryGetValue((i, j - 1), out cell) && cell.value != 0) sum++;
		if (grids[layer].TryGetValue((i + 1, j), out cell) && cell.value != 0) sum++;
		if (grids[layer].TryGetValue((i - 1, j), out cell) && cell.value != 0) sum++;
		return sum;
	}

	public void ClearAllGrid() {
		for (int i = 0; i < grids.Length; i++)
			ClearGrid(i);

		PlayerSpawn.SetActive(false);
		PatientSpawn.SetActive(false);
		LevelDiseasesController.instance.DeleteAll();		
	}

	public void ClearGrid(int layer) {
		foreach (KeyValuePair<(int, int), Cell> cell in grids[layer]) {
			cell.Value.value = 0;
			ChangeSprite(cell.Value, layer);
			if (layer == 1) fullWallsGrid[(cell.Key.Item1, cell.Key.Item2)].sprite = null;
		}
	}

	public void RefreshAllGrid(bool recompute = false) {
		for (int i = 0; i < grids.Length; i++) {
			RefreshGrid(i, recompute);
		}
	}

	public void RefreshGrid(int layer, bool recompute = false) {
		foreach (KeyValuePair<(int, int), Cell> cell in grids[layer]) {
			if (recompute && cell.Value.value > 0)
				cell.Value.value = ComputeCellValue(cell.Key.Item1, cell.Key.Item2, layer);
			ChangeSprite(cell.Value, layer);
			if (layer == 1 && cell.Value.value > 0)
				fullWallsGrid[(cell.Key.Item1, cell.Key.Item2)].sprite = fullWallSprites[cell.Value.value - 1];
		}
	}

	public void ResizeGrid(int rows, int columns) {
		for (int i = 0; i < Mathf.Max(rows, this.rows); i++)
			for (int j = 0; j < Mathf.Max(columns, this.columns); j++)
				if (i >= rows || j >= columns)
					for (int x = 0; x < grids.Length; x++)
						DeleteCell(x, i, j);

		for (int i = 0; i < Mathf.Max(rows, this.rows); i++)
			for (int j = 0; j < Mathf.Max(columns, this.columns); j++)
				if (i >= this.rows || j >= this.columns)
					for (int x = 0; x < grids.Length; x++)
						InitCell(x, i, j);

		this.rows = rows;
		this.columns = columns;
		ResetCamera();
	}

	private void InitGrid() {
		for(int x = 0; x < grids.Length; x++) {
			grids[x] = new Dictionary<(int, int), Cell>();
			for (int i = 0; i < rows; i++)
				for (int j = 0; j < columns; j++)
					InitCell(x, i, j);
		}

		ResetCamera();
	}

	private void InitCell(int x, int i, int j) {
		GameObject newGO = new GameObject(i + "-" + j, typeof(SpriteRenderer), typeof(BoxCollider2D));
		newGO.transform.SetParent(CellsParents[x]);
		newGO.AddComponent<CellController>();
		newGO.GetComponent<CellController>().Setup(i, j);
		newGO.transform.position = new Vector3(j / size, -i / size, 0);
		SpriteRenderer sr = newGO.GetComponent<SpriteRenderer>();
		sr.sortingOrder = x-2;

		newGO.GetComponent<BoxCollider2D>().size = new Vector2(1f / size, 1f / size);

		grids[x].Add((i, j), new Cell(newGO, sr));
	}

	private void DeleteCell(int x, int i, int j) {
		Cell cell;
		if(grids[x].TryGetValue((i, j), out cell)) {
			Destroy(cell.go);
			grids[x].Remove((i, j));
		}
	}

	public void SetFollower(Sprite followerSprite) {
		drawType = DrawType.levelObject;
		followerSR.sprite = followerSprite;
	}

	public void UnsetFollower() {
		drawType = DrawType.none;
		followerSR.sprite = null;
	}

	private void ResetCamera() {
		Camera.main.transform.position = new Vector3((columns - 1) / 2f / size, (1 - rows) / 2f / size, -10f);
	}

	void OnEnable() {
		ResetCamera();

	}
}
