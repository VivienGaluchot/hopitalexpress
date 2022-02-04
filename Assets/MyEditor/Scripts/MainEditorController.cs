using UnityEngine;
using UnityEngine.UI;

public class MainEditorController : MonoBehaviour {

    public enum Editors {
        Treatment,
        Disease,
        Level
    }

    [SerializeField] private Dropdown dropdown;
    [SerializeField] private GameObject[] EditorsPanels;
    public Editors currentEditor;

    private void Start() {
        for (int i = 0; i < EditorsPanels.Length; i++)
            EditorsPanels[i].SetActive(false);

        dropdown.value = (int)currentEditor;
        EditorsPanels[(int)currentEditor].SetActive(true);
    }

    public void ChangeEditor(Dropdown dd) {
        if (dd.value < EditorsPanels.Length) {
            EditorsPanels[(int)currentEditor].SetActive(false);
            currentEditor = (Editors)dd.value;
            EditorsPanels[(int)currentEditor].SetActive(true);
        }
    }
}
