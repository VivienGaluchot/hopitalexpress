using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class LevelEditorController : MonoBehaviour {


	// USELESS BUT FUN
	public bool randomFun;
	public float tickrate;
	private float elapsedTime;
	private int clicked, total;
	// END OF USELESS BUT FUN

	public float size;
	public int columns, rows;

	public Sprite[] wallSprites;
	private Sprite baseSprite;

	public Dictionary<(int, int), Cell> grid { get; private set; }

	[SerializeField] private GraphicRaycaster m_Raycaster;
	[SerializeField] private PointerEventData m_PointerEventData;
	[SerializeField] private EventSystem m_EventSystem;
	[SerializeField] private GameObject MenuPanel;

	private Transform CellsParent;

	public class Cell {
		public Cell(GameObject go) { this.go = go; sr = this.go.GetComponent<SpriteRenderer>(); value = 0; }
		public Cell(GameObject go, SpriteRenderer sr) { this.go = go; this.sr = sr; value = 0; }

		public GameObject go;
		public int value;
		public SpriteRenderer sr;
	}

	private void Start() {
		CellsParent = transform.Find("Cells");
		InitGrid();

		// USELESS BUT FUN
		elapsedTime = 0f;
		clicked = 0;
		total = columns * rows;
	}

    private void Update() {
		if (Input.GetKeyDown("tab"))
			MenuPanel.SetActive(!MenuPanel.activeSelf);

		// USELESS BUT FUN
		if (randomFun) {
			elapsedTime += Time.deltaTime;
			if(elapsedTime > tickrate) {
				elapsedTime -= tickrate;
				int i = Random.Range(0, rows), j = Random.Range(0, columns);
				bool reset = Random.Range(0, 1f) < (float)clicked / total;
				if (grid[(i, j)].value > 0 && reset) {
					clicked--;
					ClickedCell(i, j, true, true);
				} else if(grid[(i, j)].value == 0 && !reset) {
					clicked++;
					ClickedCell(i, j, true);
				}
            }
		}
    }

	public bool DoesHitUI() {
		m_PointerEventData = new PointerEventData(m_EventSystem);
		m_PointerEventData.position = Input.mousePosition;

		List<RaycastResult> results = new List<RaycastResult>();
		m_Raycaster.Raycast(m_PointerEventData, results);

		return results.Count != 0;
	}

	public void ClickedCell(int i, int j, bool clicked = false, bool reset = false) {
		Cell cell;
		if (grid.TryGetValue((i, j), out cell) && (cell.value != 0 || clicked) && !reset) {
			int newValue = ComputeCellValue(i, j);
			if (newValue != cell.value) {
				cell.value = newValue;
				ChangeSprite(cell);
				Propagate(i, j);
			}
		} else if(reset && grid.TryGetValue((i, j), out cell) && cell.value != 0) {
			cell.value = 0;
			ChangeSprite(cell);
			Propagate(i, j);
		}	
	}

	private void Propagate(int i, int j) {
		ClickedCell(i + 1, j);
		ClickedCell(i - 1, j);
		ClickedCell(i, j + 1);
		ClickedCell(i, j - 1);
	}

	private void ChangeSprite(Cell cell) {
		switch(cell.value) {
			case 0:
				cell.sr.sprite = baseSprite;
				break;
			case 1:
				cell.sr.sprite = wallSprites[0];
				break;
			case 2:
				cell.sr.sprite = wallSprites[1];
				cell.sr.transform.rotation = Quaternion.identity;
				break;
			case 3:
				cell.sr.sprite = wallSprites[1];
				cell.sr.transform.rotation = Quaternion.Euler(0f, 0f, 270f);
				break;
			case 4:
				cell.sr.sprite = wallSprites[1];
				cell.sr.transform.rotation = Quaternion.Euler(0f, 0f, 180f);
				break;
			case 5:
				cell.sr.sprite = wallSprites[1];
				cell.sr.transform.rotation = Quaternion.Euler(0f, 0f, 90f);
				break;
			case 6:
				cell.sr.sprite = wallSprites[2];
				cell.sr.transform.rotation = Quaternion.identity;
				break;
			case 7:
				cell.sr.sprite = wallSprites[2];
				cell.sr.transform.rotation = Quaternion.Euler(0f, 0f, 90f);
				break;
			case 8:
				cell.sr.sprite = wallSprites[3];
				cell.sr.transform.rotation = Quaternion.identity;
				break;
			case 9:
				cell.sr.sprite = wallSprites[3];
				cell.sr.transform.rotation = Quaternion.Euler(0f, 0f, 270f);
				break;
			case 10:
				cell.sr.sprite = wallSprites[3];
				cell.sr.transform.rotation = Quaternion.Euler(0f, 0f, 180f);
				break;
			case 11:
				cell.sr.sprite = wallSprites[3];
				cell.sr.transform.rotation = Quaternion.Euler(0f, 0f, 90f);
				break;
			case 12:
				cell.sr.sprite = wallSprites[4];
				cell.sr.transform.rotation = Quaternion.identity;
				break;
			case 13:
				cell.sr.sprite = wallSprites[4];
				cell.sr.transform.rotation = Quaternion.Euler(0f, 0f, 270f);
				break;
			case 14:
				cell.sr.sprite = wallSprites[4];
				cell.sr.transform.rotation = Quaternion.Euler(0f, 0f, 180f);
				break;
			case 15:
				cell.sr.sprite = wallSprites[4];
				cell.sr.transform.rotation = Quaternion.Euler(0f, 0f, 90f);
				break;
			case 16:
				cell.sr.sprite = wallSprites[5];
				break;
		}
    }

	// 0    empty               // 8    L
	// 1    alone               // 9    L 90°
	// 2    one                 // 10   L 180°
	// 3    one 90°             // 11   L 270°
	// 4    one 180°            // 12   T
	// 5    one 270°			// 13   T 90°
	// 6    I vertical			// 14   T 180°
	// 7    I horizontal		// 15   T 270°
	// 16   X

	private int ComputeCellValue(int i, int j) {
		// ça peut être sympa de faire un joli algo ici
		Cell cell;
		int neighbours = CountNeighbour(i, j);
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
			case 4:
				return 16;
		}
		return 0;
	}

	private int CountNeighbour(int i, int j) {
		Cell cell;
		int sum = 0;
		if (grid.TryGetValue((i, j + 1), out cell) && cell.value != 0) sum++;
		if (grid.TryGetValue((i, j - 1), out cell) && cell.value != 0) sum++;
		if (grid.TryGetValue((i + 1, j), out cell) && cell.value != 0) sum++;
		if (grid.TryGetValue((i - 1, j), out cell) && cell.value != 0) sum++;
		return sum;
	}

	public void ClearGrid() {
		foreach (KeyValuePair<(int, int), Cell> cell in grid) {
			cell.Value.value = 0;
			ChangeSprite(cell.Value);
			cell.Value.sr.gameObject.GetComponent<CellController>().AdjustClickedBool(cell.Value.value);
		}
	}

	public void RefreshGrid() {
		foreach (KeyValuePair<(int, int), Cell> cell in grid) {
			ChangeSprite(cell.Value);
			cell.Value.sr.gameObject.GetComponent<CellController>().AdjustClickedBool(cell.Value.value);
		}
	}

	public void ResizeGrid(int rows, int columns) {
		for (int i = 0; i < Mathf.Max(rows, this.rows); i++)
			for (int j = 0; j < Mathf.Max(columns, this.columns); j++)
				if (i >= rows || j >= columns)
					DeleteCell(i, j);

		for (int i = 0; i < Mathf.Max(rows, this.rows); i++)
			for (int j = 0; j < Mathf.Max(columns, this.columns); j++)
				if (i >= this.rows || j >= this.columns)
					InitCell(i, j);

		this.rows = rows;
		this.columns = columns;
		Camera.main.transform.position = new Vector3((columns - 1) / 2f / size, (1 - rows) / 2f / size, -10f);
	}

	private void InitGrid() {
		baseSprite = Sprite.Create(Texture2D.whiteTexture, new Rect(0, 0, 1, 1), new Vector2(.5f, .5f), size);
		grid = new Dictionary<(int, int), Cell>();
		for(int i = 0; i < rows; i++)
			for(int j = 0; j < columns; j++)
				InitCell(i, j);

		Camera.main.transform.position = new Vector3((columns-1)/2f/size, (1-rows)/2f/size, -10f);
	}

	private void InitCell(int i, int j) {
		GameObject newGO = new GameObject(i + "-" + j, typeof(SpriteRenderer), typeof(BoxCollider2D));
		newGO.transform.SetParent(CellsParent);
		newGO.AddComponent<CellController>();
		newGO.GetComponent<CellController>().Setup(this, i, j);
		newGO.transform.position = new Vector3(j / size, -i / size, 0);
		SpriteRenderer sr = newGO.GetComponent<SpriteRenderer>();
		sr.sprite = baseSprite;
		sr.sortingOrder = -1;
		sr.color = (i + j) % 2 == 0 ? Color.grey : Color.white;
		newGO.GetComponent<BoxCollider2D>().size = new Vector2(1f / size, 1f / size);

		grid.Add((i, j), new Cell(newGO, sr));
	}

	private void DeleteCell(int i, int j) {
		Cell cell;
		if(grid.TryGetValue((i, j), out cell)) {
			Destroy(cell.go);
			grid.Remove((i, j));
        }
	}
}
