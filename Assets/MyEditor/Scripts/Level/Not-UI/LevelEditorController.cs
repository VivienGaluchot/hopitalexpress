using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;
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
	public InputField levelTime;

	public const float size = 2f;
	public int rows { get; private set; }
	public int columns { get; private set; }
	public DrawType drawType { get; private set; }

	public Sprite[] floorSprites, wallSprites;
	public Sprite[] fullWallSprites;
	public Dictionary<(int, int), Cell> floorGrid { get; private set; }
	public Dictionary<(int, int), Cell> wallGrid { get; private set; }
	public Dictionary<(int, int), GameObject> fullWallsGrid { get; private set; }
	private int wallColor;

	private Transform FloorCellsParent, WallCellsParent;
	private Transform[] WallsParents;
	public Transform ObjectsParent { get; private set; }
	
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
	private GameObject Follower, FollowerChild;
	private Vector3 followerOffset;
	private SpriteRenderer followerSR;
	private objectsParams followerParams;

	public class objectsParams {
		public objectsParams() { }
		public objectsParams(string path, string prefabTag) { this.path = path; this.prefabTag = prefabTag; }
		public string path, prefabTag;
	}

	public List<GameObject> ObjectsList { get; private set; }
	private GameObject clickedGO;
	private Vector3 clickedGOffset = Vector3.zero;
	private bool isDragging;

	[SerializeField] private GameObject[] Infos;
	private InfoType currentInfo;

	public enum InfoType {
		seat,
		container
    }

	private void Awake() { 
		instance = this;
		drawType = DrawType.none;
		rows = Int32.Parse(rowsIF.text);
		columns = Int32.Parse(columnsIF.text);

		floorGrid = new Dictionary<(int, int), Cell>();
		wallGrid = new Dictionary<(int, int), Cell>();
		fullWallsGrid = new Dictionary<(int, int), GameObject>();

		FloorCellsParent = transform.Find("FloorLayer");
		WallCellsParent = transform.Find("WallsLayer");
		WallsParents = new Transform[2];
		WallsParents[0] = WallCellsParent;
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
		Follower = new GameObject("Follower");
		Follower.transform.SetParent(transform);
		FollowerChild = new GameObject("Sprite", typeof(SpriteRenderer));
		FollowerChild.transform.SetParent(Follower.transform);
		followerSR = FollowerChild.GetComponent<SpriteRenderer>();
		followerOffset = Vector3.zero;
		followerParams = new objectsParams();

		ObjectsList = new List<GameObject>();
		foreach (GameObject Info in Infos) Info.SetActive(false);
	}
	private void Start() { InitGrids(); }

	private void Update() {
		if(Input.GetKeyDown("escape")) {
			UnsetFollower();
			DrawMenuController.instance.Unclick();
			StopDisplayInfos();
		}

		if (clickedGO && drawType != DrawType.none) clickedGO = null;

		Vector3 worldMousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition) + new Vector3(0f, 0f, 10f);

		switch (drawType) {
			case DrawType.floor:
			case DrawType.eraseFloor:
			case DrawType.fillFloor:
			case DrawType.walls:
			case DrawType.eraseWalls:
			case DrawType.fillWalls:
				if ((Input.GetMouseButtonDown(0) || Input.GetMouseButton(0)) && !GlobalFunctions.DoesHitUI()) {
					RaycastHit2D hit2D = Physics2D.GetRayIntersection(Camera.main.ScreenPointToRay(Input.mousePosition), Mathf.Infinity, LayerMask.GetMask("Cell")); ;
					if (hit2D.collider != null) {
						int i = hit2D.collider.gameObject.GetComponent<CellController>().row;
						int j = hit2D.collider.gameObject.GetComponent<CellController>().column;
						switch (drawType) {
							case DrawType.floor:
								FloorClicked(i, j);
								break;
							case DrawType.eraseFloor:
								FloorClicked(i, j, true);
								break;
							case DrawType.fillFloor:
								FloorFill(i, j, true);
								break;
							case DrawType.walls:
								WallClicked(i, j);
								break;
							case DrawType.eraseWalls:
								WallClicked(i, j, true);
								break;
							case DrawType.fillWalls:
								WallFill(i, j);
								break;
						}
					}
				}
				break;
			case DrawType.playerSpawn:
			case DrawType.patientSpawn:
				if (Input.GetMouseButtonDown(0) && !GlobalFunctions.DoesHitUI()) {
					if (drawType == DrawType.playerSpawn) SetPlayerSpawn(worldMousePos);
					else if (drawType == DrawType.patientSpawn) SetPatientSpawn(worldMousePos);
				}
				break;
			case DrawType.levelObject:
				Follower.transform.position = worldMousePos + followerOffset;
				bool ctrled = Input.GetKey("left ctrl");
				followerSR.sortingOrder = ctrled ? 10 : 0;
				if (Input.GetMouseButtonDown(0) && !GlobalFunctions.DoesHitUI()) {
					RaycastHit2D[] hits = Physics2D.GetRayIntersectionAll(Camera.main.ScreenPointToRay(Input.mousePosition), Mathf.Infinity, LayerMask.GetMask("LevelObjects")); ;
					if (hits.Length == 0 || ctrled) {
						CreateLevelObject(worldMousePos, ctrled, hits);
					}
				}
				break;
			case DrawType.none:
				if (clickedGO) {
					if (Input.GetKeyDown("delete")) {
						DeleteClickedGO();
					} else if (Input.GetMouseButtonUp(0)) {
						isDragging = false;
						// We dragged an object to the ui, I guess that mean we want to remove it
						if (GlobalFunctions.DoesHitUI()) { DeleteClickedGO(); }
					} else if (Input.GetMouseButton(0) && isDragging) {
						clickedGO.transform.position = Camera.main.ScreenToWorldPoint(Input.mousePosition) + new Vector3(0f, 0f, 10f) + clickedGOffset;
						if (clickedGO.GetComponent<LevelObjectController>().isChild) {
							clickedGO.transform.Find("Sprite").GetComponent<SpriteRenderer>().sortingOrder = Mathf.RoundToInt(clickedGO.transform.localPosition.y * -100);
						}
					}
				}

				if ((Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1)) && !GlobalFunctions.DoesHitUI()) {
					clickedGO = null;
					StopDisplayInfos();
					RaycastHit2D hit = Physics2D.GetRayIntersection(Camera.main.ScreenPointToRay(Input.mousePosition), Mathf.Infinity, LayerMask.GetMask("LevelObjects")); ;
					if (hit.collider) {
						if(Input.GetMouseButtonDown(0)) {
							clickedGO = hit.collider.gameObject;
							clickedGOffset = hit.transform.position - worldMousePos;
							isDragging = true;
						} else {
							LevelObjectController loc = hit.collider.gameObject.GetComponent<LevelObjectController>();
							if (loc) {
								if (loc.prefabTag == "Seat") {
									currentInfo = InfoType.seat;
									Infos[(int)currentInfo].GetComponentInChildren<Toggle>().onValueChanged.RemoveAllListeners();
									Infos[(int)currentInfo].GetComponentInChildren<Toggle>().isOn = loc.isWelcomeSeat;
									Infos[(int)currentInfo].transform.position = Input.mousePosition;
									Infos[(int)currentInfo].SetActive(true);
									Infos[(int)currentInfo].GetComponentInChildren<Toggle>().onValueChanged.AddListener((value) => { loc.isWelcomeSeat = value; });
								} else if(loc.prefabTag == "Container") {
									currentInfo = InfoType.container;
									Infos[(int)currentInfo].GetComponentInChildren<InputField>().onValueChanged.RemoveAllListeners();
									Infos[(int)currentInfo].GetComponentInChildren<InputField>().text = loc.containerTime.ToString();
									Infos[(int)currentInfo].transform.position = Input.mousePosition;
									Infos[(int)currentInfo].SetActive(true);
									Infos[(int)currentInfo].GetComponentInChildren<InputField>().onValueChanged.AddListener((value) => { loc.containerTime = (value != "" ? Single.Parse(value) : 0f); });
								}
							}
						}
					}
				}
				break;
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
	public void DrawTypeToSpawn(int newDT) {
		UnsetFollower();
		StopDisplayInfos();
		drawType = (DrawType) newDT;
	}
	private void UnsetSpawns() {
		PlayerSpawn.SetActive(false);
		PatientSpawn.SetActive(false);
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
	private void FloorClicked(int i, int j, bool erase = false) {
		if (floorGrid.TryGetValue((i, j), out Cell cell)) {
			// We found a cell, what do we do?
			if(erase) {
				if(cell.value != 0) {
					cell.value = 0;
					cell.sr.sprite = null;
					// Update neighbours sprites
					UpdateFloorNeighbours(i, j);
				}
			} else {
				UpdateFloorCellSprite(cell, i, j);
			}
		}
	}
	private void UpdateFloorNeighbours(int i, int j) {
		if (floorGrid.TryGetValue((i + 1, j), out Cell cell) && cell.value > 0) { UpdateFloorCellSprite(cell, i + 1, j); }
		if (floorGrid.TryGetValue((i - 1, j), out cell) && cell.value > 0) { UpdateFloorCellSprite(cell, i - 1, j); }
		if (floorGrid.TryGetValue((i, j + 1), out cell) && cell.value > 0) { UpdateFloorCellSprite(cell, i, j + 1); }
		if (floorGrid.TryGetValue((i, j - 1), out cell) && cell.value > 0) { UpdateFloorCellSprite(cell, i, j - 1); }
	}
	private void UpdateFloorCellSprite(Cell cell, int i, int j) {
		int newValue = ComputeCellValue(floorGrid, i, j, true); // floor need to check corners
		if (cell.value != newValue) {
			cell.value = newValue;
			cell.sr.sprite = floorSprites[cell.value-1];
			// Update neighbours sprites
			UpdateFloorNeighbours(i, j);
		}
	}
	private void WallClicked(int i, int j, bool erase = false) {
		if (wallGrid.TryGetValue((i, j), out Cell cell)) {
			// We found a cell, what do we do?
			if(wallVisibility == WallVisibility.none) SwitchWallsVisibility();
			if (erase) {
				if (cell.value != 0) {
					cell.value = 0;
					cell.sr.sprite = null;
					fullWallsGrid[(i, j)].GetComponent<SpriteRenderer>().sprite = null;
					// Update neighbours sprites
					UpdateWallNeighbours(i, j);
				}
			} else {
				UpdateWallCellSprite(cell, i, j, true);
			}
		}
	}
	private void UpdateWallNeighbours(int i, int j) {
		if (wallGrid.TryGetValue((i + 1, j), out Cell cell) && cell.value > 0) { UpdateWallCellSprite(cell, i + 1, j); }
		if (wallGrid.TryGetValue((i - 1, j), out cell) && cell.value > 0) { UpdateWallCellSprite(cell, i - 1, j); }
		if (wallGrid.TryGetValue((i, j + 1), out cell) && cell.value > 0) { UpdateWallCellSprite(cell, i, j + 1); }
		if (wallGrid.TryGetValue((i, j - 1), out cell) && cell.value > 0) { UpdateWallCellSprite(cell, i, j - 1); }
	}
	// Cette fonction peut faire appara?tre un sprite sur une case vide, attention ? ce qu'on lui donne !
	private void UpdateWallCellSprite(Cell cell, int i, int j, bool colorUpdate = false) {
		int newValue = ComputeCellValue(wallGrid, i, j);
		if(colorUpdate) {
			if (cell.value != newValue + wallColor) {
				int oldValue = cell.value;
				cell.value = newValue + wallColor;
				cell.sr.sprite = wallSprites[newValue - 1];
				fullWallsGrid[(i, j)].GetComponent<SpriteRenderer>().sprite = fullWallSprites[newValue + wallColor - 1];
				// Update neighbours sprites
				if (oldValue % 16 != newValue)
					UpdateWallNeighbours(i, j);
			}
		} else {
			if(cell.value%16 != newValue%16) {
				int myWallColor = ((cell.value-1) / 16)*16;
				cell.value = newValue + myWallColor;
				cell.sr.sprite = wallSprites[newValue - 1];
				fullWallsGrid[(i, j)].GetComponent<SpriteRenderer>().sprite = fullWallSprites[newValue + myWallColor - 1];
			}
		}		
	}
	private int ComputeCellValue(Dictionary<(int, int), Cell> grid, int i, int j, bool checkDiag = false) {
		// ?a peut ?tre sympa de faire un joli algo ici
		Cell cell;
		int neighbours = CountNeighbours(grid, i, j);
		switch (neighbours) {
			case 0:
				return 1;
			case 1:
				if (grid.TryGetValue((i + 1, j), out cell) && cell.value > 0) return 2;
				if (grid.TryGetValue((i, j - 1), out cell) && cell.value > 0) return 3;
				if (grid.TryGetValue((i - 1, j), out cell) && cell.value > 0) return 4;
				return 5;
			case 2:
				if (grid.TryGetValue((i + 1, j), out cell) && cell.value > 0 && grid.TryGetValue((i - 1, j), out cell) && cell.value > 0) return 6;
				if (grid.TryGetValue((i, j - 1), out cell) && cell.value > 0 && grid.TryGetValue((i, j + 1), out cell) && cell.value > 0) return 7;
				if (grid.TryGetValue((i - 1, j), out cell) && cell.value > 0 && grid.TryGetValue((i, j + 1), out cell) && cell.value > 0) return 8;
				if (grid.TryGetValue((i + 1, j), out cell) && cell.value > 0 && grid.TryGetValue((i, j + 1), out cell) && cell.value > 0) return 9;
				if (grid.TryGetValue((i + 1, j), out cell) && cell.value > 0 && grid.TryGetValue((i, j - 1), out cell) && cell.value > 0) return 10;
				return 11;
			case 3:
				if (grid.TryGetValue((i + 1, j), out cell) && cell.value > 0 && grid.TryGetValue((i, j + 1), out cell) && cell.value > 0 && grid.TryGetValue((i, j - 1), out cell) && cell.value > 0) return 12;
				if (grid.TryGetValue((i + 1, j), out cell) && cell.value > 0 && grid.TryGetValue((i - 1, j), out cell) && cell.value > 0 && grid.TryGetValue((i, j - 1), out cell) && cell.value > 0) return 13;
				if (grid.TryGetValue((i, j + 1), out cell) && cell.value > 0 && grid.TryGetValue((i - 1, j), out cell) && cell.value > 0 && grid.TryGetValue((i, j - 1), out cell) && cell.value > 0) return 14;
				return 15;
			default:
				if (!checkDiag)
					return 16;

				// Check diag neighbours to see if empty corner
				// We can't NOT find diag neighbours because cell has 4 neighbours and grid is rect-shape
				if (grid[(i - 1, j + 1)].value == 0)
					return 17;
				if (grid[(i - 1, j - 1)].value == 0)
					return 18;
				if (grid[(i + 1, j - 1)].value == 0)
					return 19;
				if (grid[(i + 1, j + 1)].value == 0)
					return 20;
				return 16;
		}
	}
	private int CountNeighbours(Dictionary<(int, int), Cell> grid, int i, int j) {
		int sum = 0;
		if (grid.TryGetValue((i, j + 1), out Cell cell) && cell.value != 0) sum++;
		if (grid.TryGetValue((i, j - 1), out cell) && cell.value != 0) sum++;
		if (grid.TryGetValue((i + 1, j), out cell) && cell.value != 0) sum++;
		if (grid.TryGetValue((i - 1, j), out cell) && cell.value != 0) sum++;
		return sum;
	}
	// -------------- END CELL CLICK
	// -------------- FILLERS
	private void FloorFill(int i, int j, bool first = false) {
		// Only work if we clicked on an empty cell
		if (!floorGrid.TryGetValue((i, j), out Cell cell) || cell.value > 0)
			return;

		int newValue = ComputeCellValue(floorGrid, i, j, true);
		if (cell.value != newValue) {
			cell.value = newValue;

			// Propagate
			FloorFill(i + 1, j);
			FloorFill(i - 1, j);
			FloorFill(i, j + 1);
			FloorFill(i, j - 1);
		}

		if(first)
			RefreshFloorGrid(true);
	}
	private void WallFill(int i, int j) {
		// If we clicked on void cell, abort mission
		if (!floorGrid.TryGetValue((i, j), out Cell cell) || cell.value == 0)
			return;

		// looking for an edge
		while (floorGrid.TryGetValue((i-1, j), out cell) && cell.value > 0) { i--; }
		int startingX = i, startingY = j, oldI = -1, oldJ = -1, x, y;
		do {
			wallGrid[(i, j)].value = wallColor+1;

			x = i;
			y = j;

			if (!floorGrid.TryGetValue((i + 1, j), out cell) || cell.value == 0)
				(i, j) = tryCells((i, j - 1), (i, j + 1), (i - 1, j), i, j, oldI, oldJ);
			else if (!floorGrid.TryGetValue((i, j - 1), out cell) || cell.value == 0)
				(i, j) = tryCells((i + 1, j), (i - 1, j), (i, j + 1), i, j, oldI, oldJ);
			else if (!floorGrid.TryGetValue((i - 1, j), out cell) || cell.value == 0)
				(i, j) = tryCells((i, j - 1), (i, j + 1), (i + 1, j), i, j, oldI, oldJ);
			else if (!floorGrid.TryGetValue((i, j + 1), out cell) || cell.value == 0)
				(i, j) = tryCells((i - 1, j), (i + 1, j), (i, j + 1), i, j, oldI, oldJ);
			else
				(i, j) = CheckDiagonals(i, j, oldI, oldJ);

			oldI = x;
			oldJ = y;

		} while (!(i == startingX && j == startingY) && !(i == -1 && j == -1));

		RefreshWallGrid(true);
	}
	private (int, int) tryCells((int i, int j) first, (int i, int j) second, (int i, int j) third, int i, int j, int oldI, int oldJ) {
		int x = -1, y = -1;
		if (!floorGrid.TryGetValue((first.i, first.j), out Cell cell) || cell.value == 0 || !tryDirection(first.i, first.j, oldI, oldJ, i, j, out x, out y))
			if (!floorGrid.TryGetValue((second.i, second.j), out cell) || cell.value == 0 || !tryDirection(second.i, second.j, oldI, oldJ, i, j, out x, out y))
				if (!floorGrid.TryGetValue((third.i, third.j), out cell) || cell.value == 0 || !tryDirection(third.i, third.j, oldI, oldJ, i, j, out x, out y))
					Debug.Log("No correct path found");

		return (x, y);
	}
	private (int, int) CheckDiagonals(int i, int j, int oldI, int oldJ) {
		int x = -1, y = -1;
		if (floorGrid.TryGetValue((i + 1, j + 1), out Cell cell) && cell.value == 0) {
			if (!tryDirection(i + 1, j, oldI, oldJ, i, j, out x, out y))
				tryDirection(i, j + 1, oldI, oldJ, i, j, out x, out y);
		} else if (floorGrid.TryGetValue((i + 1, j - 1), out cell) && cell.value == 0) {
			if (!tryDirection(i + 1, j, oldI, oldJ, i, j, out x, out y))
				tryDirection(i, j - 1, oldI, oldJ, i, j, out x, out y);
		} else if (floorGrid.TryGetValue((i - 1, j - 1), out cell) && cell.value == 0) {
			if (!tryDirection(i - 1, j, oldI, oldJ, i, j, out x, out y))
				tryDirection(i, j - 1, oldI, oldJ, i, j, out x, out y);
		} else if (floorGrid.TryGetValue((i - 1, j + 1), out cell) && cell.value == 0) {
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
	private void InitGrids() {
		floorGrid = new Dictionary<(int, int), Cell>();
		wallGrid = new Dictionary<(int, int), Cell>();
		for (int i = 0; i < rows; i++) {
			for (int j = 0; j < columns; j++) {
				floorGrid.Add((i, j), InitCell(i, j, FloorCellsParent, -2));
				wallGrid.Add((i, j), InitCell(i, j, WallCellsParent));

				GameObject newCell = new GameObject(i + "-" + j, typeof(SpriteRenderer));
				newCell.transform.SetParent(WallsParents[1]);
				newCell.transform.position = new Vector3(j / size, -i / size, 0);
				fullWallsGrid.Add((i, j), newCell);
			}
		}

		ResetCamera();
	}
	private Cell InitCell(int i, int j, Transform parent, int order = 0) {
		GameObject newGO = new GameObject(i + "-" + j, typeof(SpriteRenderer), typeof(BoxCollider2D));
		newGO.layer = LayerMask.NameToLayer("Cell");
		newGO.transform.SetParent(parent);
		newGO.AddComponent<CellController>();
		newGO.GetComponent<CellController>().Setup(i, j);
		newGO.transform.position = new Vector3(j / size, -i / size, 0);
		SpriteRenderer sr = newGO.GetComponent<SpriteRenderer>();
		sr.sortingOrder = order;

		newGO.GetComponent<BoxCollider2D>().size = new Vector2(1f / size, 1f / size);

		return new Cell(newGO, sr);
	}
	public void ClearAllGrid() {
		ClearGrid(floorGrid);
		ClearGrid(wallGrid, true);
		ClearLevelObjects();
	}
	public void ClearGrid(Dictionary<(int, int), Cell> grid, bool wall = false) {
		foreach (KeyValuePair<(int, int), Cell> cell in grid) {
			cell.Value.value = 0;
			cell.Value.sr.sprite = null;
			if(wall) fullWallsGrid[(cell.Key.Item1, cell.Key.Item2)].GetComponent<SpriteRenderer>().sprite = null;
		}
	}
	public void RefreshAllGrid(bool recompute = false) {
		RefreshFloorGrid(recompute);
		RefreshWallGrid(recompute);
	}
	private void RefreshFloorGrid(bool recompute = false) {
		foreach (KeyValuePair<(int, int), Cell> cell in floorGrid) {
			if (cell.Value.value > 0) {
				if (recompute) cell.Value.value = ComputeCellValue(floorGrid, cell.Key.Item1, cell.Key.Item2);
				cell.Value.sr.sprite = floorSprites[cell.Value.value-1];
			} else
				cell.Value.sr.sprite = null;
		}
	}
	private void RefreshWallGrid(bool recompute = false) {
		foreach (KeyValuePair<(int, int), Cell> cell in wallGrid) {
			if (cell.Value.value > 0) {
				if (recompute) {
					int newValue = ComputeCellValue(wallGrid, cell.Key.Item1, cell.Key.Item2);
					int myWallColor = ((cell.Value.value - 1) / 16) * 16;
					cell.Value.value = newValue + myWallColor;
					cell.Value.sr.sprite = wallSprites[newValue - 1];
					fullWallsGrid[(cell.Key.Item1, cell.Key.Item2)].GetComponent<SpriteRenderer>().sprite = fullWallSprites[newValue + myWallColor - 1];
				} else {
					cell.Value.sr.sprite = wallSprites[(cell.Value.value - 1) % 16];
					fullWallsGrid[(cell.Key.Item1, cell.Key.Item2)].GetComponent<SpriteRenderer>().sprite = fullWallSprites[cell.Value.value - 1];
				}
			} else
				cell.Value.sr.sprite = null;
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
		for (int i = 0; i < Mathf.Max(rows, this.rows); i++) {
			for (int j = 0; j < Mathf.Max(columns, this.columns); j++) {
				if (i >= rows || j >= columns) {
					DeleteCells(i, j);
				} else if (i >= this.rows || j >= this.columns) {
					InitCell(i, j, FloorCellsParent, -2);
					InitCell(i, j, WallCellsParent, -1);

					GameObject newCell = new GameObject(i + "-" + j, typeof(SpriteRenderer));
					newCell.transform.SetParent(WallsParents[1]);
					newCell.transform.position = new Vector3(j / size, -i / size, 0);
					fullWallsGrid.Add((i, j), newCell);
				}
			}
		}

		this.rows = rows;
		this.columns = columns;
		rowsIF.text = rows.ToString();
		columnsIF.text = columns.ToString();
		ResetCamera();
	}
	private void DeleteCells(int i, int j) {
		if (floorGrid.TryGetValue((i, j), out Cell cell)) {
			Destroy(cell.go);
			floorGrid.Remove((i, j));
		}
		if (wallGrid.TryGetValue((i, j), out cell)) {
			Destroy(cell.go);
			wallGrid.Remove((i, j));
		}
		if (fullWallsGrid.TryGetValue((i, j), out GameObject fullWallGO)) {
			Destroy(fullWallGO);
			fullWallsGrid.Remove((i, j));
		}
	}
	// -------------- END GRID MANAGEMENT
	// -------------- LEVEL OBJECTS
	public void SetFollower(Sprite followerSprite, string path, string prefabTag, Vector3 childPos) {
		StopDisplayInfos();
		if (followerSR.sprite != followerSprite) {
			DrawMenuController.instance.Unclick();
			drawType = DrawType.levelObject;
			followerSR.sprite = followerSprite;
			followerParams = new objectsParams(path, prefabTag);
			FollowerChild.transform.localPosition = childPos;
			followerOffset = -childPos;
		} else {
			followerSR.sprite = null;
			drawType = DrawType.none;
		}
	}
	public void UnsetFollower() {
		drawType = DrawType.none;
		followerSR.sprite = null;
	}
	// if we want the level object to be the child of another, we say yes to setchild and we give the hits from raycasting to look for parent
	public void CreateLevelObject(Vector3 mousePos, bool setChild, RaycastHit2D[] hits) {
		// Create main object
		GameObject newGO = new GameObject(followerSR.sprite.name, typeof(BoxCollider2D), typeof(LevelObjectController));
		newGO.layer = LayerMask.NameToLayer("LevelObjects");
		newGO.transform.position = mousePos - FollowerChild.transform.localPosition;
		newGO.GetComponent<LevelObjectController>().SetParams(followerParams.path, followerParams.prefabTag);

		// Create child to get spriterenderer (because we may want to offset it)
		GameObject childForSprite = Instantiate(FollowerChild, newGO.transform);
		childForSprite.name = "Sprite";
		childForSprite.GetComponent<SpriteRenderer>().sprite = followerSR.sprite;
		childForSprite.transform.localPosition = FollowerChild.transform.localPosition;

		// Adjust boxcollider2D size to sprite size and position
		BoxCollider2D bc2D = newGO.GetComponent<BoxCollider2D>();
		bc2D.size = followerSR.size;
		Vector2 pivot = new Vector2(followerSR.sprite.pivot.x / followerSR.sprite.rect.width, followerSR.sprite.pivot.y / followerSR.sprite.rect.height);
		Vector2 offset = new Vector2(.5f, .5f) - pivot;
		bc2D.offset = (Vector2)FollowerChild.transform.localPosition + offset*followerSR.size;

		// If setChild is false or if there is no hit.collider, we use ObjectsParent. Otherwise we use the hit.collider
		// We also want that the hit.collider is not already child from someone else, no imbrication please
		bool parentFound = false;
		if (setChild) {
			foreach (RaycastHit2D hit in hits) {
				if (ObjectsList.Contains(hit.collider.gameObject)) {
					newGO.transform.SetParent(hit.collider.transform);
					if (!hit.collider.gameObject.GetComponent<SortingGroup>()) {
						hit.collider.gameObject.AddComponent<SortingGroup>();
						hit.collider.transform.Find("Sprite").GetComponent<SpriteRenderer>().sortingOrder = -10000;
					}

					// We add a child in the list of its parent, but not in the global ObjectsList
					hit.collider.gameObject.GetComponent<LevelObjectController>().childs.Add(newGO.GetComponent<LevelObjectController>());
					childForSprite.GetComponent<SpriteRenderer>().sortingOrder = Mathf.RoundToInt(newGO.transform.localPosition.y * -100);
					newGO.GetComponent<LevelObjectController>().isChild = true;
					parentFound = true;
					break;
				}
			}
		}
		if (!parentFound) { 
			newGO.transform.SetParent(ObjectsParent);
			ObjectsList.Add(newGO);
		}
	}
	private void DeleteClickedGO() {
		LevelObjectController locParent = clickedGO.transform.parent.GetComponent<LevelObjectController>();
		if (locParent) locParent.RemoveChild(clickedGO);
		if (ObjectsList.Contains(clickedGO)) ObjectsList.Remove(clickedGO);
		Destroy(clickedGO);
		clickedGO = null;
	}
	public void ClearLevelObjects() {
		foreach (GameObject go in ObjectsList) {
			Destroy(go);
		}
		ObjectsList.Clear();
		StopDisplayInfos();
	}
	// -------------- END LEVEL OBJECTS
	// -------------- MISC
	public void ClearAllLevel() {
		UnsetSpawns();
		ClearAllGrid();


		LevelDiseasesController.instance.DeleteAll();
	}
	public void SetDrawType(DrawType newDT, int color = 0) {
		UnsetFollower();
		StopDisplayInfos();
		drawType = newDT;
		// number of wall sprites per color? 16 I think
		wallColor = color * 16;
	}
	private void StopDisplayInfos() {
		Infos[(int)currentInfo].SetActive(false);
	}
	private void ResetCamera() {
		Camera.main.transform.position = new Vector3((columns - 1) / 2f / size, (1 - rows) / 2f / size, -10f);
	}
	void OnEnable() { ResetCamera(); }
}
