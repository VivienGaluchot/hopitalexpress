using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerSelectionController : MonoBehaviour {

    public GameObject returnButton;

    public GameObject loadBar;

    public List<GameObject> persoList;


    private Dictionary<PlayerInput, PlayerSkinSelectorController> enabledPlayers;

    private bool isLocked = false;


    void Start() {
        enabledPlayers = new Dictionary<PlayerInput, PlayerSkinSelectorController>();
        returnButton.GetComponent<Button>().onClick.AddListener(onReturn);
    }

    void Update() {
        if (!loadBar.GetComponent<TimeBarController>().IsFull()) {
            foreach (PlayerInput input in PlayerInput.All) {
                if (input.GetAction2()) {
                    TryNewPlayer(input);
                }
            }

            bool allReady = enabledPlayers.Count > 0;
            foreach (var keyValue in enabledPlayers) {
                allReady = allReady && keyValue.Value.state == PlayerSkinSelectorController.State.Ready;
            }
            loadBar.GetComponent<TimeBarController>().isLoading = allReady;

        } else if (!isLocked) {
            isLocked = true;
            foreach (var perso in persoList) {
                perso.GetComponent<PlayerSkinSelectorController>().Lock();
            }
            returnButton.GetComponent<Button>().interactable = false;
            Scenes.LoadAsync(this, Scenes.GameScenes.LevelSelectionScene);
        }
    }

    private void onReturn() {
        returnButton.GetComponent<Button>().interactable = false;
        Scenes.LoadAsync(this, Scenes.GameScenes.HomeMenuScene);
    }

    public void DisablePlayer(PlayerInput playerInput) {
        if (enabledPlayers.ContainsKey(playerInput)) {
            enabledPlayers[playerInput].Disable();
            enabledPlayers.Remove(playerInput);
        }
    }

    private void TryNewPlayer(PlayerInput playerInput) {
        if (!enabledPlayers.ContainsKey(playerInput) && (enabledPlayers.Count < persoList.Count)) {
            foreach (GameObject perso in persoList) {
                PlayerSkinSelectorController ctr = perso.GetComponent<PlayerSkinSelectorController>();
                if (!enabledPlayers.ContainsValue(ctr)) {
                    enabledPlayers.Add(playerInput, ctr);
                    ctr.Enable(playerInput, this);
                    break;
                }
            }
        }
    }
}
