using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelObjectsController : MonoBehaviour {
	public static LevelObjectsController instance;

	public string[] objectsPath;

	private Transform ObjectsParent, FullWallsParent;
	private GameObject Follower;
	private SpriteRenderer followerSR;

	private bool hasFollower;

	private Dictionary<GameObject, Vector3> objects;

	private void Start() {
		instance = this;

		Follower = new GameObject("Follower", typeof(SpriteRenderer));
		followerSR = Follower.GetComponent<SpriteRenderer>();

		LevelEditorController.instance = GetComponent<LevelEditorController>();
		ObjectsParent = transform.Find("ObjectsLayer");
		FullWallsParent = transform.Find("FullWallsLayer");

		ObjectsParent.gameObject.SetActive(false);
		FullWallsParent.gameObject.SetActive(false);

		objects = new Dictionary<GameObject, Vector3>();
	}

	private void Update() {
		if (hasFollower) {
			Vector3 worldMousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition) + new Vector3(0f, 0f, 10f);
			Follower.transform.position = worldMousePos;
			if (Input.GetKeyDown("escape")) {
				UnsetFollower();
			} else if (Input.GetMouseButtonDown(0) && !LevelEditorController.instance.DoesHitUI()) {
				objects.Add(Instantiate(Follower, ObjectsParent), worldMousePos);
			}
		}
	}

	public void StartDisplay() {
		ObjectsParent.gameObject.SetActive(true);
		FullWallsParent.gameObject.SetActive(true);
	}

	public void StopDisplay() {
		UnsetFollower();
		ObjectsParent.gameObject.SetActive(false);
		FullWallsParent.gameObject.SetActive(false);
	}

	public void SetFollower(Sprite followerSprite) {
		hasFollower = true;
		followerSR.sprite = followerSprite;
	}

	public void UnsetFollower() {
		hasFollower = false;
		followerSR.sprite = null;
    }
}
