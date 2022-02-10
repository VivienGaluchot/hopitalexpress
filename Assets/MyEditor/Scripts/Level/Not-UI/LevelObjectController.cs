using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelObjectController {

	public LevelObjectController(string path, bool isSeat, bool isWelcomeSeat) {
		this.path = path; 
		this.isSeat = isSeat;
		this.isWelcomeSeat = isWelcomeSeat;
	}

	public LevelObjectController(LevelObjectController copy) {
		this.path = copy.path;
		this.isSeat = copy.isSeat;
		this.isWelcomeSeat = copy.isWelcomeSeat;
	}

	public string path { get; private set; }
	public bool isSeat { get; private set; }
	public bool isWelcomeSeat;

}
