using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectController : MonoBehaviour {

    private GameObject myObject;

    public void SetInformations(GameObject go) {
        myObject = go;
    }

    public void SendInformations() {
        LevelObjectsController.instance.SetFollower(myObject);
    }
}
