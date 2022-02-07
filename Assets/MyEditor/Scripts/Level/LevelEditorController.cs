using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public enum DrawType {
	none,
	floor,
	eraseFloor,
	fillFloor,
	walls,
	eraseWalls,
	fillWalls,
	playerSpawn,
	patientSpawn,
	levelObject
}

public class Cell {
	public Cell(GameObject go, SpriteRenderer sr) { this.go = go; this.sr = sr; value = 0; }

	public GameObject go;
	public int value;
	public SpriteRenderer sr;
}

public class LevelEditorController : MonoBehaviour {
	public static LevelEditorController instance;

	[SerializeField] private InputField rowsIF;
	[SerializeField] private InputField columnsIF;

	public const float size = 2f;
	public int rows { get; private set; }
	public int columns { get; private set; }
	public DrawType drawType;

	public Sprite[] floorSprites, wallSprites, fullWallSprites;
	public Sprite[][] sprites;
	public Dictionary<(int, int), Cell>[] grids { get; private set; }
	public Dictionary<(int, int), GameObject> fullWallsGrid { get; private set; }

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

		rows = Int32.Parse(rowsIF.text);
		columns = Int32.Parse(columnsIF.text);

		sprites = new Sprite[2][] { floorSprites, wallSprites };

		grids = new Dictionary<(int, int), Cell>[2];
		fullWallsGrid = new Dictionary<(int, int), GameObject>();

		CellsParents = new Transform[2];
		CellsParents[0] = transform.Find("FloorLayer");
		CellsParents[1] = transform.Find("WallsLayer");
		WallsParents = new Transform[2];
		WallsParents[0] = CellsParents[1];
		WallsParents[1] = transform.Find("FullWallsLayer");

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
	private void Start() { InitGrid(); }

