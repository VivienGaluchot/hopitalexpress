using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PlayerSelectionController : MonoBehaviour {

    public GameObject returnButton;

    public List<GameObject> persoList;

    private Dictionary<PlayerSkinSelectorController.PlayerInput, PlayerSkinSelectorController> enabledPlayers;


    void Start() {
        enabledPlayers = new Dictionary<PlayerSkinSelectorController.PlayerInput, PlayerSkinSelectorController>();
        returnButton.GetComponent<Button>().onClick.AddListener(onReturn);
    }

    void Update() {
        if (Input.GetKeyDown(PlayerSkinSelectorController.PlayerInputAction0[PlayerSkinSelectorController.PlayerInput.Keyboard])) {
            NewPlayer(PlayerSkinSelectorController.PlayerInput.Keyboard);
        }
        if (Input.GetKeyDown(PlayerSkinSelectorController.PlayerInputAction0[PlayerSkinSelectorController.PlayerInput.Joystick1])) {
            NewPlayer(PlayerSkinSelectorController.PlayerInput.Joystick1);
        }
        if (Input.GetKeyDown(PlayerSkinSelectorController.PlayerInputAction0[PlayerSkinSelectorController.PlayerInput.Joystick2])) {
            NewPlayer(PlayerSkinSelectorController.PlayerInput.Joystick2);
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


    public void DisablePlayer(PlayerSkinSelectorController.PlayerInput playerInput) {
        if (enabledPlayers.ContainsKey(playerInput)) {
            enabledPlayers[playerInput].Disable();
            enabledPlayers.Remove(playerInput);
        }
    }

    private void NewPlayer(PlayerSkinSelectorController.PlayerInput playerInput) {
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
