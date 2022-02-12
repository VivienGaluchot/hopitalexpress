using System.Collections.Generic;
using UnityEngine;

public class TreatmentEditorController : MonoBehaviour {
	public static TreatmentEditorController instance;

	public string[] treatmentPaths;

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

		Follower = new GameObject("Follower");
		Follower.transform.SetParent(transform);
		GameObject FollowerSprite = new GameObject("Sprite", typeof(SpriteRenderer));
		followerSR = FollowerSprite.GetComponent<SpriteRenderer>();
		followerSR.drawMode = SpriteDrawMode.Sliced;
		FollowerSprite.transform.SetParent(Follower.transform);

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

		if (hasFollower || Input.GetMouseButtonDown(0) || isDrawingLine) {
			Vector3 worldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition) + new Vector3(0f, 0f, 10f);

			if (hasFollower)
				Follower.transform.position = worldPos + new Vector3(0f, 0f, 10f);

			if (Input.GetMouseButtonDown(0)) {
				if(overTICs.Count == 0) {
					if (isDrawingLine) {
						StopDrawLine();
					} else if (!GlobalFunctions.DoesHitUI() && hasFollower) {
						GameObject newGo = Instantiate(TreatmentItemPrefab, worldPos, Quaternion.identity, transform);
						newGo.GetComponent<TreatmentItemController>().path = followerPath;
						TreatmentItems.Add(newGo.GetComponent<TreatmentItemController>());

						Transform SpriteContainer = newGo.transform.Find("Sprite");

						SpriteRenderer sr = SpriteContainer.GetComponent<SpriteRenderer>();
						sr.sprite = followerSR.sprite;
                        sr.drawMode = SpriteDrawMode.Sliced;
                        if (sr.size.x > sr.size.y) sr.size = new Vector2(.6f, .6f * sr.size.y / sr.size.x);
                        else sr.size = new Vector2(.6f * sr.size.x / sr.size.y, .6f);

                        // Adapt position depending on pivot
                        Vector2 pivot = new Vector2(sr.sprite.pivot.x / sr.sprite.rect.width, sr.sprite.pivot.y / sr.sprite.rect.height);
                        Vector2 offset = pivot - new Vector2(.5f, .5f);
						SpriteContainer.transform.localPosition += (Vector3)offset;

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
	public void NewFollower(GameObject follower, string path) {
		Sprite newSprite = follower.transform.Find("Sprite").GetComponent<SpriteRenderer>().sprite;
		if (followerSR.sprite != newSprite) {
			followerSR.sprite = newSprite;
			hasFollower = true;
			followerPath = path;

            Vector2 pivot = new Vector2(followerSR.sprite.pivot.x / followerSR.sprite.rect.width, followerSR.sprite.pivot.y / followerSR.sprite.rect.height);
            Vector2 offset = pivot - new Vector2(.5f, .5f);
            followerSR.transform.localPosition = (Vector3)offset;

            // Resize sprite to what we want
            if (followerSR.sprite.rect.width > followerSR.sprite.rect.height) followerSR.size = new Vector2(1f, followerSR.sprite.rect.height / followerSR.sprite.rect.width);
			else followerSR.size = new Vector2(followerSR.sprite.rect.width / followerSR.sprite.rect.height, 1f);

		} else {
			UnsetFollower();
		}
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
