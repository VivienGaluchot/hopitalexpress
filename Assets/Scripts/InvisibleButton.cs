using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class InvisibleButton : MonoBehaviour, IDeselectHandler {

    void Start() {
        GetComponent<Button>().Select();
    }

    public void OnDeselect(BaseEventData data) {
        gameObject.SetActive(false);
    }
}
