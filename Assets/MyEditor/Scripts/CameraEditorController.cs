using UnityEngine;

public class CameraEditorController : MonoBehaviour {

    [SerializeField] private float scrollSpeed;

    private Vector3 clickedPos;

    private void Update() {
        if(Input.GetMouseButtonDown(2)) {
            clickedPos = Input.mousePosition;
        } else if(Input.GetMouseButton(2)) {
            //Vector3 dist = lastPos - Input.mousePosition;
            //transform.position += dist * dragSpeed;
            //lastPos = Input.mousePosition;
        }
    }

}
