using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelObjectController : MonoBehaviour {
	public void SetParams(string path, bool isSeat, bool isWelcomeSeat = false) {
		this.path = path;
		this.isSeat = isSeat;
		this.isWelcomeSeat = isWelcomeSeat;
	}

	public string path { get; private set; }
	public bool isSeat { get; private set; }
	public bool isWelcomeSeat;
	public List<LevelObjectController> childs { get; private set; } = new List<LevelObjectController>();
}
