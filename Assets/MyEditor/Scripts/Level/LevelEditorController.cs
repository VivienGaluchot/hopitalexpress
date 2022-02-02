using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class LevelEditorController : MonoBehaviour {
	public float size;
	public int columns, rows;

	public Sprite[] floorSprites, wallSprites;
	public Sprite[][] sprites;
	public Dictionary<(int, int), Cell>[] grids { get; private set; }
	private LayerController lc;
	private Transform[] CellsParents;

	[SerializeField] private GraphicRaycaster m_Raycaster;
	[SerializeField] private PointerEventData m_PointerEventData;
	[SerializeField] private EventSystem m_EventSystem;


	private bool selectingPatientSpawn, selectingPlayerSpawn;
	[SerializeField] private Image patientButtonImage;
	[SerializeField] private Image playerButtonImage;
	[SerializeField] private GameObject PatientSpawnPrefab;
	[SerializeField] private GameObject PlayerSpawnPrefab;
	public GameObject PlayerSpawn { get; private set; }
	public GameObject PatientSpawn { get; private set; }

	private bool isFloorFillerSelected;
	[SerializeField] private Image floorFillerImage;

	private LevelDiseasesController ldc;

	public class Cell {
		public Cell(GameObject go) { this.go = go; sr = this.go.GetComponent<SpriteRenderer>(); value = 0; }
		public Cell(GameObject go, SpriteRenderer sr) { this.go = go; this.sr = sr; value = 0; }

		public GameObject go;
		public int value;
		public SpriteRenderer sr;
	}

	private void Start() {
		sprites = new Sprite[2][] { floorSprites, wallSprites };
		grids = new Dictionary<(int, int), Cell>[2];
		ldc = GetComponent<LevelDiseasesController>();
		lc = GetComponent<LayerController>();
		CellsParents = new Transform[2];
		CellsParents[0] = transform.Find("FloorLayer");
		CellsParents[1] = transform.Find("WallsLayer");
		InitGrid();
	}

	private void Update() {
		if(Input.GetMouseButtonDown(0) && !DoesHitUI()) {
			if(selectingPatientSpawn) {
				if (!PatientSpawn)
					PatientSpawn = Instantiate(PatientSpawnPrefab);
				PatientSpawn.transform.position = Camera.main.ScreenToWorldPoint(Input.mousePosition) + new Vector3(0f, 0f, 10f);
				SetPatientSpawnButton(false);
			} else if(selectingPlayerSpawn) {
				if (!PlayerSpawn)
					PlayerSpawn = Instantiate(PlayerSpawnPrefab);
				PlayerSpawn.transform.position = Camera.main.ScreenToWorldPoint(Input.mousePosition) + new Vector3(0f, 0f, 10f);
				SetPlayerSpawnButton(false);
			}
		}
	}

	public void FloorFillerClicked() {
		isFloorFillerSelected = !isFloorFillerSelected;
		floorFillerImage.color = isFloorFillerSelected ? Color.green : Color.white;
	}

	public void SetPlayerSpawn(Vector3 pos) {
		if (!PlayerSpawn)
			PlayerSpawn = Instantiate(PlayerSpawnPrefab);
		PlayerSpawn.transform.position = pos;
	}

	public void SetPatientSpawn(Vector3 pos) {
		if (!PatientSpawn)
			PatientSpawn = Instantiate(PatientSpawnPrefab);
		PatientSpawn.transform.position = pos;
	}

	public void StopSelectingSpawns() {
		SetPatientSpawnButton(false);
		SetPlayerSpawnButton(false);
	}

	private void SetPatientSpawnButton(bool oui) {
		if (oui) {
			patientButtonImage.color = Color.green;
			selectingPatientSpawn = true;
			playerButtonImage.color = Color.white;
			selectingPlayerSpawn = false;
		} else {
			patientButtonImage.color = Color.white;
			selectingPatientSpawn = false;
		}
	}

	private void SetPlayerSpawnButton(bool oui) {
		if (oui) {
			playerButtonImage.color = Color.green;
			selectingPlayerSpawn = true;
			patientButtonImage.color = Color.white;
			selectingPatientSpawn = false;
		} else {
			playerButtonImage.color = Color.white;
			selectingPlayerSpawn = false;
		}
	}

	public void SelectPatientSpawn() {
		selectingPatientSpawn = !selectingPatientSpawn;
		SetPatientSpawnButton(selectingPatientSpawn);
	}

	public void SelectPlayerSpawn() {
		selectingPlayerSpawn = !selectingPlayerSpawn;
		SetPlayerSpawnButton(selectingPlayerSpawn);
	}

	public bool DoesHitUI() {
		m_PointerEventData = new PointerEventData(m_EventSystem);
		m_PointerEventData.position = Input.mousePosition;

		List<RaycastResult> results = new List<RaycastResult>();
		m_Raycaster.Raycast(m_PointerEventData, results);

		return results.Count != 0;
	}

	public bool ClickedCell(int i, int j, bool clicked = false, bool reset = false, bool fill = false) {
		if(!selectingPatientSpawn && !selectingPlayerSpawn) {
			Cell cell;
			if(grids[lc.currentLayer].TryGetValue((i, j), out cell)) {
				if (isFloorFillerSelected) {
					// c'est niqué comme méthode, faudrait faire mieux...........
					int newValue = ComputeCellValue(i, j);
					if (cell.value == 0) {
						if (newValue != cell.value) {
							cell.value = newValue;
							ChangeSprite(cell, lc.currentLayer);

							Propagate(i, j, false, true);
						}
					}
					if (!fill) {
						isFloorFillerSelected = false;
						ClickedCell(i, j, true);
						isFloorFillerSelected = true;
					}
				} else if (!reset && (cell.value != 0 || clicked)) {
					int newValue = ComputeCellValue(i, j);
					if (newValue != cell.value) {
						cell.value = newValue;
						ChangeSprite(cell, lc.currentLayer);

						Propagate(i, j);
					}

					return true;
				} else if (reset && cell.value != 0) {
					cell.value = 0;
					ChangeSprite(cell, lc.currentLayer);
					Propagate(i, j);

					return false;
				}
			}
		}

		return reset;
	}

	private void Propagate(int i, int j, bool diag = true, bool fill = false) {
		ClickedCell(i + 1, j, false, false, fill);
		ClickedCell(i - 1, j, false, false, fill);
		ClickedCell(i, j + 1, false, false, fill);
		ClickedCell(i, j - 1, false, false, fill);
		if(diag) {
			ClickedCell(i + 1, j + 1, false, false, fill);
			ClickedCell(i + 1, j - 1, false, false, fill);
			ClickedCell(i - 1, j + 1, false, false, fill);
			ClickedCell(i - 1, j - 1, false, false, fill);
		}
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

	private int ComputeCellValue(int i, int j) {
		// ça peut être sympa de faire un joli algo ici
		Cell cell;
		int neighbours = CountNeighbours(i, j);
		switch (neighbours) {
			case 0:
				return 1;
			case 1:
				if (grids[lc.currentLayer].TryGetValue((i + 1, j), out cell) && cell.value > 0) return 2;
				if (grids[lc.currentLayer].TryGetValue((i, j - 1), out cell) && cell.value > 0) return 3;
				if (grids[lc.currentLayer].TryGetValue((i - 1, j), out cell) && cell.value > 0) return 4;
				return 5;
			case 2:
				if (grids[lc.currentLayer].TryGetValue((i + 1, j), out cell) && cell.value > 0 && grids[lc.currentLayer].TryGetValue((i - 1, j), out cell) && cell.value > 0) return 6;
				if (grids[lc.currentLayer].TryGetValue((i, j - 1), out cell) && cell.value > 0 && grids[lc.currentLayer].TryGetValue((i, j + 1), out cell) && cell.value > 0) return 7;
				if (grids[lc.currentLayer].TryGetValue((i - 1, j), out cell) && cell.value > 0 && grids[lc.currentLayer].TryGetValue((i, j + 1), out cell) && cell.value > 0) return 8;
				if (grids[lc.currentLayer].TryGetValue((i + 1, j), out cell) && cell.value > 0 && grids[lc.currentLayer].TryGetValue((i, j + 1), out cell) && cell.value > 0) return 9;
				if (grids[lc.currentLayer].TryGetValue((i + 1, j), out cell) && cell.value > 0 && grids[lc.currentLayer].TryGetValue((i, j - 1), out cell) && cell.value > 0) return 10;
				return 11;
			case 3:
				if (grids[lc.currentLayer].TryGetValue((i + 1, j), out cell) && cell.value > 0 && grids[lc.currentLayer].TryGetValue((i, j + 1), out cell) && cell.value > 0 && grids[lc.currentLayer].TryGetValue((i, j - 1), out cell) && cell.value > 0) return 12;
				if (grids[lc.currentLayer].TryGetValue((i + 1, j), out cell) && cell.value > 0 && grids[lc.currentLayer].TryGetValue((i - 1, j), out cell) && cell.value > 0 && grids[lc.currentLayer].TryGetValue((i, j - 1), out cell) && cell.value > 0) return 13;
				if (grids[lc.currentLayer].TryGetValue((i, j + 1), out cell) && cell.value > 0 && grids[lc.currentLayer].TryGetValue((i - 1, j), out cell) && cell.value > 0 && grids[lc.currentLayer].TryGetValue((i, j - 1), out cell) && cell.value > 0) return 14;
				return 15;
			case 4:
				if(lc.currentLayer == 1)
					return 16;

				// Check diag neighbours to see if empty corner
				// We can't NOT find diag neighbours because cell has 4 neighbours and grid is rect-shape
				if (grids[lc.currentLayer][(i - 1, j + 1)].value == 0)
					return 17;
				if (grids[lc.currentLayer][(i - 1, j - 1)].value == 0)
					return 18;
				if (grids[lc.currentLayer][(i + 1, j - 1)].value == 0)
					return 19;
				if (grids[lc.currentLayer][(i + 1, j + 1)].value == 0)
					return 20;
				return 16;
		}
		return 0;
	}

	private int CountNeighbours(int i, int j) {
		Cell cell;
		int sum = 0;
		if (grids[lc.currentLayer].TryGetValue((i, j + 1), out cell) && cell.value != 0) sum++;
		if (grids[lc.currentLayer].TryGetValue((i, j - 1), out cell) && cell.value != 0) sum++;
		if (grids[lc.currentLayer].TryGetValue((i + 1, j), out cell) && cell.value != 0) sum++;
		if (grids[lc.currentLayer].TryGetValue((i - 1, j), out cell) && cell.value != 0) sum++;
		return sum;
	}

	public void ClearAllGrid() {
		for (int i = 0; i < grids.Length; i++)
			ClearGrid(i);

		Destroy(PlayerSpawn);
		Destroy(PatientSpawn);
		PlayerSpawn = null;
		PatientSpawn = null;
		ldc.DeleteAll();		
	}

	public void ClearGrid(int layer) {
		foreach (KeyValuePair<(int, int), Cell> cell in grids[layer]) {
			cell.Value.value = 0;
			ChangeSprite(cell.Value, layer);
		}
	}

	public void RefreshAllGrid() {
		for (int i = 0; i < grids.Length; i++)
			RefreshGrid(i);
	}

	public void RefreshGrid(int layer) {
		foreach (KeyValuePair<(int, int), Cell> cell in grids[layer])
			ChangeSprite(cell.Value, layer);
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
		Camera.main.transform.position = new Vector3((columns - 1) / 2f / size, (1 - rows) / 2f / size, -10f);
	}

	private void InitGrid() {
		for(int x = 0; x < grids.Length; x++) {
			grids[x] = new Dictionary<(int, int), Cell>();
			for (int i = 0; i < rows; i++)
				for (int j = 0; j < columns; j++)
					InitCell(x, i, j);
		}
		
		Camera.main.transform.position = new Vector3((columns-1)/2f/size, (1-rows)/2f/size, -10f);
	}

	private void InitCell(int x, int i, int j) {
		GameObject newGO = new GameObject(i + "-" + j, typeof(SpriteRenderer), typeof(BoxCollider2D));
		newGO.transform.SetParent(CellsParents[x]);
		newGO.AddComponent<CellController>();
		newGO.GetComponent<CellController>().Setup(this, i, j);
		newGO.transform.position = new Vector3(j / size, -i / size, 0);
		SpriteRenderer sr = newGO.GetComponent<SpriteRenderer>();
		sr.sortingOrder = x;

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
}
