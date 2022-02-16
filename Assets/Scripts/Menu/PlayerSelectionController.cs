using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PlayerSelectionController : MonoBehaviour {

    public GameObject returnButton;

    public List<GameObject> persoList;

    private Dictionary<PlayerInput, PlayerSkinSelectorController> enabledPlayers;


    void Start() {
        enabledPlayers = new Dictionary<PlayerInput, PlayerSkinSelectorController>();
        returnButton.GetComponent<Button>().onClick.AddListener(onReturn);
    }

    void Update() {
        foreach (PlayerInput input in PlayerInput.All) {
            if (input.GetAction0()) {
                TryNewPlayer(input);
            }
        }
    }

    private void onReturn() {
        StartCoroutine(LoadAsync("HomeMenuScene"));
    }

    private IEnumerator LoadAsync(string sceneName) {
        returnButton.GetComponent<Button>().interactable = false;
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        while (!asyncLoad.isDone) {
            yield return null;
        }
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
