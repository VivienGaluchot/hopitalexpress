using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectController : MonoBehaviour {

    private LevelObjectsController loc;
    private GameObject myObject;

    public void SetInformations(LevelObjectsController parent, GameObject go) {
        loc = parent;
        myObject = go;
    }

    public void SendInformations() {
        loc.SetFollower(myObject);
    }
}
