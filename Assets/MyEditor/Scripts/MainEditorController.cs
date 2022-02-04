using UnityEngine;
using UnityEngine.UI;

public class MainEditorController : MonoBehaviour {

    [SerializeField] private GameObject[] Editors;
    private int currentEditor;

    private void Start() {
        for (int i = 0; i < Editors.Length; i++)
            Editors[i].SetActive(false);

        currentEditor = 0;
        Editors[0].SetActive(true);
    }

    public void ChangeEditor(Dropdown dd) {
        if (dd.value < Editors.Length) {
            Editors[currentEditor].SetActive(false);
            currentEditor = dd.value;
            Editors[currentEditor].SetActive(true);
        }
    }
}
