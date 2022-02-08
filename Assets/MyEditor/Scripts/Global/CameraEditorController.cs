using UnityEngine;

public class CameraEditorController : MonoBehaviour {
	public static CameraEditorController instance;
    private Camera cam;
	private Vector3 lastPos;

	private bool isDragging;

    private void Awake() {
		instance = this;
		cam = Camera.main;
    }

    private void Update() {
		if (Input.GetMouseButtonDown(2) && !GlobalFunctions.DoesHitUI()) {
			lastPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
			isDragging = true;
		} else if (Input.GetMouseButton(2) && isDragging) {
			transform.position += (lastPos - Camera.main.ScreenToWorldPoint(Input.mousePosition));
			lastPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
		} else if (Input.GetMouseButtonUp(2))
			isDragging = false;

		float scroll = Input.mouseScrollDelta.y;
		if(scroll != 0 && !GlobalFunctions.DoesHitUI()) {
			cam.orthographicSize = Mathf.Min(40, Mathf.Max(1f, cam.orthographicSize - scroll));
        }
	}
}
