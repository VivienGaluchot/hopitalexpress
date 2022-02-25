using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HomeMenuController : MonoBehaviour {

    public GameObject continueButton;

    public GameObject newGameButton;

    public GameObject editorButton;

    public GameObject optionButton;

    public GameObject quitButton;


    void Start() {
        QualitySettings.vSyncCount = 0;
        continueButton.GetComponent<Button>().onClick.AddListener(onContinue);
        newGameButton.GetComponent<Button>().onClick.AddListener(onNewGame);
        editorButton.GetComponent<Button>().onClick.AddListener(onEditor);
        optionButton.GetComponent<Button>().onClick.AddListener(onOption);
        quitButton.GetComponent<Button>().onClick.AddListener(onQuit);
    }

    private void onContinue() {

    }

    private void onNewGame() {
        continueButton.GetComponent<Button>().interactable = false;
        newGameButton.GetComponent<Button>().interactable = false;
        editorButton.GetComponent<Button>().interactable = false;
        optionButton.GetComponent<Button>().interactable = false;
        quitButton.GetComponent<Button>().interactable = false;
        Scenes.LoadAsync(this, Scenes.GameScenes.PlayerSelectionScene);
    }

    private void onEditor() {
        continueButton.GetComponent<Button>().interactable = false;
        newGameButton.GetComponent<Button>().interactable = false;
        editorButton.GetComponent<Button>().interactable = false;
        optionButton.GetComponent<Button>().interactable = false;
        quitButton.GetComponent<Button>().interactable = false;
        Scenes.LoadAsync(this, Scenes.GameScenes.EditorScene);
    }

    private void onOption() {

    }

    private void onQuit() {
        Application.Quit();
    }
}