	private void Update() {

		if(drawType == DrawType.levelObject) {
            Follower.transform.position = Camera.main.ScreenToWorldPoint(Input.mousePosition) + new Vector3(1f, -1f, 10f);
		}

		if ((Input.GetMouseButtonDown(0) || Input.GetMouseButton(0)) && !GlobalFunctions.DoesHitUI()) {
			switch (drawType) {
				case DrawType.floor:
				case DrawType.eraseFloor:
				case DrawType.fillFloor:
				case DrawType.walls:
				case DrawType.eraseWalls:
				case DrawType.fillWalls:
					RaycastHit2D hit2D = Physics2D.GetRayIntersection(Camera.main.ScreenPointToRay(Input.mousePosition), Mathf.Infinity, LayerMask.GetMask("Cell")); ;
					if (hit2D.collider != null) {
						int i = hit2D.collider.gameObject.GetComponent<CellController>().row;
						int j = hit2D.collider.gameObject.GetComponent<CellController>().column;
						switch (drawType) {
							case DrawType.floor:
								ClickedCell(0, i, j, true);
                                break;
							case DrawType.eraseFloor:
								ClickedCell(0, i, j, true, true);
								break;
							case DrawType.fillFloor:
								FloorFill(i, j);
								break;
							case DrawType.walls:
								ClickedCell(1, i, j, true);
								break;
							case DrawType.eraseWalls:
								ClickedCell(1, i, j, true, true);
								break;
							case DrawType.fillWalls:
								WallFill(i, j);
								break;
						}
					}
					break;
				case DrawType.playerSpawn:
				case DrawType.patientSpawn:
				case DrawType.levelObject:
					if (Input.GetMouseButtonDown(0)) {
						Vector3 worldMousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition) + new Vector3(0f, 0f, 10f);
						switch (drawType) {
							case DrawType.playerSpawn:
								SetPlayerSpawn(worldMousePos);
								break;
							case DrawType.patientSpawn:
								SetPatientSpawn(worldMousePos);
								break;
							case DrawType.levelObject:
								RaycastHit2D hit = Physics2D.GetRayIntersection(Camera.main.ScreenPointToRay(Input.mousePosition), Mathf.Infinity, LayerMask.GetMask("LevelObjects")); ;
								if(!hit.collider) {
									GameObject newGO = Instantiate(Follower, worldMousePos, Quaternion.identity, ObjectsParent);
									newGO.AddComponent<BoxCollider2D>();
									newGO.layer = LayerMask.NameToLayer("LevelObjects");
								} else {
									// We clicked on an object, shall we select it?
                                }
								break;
						}
					}
					break;			
			}
		}
	}

	// -------------- SPAWNS
	public void SetPlayerSpawn(Vector3 pos) {
		if (!PlayerSpawn.activeSelf)
			PlayerSpawn.SetActive(true); 
		PlayerSpawn.transform.position = pos; 
	}
	public void SetPatientSpawn(Vector3 pos) {
		if (!PatientSpawn.activeSelf)
			PatientSpawn.SetActive(true); 
		PatientSpawn.transform.position = pos; 
	}
	// -------------- END SPAWNS
	// -------------- VISIBILITY SWITCHERS
	public void SwitchObjectsVisibility() { ObjectsParent.gameObject.SetActive(!ObjectsParent.gameObject.activeSelf); }
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
	// -------------- END VISIBILITY SWITCHERS
	// -------------- CELL CLICK
	public void ClickedCell(int layer, int i, int j, bool clicked = false, bool reset = false) {
		if (grids[layer].TryGetValue((i, j), out Cell cell)) {
			if ((drawType == DrawType.walls || drawType == DrawType.eraseWalls || drawType == DrawType.fillWalls) 
				&& wallVisibility == WallVisibility.none) SwitchWallsVisibility();
			if (!reset) {
				if ((cell.value != 0 || clicked)) {
					int newValue = ComputeCellValue(i, j, layer);
					if (newValue != cell.value) {
						cell.value = newValue;
						ChangeSprite(cell, layer);
						if (layer == 1 && newValue > 0) fullWallsGrid[(i, j)].GetComponent<SpriteRenderer>().sprite = fullWallSprites[newValue - 1];
						Propagate(layer, i, j);
					}
				}
			} else if (cell.value != 0) {
				cell.value = 0;
				ChangeSprite(cell, layer);
				if (layer == 1) fullWallsGrid[(i, j)].GetComponent<SpriteRenderer>().sprite = null;
				Propagate(layer, i, j);
			}
		}
	}   
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
				if (layer == 1)
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
	private void Propagate(int layer, int i, int j) {
		ClickedCell(layer, i + 1, j, false, false);
		ClickedCell(layer, i - 1, j, false, false);
		ClickedCell(layer, i, j + 1, false, false);
		ClickedCell(layer, i, j - 1, false, false);
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
	// -------------- END CELL CLICK
	// -------------- FILLERS
	private void FloorFill(int i, int j) {
		// Only work if we clicked on an empty cell
		if (!grids[0].TryGetValue((i, j), out Cell cell) || cell.value > 0)
			return;

		int newValue = ComputeCellValue(i, j, 0);
		if (cell.value != newValue) {
			cell.value = newValue;
			//ChangeSprite(cell, 0);

			// Propagate
			FloorFill(i + 1, j);
			FloorFill(i - 1, j);
			FloorFill(i, j + 1);
			FloorFill(i, j - 1);
		}

		RefreshGrid(0, true);
	}
	private void WallFill(int i, int j) {
		// If we clicked on void cell, abort mission
		if (!grids[0].TryGetValue((i, j), out Cell cell) || cell.value == 0)
			return;

		while (grids[0].TryGetValue((i-1, j), out cell) && cell.value > 0) { i--; }
		// We found an edge!! Save it for later
		int startingX = i, startingY = j, oldI = -1, oldJ = -1, x, y;
		do {
			grids[1][(i, j)].value = 1;

			x = i;
			y = j;

			if (!grids[0].TryGetValue((i + 1, j), out cell) || cell.value == 0) 
				(i, j) = tryCells((i, j - 1), (i, j + 1), (i - 1, j), i, j, oldI, oldJ);
			 else if (!grids[0].TryGetValue((i, j - 1), out cell) || cell.value == 0) 
				(i, j) = tryCells((i + 1, j), (i - 1, j), (i, j + 1), i, j, oldI, oldJ);
			 else if (!grids[0].TryGetValue((i - 1, j), out cell) || cell.value == 0) 
				(i, j) = tryCells((i, j - 1), (i, j + 1), (i + 1, j), i, j, oldI, oldJ);
			 else if (!grids[0].TryGetValue((i, j + 1), out cell) || cell.value == 0) 
				(i, j) = tryCells((i - 1, j), (i + 1, j), (i, j + 1), i, j, oldI, oldJ);
			 else
				(i, j) = CheckDiagonals(i, j, oldI, oldJ);
			
			oldI = x;
			oldJ = y;

		} while (!(i == startingX && j == startingY) && !(i == -1 && j == -1));
		RefreshGrid(1, true);
	}
	private (int, int) tryCells((int i, int j) first, (int i, int j) second, (int i, int j) third, int i, int j, int oldI, int oldJ) {
		int x = -1, y = -1;
		if (!grids[0].TryGetValue((first.i, first.j), out Cell cell) || cell.value == 0 || !tryDirection(first.i, first.j, oldI, oldJ, i, j, out x, out y))
			if (!grids[0].TryGetValue((second.i, second.j), out cell) || cell.value == 0 || !tryDirection(second.i, second.j, oldI, oldJ, i, j, out x, out y))
				if (!grids[0].TryGetValue((third.i, third.j), out cell) || cell.value == 0 || !tryDirection(third.i, third.j, oldI, oldJ, i, j, out x, out y))
					Debug.Log("No correct path found");

		return (x, y);
	}
	private (int, int) CheckDiagonals(int i, int j, int oldI, int oldJ) {
		int x = -1, y = -1;
		if (grids[0].TryGetValue((i + 1, j + 1), out Cell cell) && cell.value == 0) {
			if (!tryDirection(i + 1, j, oldI, oldJ, i, j, out x, out y))
				tryDirection(i, j + 1, oldI, oldJ, i, j, out x, out y);
		} else if (grids[0].TryGetValue((i + 1, j - 1), out cell) && cell.value == 0) {
			if (!tryDirection(i + 1, j, oldI, oldJ, i, j, out x, out y))
				tryDirection(i, j - 1, oldI, oldJ, i, j, out x, out y);
		} else if (grids[0].TryGetValue((i - 1, j - 1), out cell) && cell.value == 0) {
			if (!tryDirection(i - 1, j, oldI, oldJ, i, j, out x, out y))
				tryDirection(i, j - 1, oldI, oldJ, i, j, out x, out y);
		} else if (grids[0].TryGetValue((i - 1, j + 1), out cell) && cell.value == 0) {
			if (!tryDirection(i - 1, j, oldI, oldJ, i, j, out x, out y))
				tryDirection(i, j + 1, oldI, oldJ, i, j, out x, out y);
		}

		return (x, y);
	}
	private bool tryDirection(int newI, int newJ, int oldI, int oldJ, int currentI, int currentJ, out int value1, out int value2) {
		if (newI != oldI || newJ != oldJ) {
			value1 = newI;
			value2 = newJ;

			return true;
		}
		value1 = -1;
		value2 = -1;
		return false;
	}
	// -------------- END FILLERS
	// -------------- GRID MANAGEMENT
	private void InitGrid() {
		for (int x = 0; x < grids.Length; x++) {
			grids[x] = new Dictionary<(int, int), Cell>();
			for (int i = 0; i < rows; i++) {
				for (int j = 0; j < columns; j++) {
					InitCell(x, i, j);

					if(x == 1) {
						GameObject newCell = new GameObject(i + "-" + j, typeof(SpriteRenderer));
						newCell.transform.SetParent(WallsParents[1]);
						newCell.transform.position = new Vector3(j / size, -i / size, 0);
						fullWallsGrid.Add((i, j), newCell);
					}
				}
			}
		}

		ResetCamera();
	}
	private void InitCell(int x, int i, int j) {
		GameObject newGO = new GameObject(i + "-" + j, typeof(SpriteRenderer), typeof(BoxCollider2D));
		newGO.layer = LayerMask.NameToLayer("Cell");
		newGO.transform.SetParent(CellsParents[x]);
		newGO.AddComponent<CellController>();
		newGO.GetComponent<CellController>().Setup(i, j);
		newGO.transform.position = new Vector3(j / size, -i / size, 0);
		SpriteRenderer sr = newGO.GetComponent<SpriteRenderer>();
		sr.sortingOrder = x - 2;

		newGO.GetComponent<BoxCollider2D>().size = new Vector2(1f / size, 1f / size);

		grids[x].Add((i, j), new Cell(newGO, sr));
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
			if (layer == 1) fullWallsGrid[(cell.Key.Item1, cell.Key.Item2)].GetComponent<SpriteRenderer>().sprite = null;
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
				fullWallsGrid[(cell.Key.Item1, cell.Key.Item2)].GetComponent<SpriteRenderer>().sprite = fullWallSprites[cell.Value.value - 1];
		}
	}
	public void ResizeGrid() {
		int newRows = Int32.Parse(rowsIF.text);
		int newColumns = Int32.Parse(columnsIF.text);
		if(newRows != rows || newColumns != columns) {
			ResizeGrid(newRows, newColumns);
		}
	}
	public void ResizeGrid(int rows, int columns) {
		ClearAllGrid();
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
	private void DeleteCell(int x, int i, int j) {
		Cell cell;
		GameObject fullWallGO;
		if (grids[x].TryGetValue((i, j), out cell)) {
			Destroy(cell.go);
			grids[x].Remove((i, j));
			if(fullWallsGrid.TryGetValue((i, j), out fullWallGO)) {
				Destroy(fullWallGO);
				fullWallsGrid.Remove((i, j));
			}
		}
	}
	// -------------- END GRID MANAGEMENT
	// -------------- MISC
	public void SetFollower(Sprite followerSprite) {
		if(followerSR.sprite != followerSprite) {
			drawType = DrawType.levelObject;
			followerSR.sprite = followerSprite;
		} else {
			followerSR.sprite = null;
			drawType = DrawType.none;
        }
	}
	public void UnsetFollower() {
		drawType = DrawType.none;
		followerSR.sprite = null;
	}
	private void ResetCamera() {
		Camera.main.transform.position = new Vector3((columns - 1) / 2f / size, (1 - rows) / 2f / size, -10f);
	}
	void OnEnable() { ResetCamera(); }
}
