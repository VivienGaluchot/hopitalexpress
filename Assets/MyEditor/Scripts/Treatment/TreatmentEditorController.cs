using System.Collections.Generic;
using UnityEngine;

public class TreatmentEditorController : MonoBehaviour {
	public static TreatmentEditorController instance;

	public string treatmentPath;

	public List<TreatmentItemController> TreatmentItems { get; private set; }
	public List<TreatmentItemController> overTICs { get; set; }
	public GameObject TreatmentItemPrefab;

	// Line
	public GameObject myLine { get; set; }
	public GameObject LinePrefab;
	private LineRenderer myLineLR;
	private bool isDrawingLine;

	private TreatmentItemController lineStart;

	public GameObject clickedObject { get; set; }

	private GameObject Follower;
	private SpriteRenderer followerSR;
	private bool hasFollower;

	private string followerPath;

	private void Awake() { 
		instance = this;
		Follower = new GameObject("Follower", typeof(SpriteRenderer));
		Follower.transform.SetParent(transform);
		followerSR = Follower.GetComponent<SpriteRenderer>();
		overTICs = new List<TreatmentItemController>();
		TreatmentItems = new List<TreatmentItemController>();
	}

	private void Update() {
		if (Input.GetKeyDown("escape")) {
			StopDrawLine();
			UnsetFollower();
		}

		if (Input.GetKeyDown("delete") && clickedObject != null) {
			TreatmentObjectController item = clickedObject.GetComponent<TreatmentObjectController>();
			item.Delete();
			StopDrawLine();
		}

		bool GMBD0 = Input.GetMouseButtonDown(0);

		if (hasFollower || GMBD0 || isDrawingLine) {
			Vector3 worldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition) + new Vector3(0f, 0f, 10f);

			if (hasFollower)
				Follower.transform.position = worldPos + new Vector3(.5f, -.5f, 10f);

			if (GMBD0) {
				if(overTICs.Count == 0) {
					if (isDrawingLine) {
						StopDrawLine();
					} else if (!GlobalFunctions.DoesHitUI() && hasFollower) {
						GameObject newGo = Instantiate(TreatmentItemPrefab, worldPos, Quaternion.identity, transform);
						newGo.GetComponent<TreatmentItemController>().path = followerPath;
						newGo.transform.GetChild(0).GetComponent<SpriteRenderer>().sprite = followerSR.sprite;
						TreatmentItems.Add(newGo.GetComponent<TreatmentItemController>());
					}
				}
			} else if (isDrawingLine) {
				myLineLR.SetPosition(1, worldPos);
			}
		}
	}

	public void ClearScreen() {
		for (int i = 1; i < transform.childCount; i++)
			Destroy(transform.GetChild(i).gameObject);

		TreatmentItems.Clear();
		UnsetFollower();
	}

	// The follower is a gameobject which follow to mouse, used to display what we'll create if we click
	public void NewFollower(Sprite sprite, string path) {
		followerSR.sprite = sprite;
		hasFollower = true;
		followerPath = path;
	}

	private void UnsetFollower() {
		followerSR.sprite = null;
		hasFollower = false;
	}

	public void DrawLine(TreatmentItemController item) {
		UnsetFollower();
		if (lineStart == null) {
			// Start drawing a line
			lineStart = item;
			isDrawingLine = true;
			myLine = Instantiate(LinePrefab, transform);
			myLineLR = myLine.GetComponent<LineRenderer>();
			myLineLR.SetPosition(0, lineStart.transform.position);
		} else if (lineStart != item && !(item.endingLines.Count > 0) && lineStart.TryAddNext(myLine.GetComponent<LineController>(), item)) {
			// Clicked another object, try to assign it to the end of the road (rope? line!)
			myLineLR.SetPosition(1, item.transform.position);
			myLine.GetComponent<LineController>().UpdateMesh();

			lineStart = null;
			myLine = null;
			DrawLine(item);
		} else {
			StopDrawLine();
		}
	}

	public void StopDrawLine() {
		isDrawingLine = false;
		lineStart = null;
		Destroy(myLine);
		myLine = null;
	}

	void OnEnable() {
		Camera.main.transform.position = new Vector3(0f, 0f, -10f);
	}
}
