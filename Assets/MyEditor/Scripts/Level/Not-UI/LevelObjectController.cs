using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelObjectController : MonoBehaviour {
	public void SetParams(string path, string prefabTag, bool isWelcomeSeat = false, float containerTime = 0f) {
		this.path = path;
		this.prefabTag = prefabTag;
		this.isWelcomeSeat = isWelcomeSeat;
		this.containerTime = containerTime;
	}

	public string path { get; private set; }
	public string prefabTag { get; private set; }
	public bool isWelcomeSeat;
	public float containerTime;
	public bool isChild;
	public List<LevelObjectController> childs { get; private set; } = new List<LevelObjectController>();

    public void RemoveChild(GameObject clickedGO) {
		if (childs.Contains(clickedGO.GetComponent<LevelObjectController>()))
			childs.Remove(clickedGO.GetComponent<LevelObjectController>());
    }
}
