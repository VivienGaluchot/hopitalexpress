using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelObjectsController : MonoBehaviour {
	public static LevelObjectsController instance;

	private Transform ObjectsParent, FullWallsParent;
	private GameObject Follower;
	private bool hasFollower;

	private Dictionary<GameObject, Vector3> objects;

	private void Start() {
		instance = this;

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
				hasFollower = false;
				Destroy(Follower);
				Follower = null;
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
		Destroy(Follower);
		Follower = null;
		hasFollower = false;
		ObjectsParent.gameObject.SetActive(false);
		FullWallsParent.gameObject.SetActive(false);
	}

	public void SetFollower(GameObject go) {
		if(hasFollower)
			Destroy(Follower);
		else
			hasFollower = true;

		Follower = Instantiate(go);
	}
}
