using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(StandaloneInputModule))]

public class ReselectOnInput : MonoBehaviour {

	private StandaloneInputModule standaloneInputModule;
	private GameObject lastSelectedObject;
	public static ReselectOnInput instance;

	void Awake() {
		instance = this;
		standaloneInputModule = GetComponent<StandaloneInputModule>();
	}

	void Update() {
		if (EventSystem.current.currentSelectedGameObject != null) {
			lastSelectedObject = EventSystem.current.currentSelectedGameObject.gameObject;
		} else if ((Input.GetAxisRaw(standaloneInputModule.horizontalAxis) != 0) ||
				 (Input.GetAxisRaw(standaloneInputModule.verticalAxis) != 0) ||
				 (Input.GetButtonDown(standaloneInputModule.submitButton)) ||
				 (Input.GetButtonDown(standaloneInputModule.cancelButton))) {
				EventSystem.current.SetSelectedGameObject(null);
				EventSystem.current.SetSelectedGameObject(instance.lastSelectedObject);
		}
	}
}

