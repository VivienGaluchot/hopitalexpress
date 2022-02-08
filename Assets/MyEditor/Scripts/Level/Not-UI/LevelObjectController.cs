using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelObjectController {

    public LevelObjectController(string path, bool isSeat) {
        this.path = path; 
        this.isSeat = isSeat;
    }

    public string path { get; private set; }
    public bool isSeat { get; private set; }


}
