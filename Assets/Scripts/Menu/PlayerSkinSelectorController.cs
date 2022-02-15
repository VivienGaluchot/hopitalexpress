using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerSkinSelectorController : MonoBehaviour {

    public List<GameObject> buttons;

    public GameObject textToBlick;

    public GameObject playerObject;

    public GameObject actionText;


    public enum State {
        Off,
        NotReady,
        Ready,
    };

    static readonly Dictionary<State, string> StateActionText = new Dictionary<State, string>() {
        {State.Off, "to join"},
        {State.NotReady, "when ready"}
    };

    public State state { get; private set; } = State.Off;

    public enum PlayerInput {
        Keyboard,
        Joystick1,
        Joystick2,
    };

    public static readonly Dictionary<PlayerInput, KeyCode> PlayerInputAction0 = new Dictionary<PlayerInput, KeyCode>() {
        {PlayerInput.Keyboard, KeyCode.Return},
        {PlayerInput.Joystick1, KeyCode.Joystick1Button0},
        {PlayerInput.Joystick2, KeyCode.Joystick2Button0}
    };

    public static readonly Dictionary<PlayerInput, KeyCode> PlayerInputAction1 = new Dictionary<PlayerInput, KeyCode>() {
        {PlayerInput.Keyboard, KeyCode.Backspace},
        {PlayerInput.Joystick1, KeyCode.Joystick1Button1},
        {PlayerInput.Joystick2, KeyCode.Joystick2Button1}
    };

    private PlayerInput playerInput;

    private PlayerSelectionController selectionController;


    private float blinkPeriod = 0;
    

    void Start() {
        textToBlick.SetActive(false);
        SetState(State.Off);
    }

    void Update() {
        // change state on action
        if (Input.GetKeyDown(PlayerInputAction0[playerInput])) {
            if (state == State.NotReady) {
                SetState(State.Ready);
            }
        }
        if (Input.GetKeyDown(PlayerInputAction1[playerInput])) {
            if (state == State.NotReady) {
                selectionController.DisablePlayer(playerInput);
            } else if (state == State.Ready) {
                SetState(State.NotReady);
            }
        }

        // blink
        blinkPeriod += Time.deltaTime;
        if (blinkPeriod < 1) {
            if (state != State.Ready) {
                textToBlick.SetActive(true);
            }
        } else if (blinkPeriod < 1.5) {
            if (state != State.Ready) {
                textToBlick.SetActive(false);
            }
        } else {
            blinkPeriod = 0;
        }
    }

    public void Enable(PlayerInput input, PlayerSelectionController controller) {
        if (state == State.Off) {
            SetState(State.NotReady);
            playerInput = input;
            selectionController = controller;
        }
    }

    public void Disable() {
        SetState(State.Off);
    }

    private void SetState(State newState) {
        playerObject.SetActive(newState != State.Off);
        foreach (GameObject button in buttons) {
            button.SetActive(newState == State.NotReady);
        }
        textToBlick.SetActive(newState != State.Ready);
        actionText.SetActive(newState != State.Ready);
        if (StateActionText.ContainsKey(newState)) {
            actionText.GetComponent<Text>().text = StateActionText[newState];
        }
        state = newState;
    }

}
