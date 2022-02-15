using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerSkinSelectorController : MonoBehaviour {

    public List<GameObject> buttons;

    public GameObject textToBlick;

    public GameObject playerObject;

    public enum State {
        Off,
        NotReady,
        Ready,
    };

    public State state { get; private set; } = State.Off;

    public enum PlayerInput {
        Keyboard,
        Joystick1,
        Joystick2,
    };

    private PlayerInput playerInput;


    private float blinkPeriod = 0;
    

    void Start() {
        textToBlick.SetActive(false);
        SetState(State.Off);
    }

    void Update() {
        blinkPeriod += Time.deltaTime;
        if (state != State.Ready) {
            if (blinkPeriod > 1.5) {
                textToBlick.SetActive(false);
                blinkPeriod = 0;
            } else if (blinkPeriod > .5) {
                textToBlick.SetActive(true);
            }
        } else {
            textToBlick.SetActive(true);
        }
    }

    public void Enable(PlayerInput input) {
        if (state == State.Off) {
            SetReady(false);
            playerInput = input;
        }
    }

    public void SetReady(bool isReady) {
        if (isReady) {
            SetState(State.Ready);
        } else {
            SetState(State.NotReady);
        }
    }

    private void SetState(State newState) {
        playerObject.SetActive(newState != State.Off);
        foreach (GameObject button in buttons) {
            button.SetActive(newState == State.NotReady);
        }
        state = newState;
    }

}
