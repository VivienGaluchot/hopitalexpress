using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraEditorController : MonoBehaviour {

    [SerializeField] private float scrollSpeed, dragSpeed;

    private Vector3 lastPos;

    private void Update() {
        if(Input.GetMouseButtonDown(2)) {
            lastPos = Input.mousePosition;
        } else if(Input.GetMouseButton(2)) {
            Vector3 dist = lastPos - Input.mousePosition;
            transform.position += dist * dragSpeed;
            lastPos = Input.mousePosition;
        }
    }

}
