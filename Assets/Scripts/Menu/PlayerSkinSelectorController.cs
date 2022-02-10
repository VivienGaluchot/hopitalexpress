using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerSkinSelectorController : MonoBehaviour {

    public List<GameObject> buttons;

    public GameObject textToBlick;

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
    }

    void Update() {
        blinkPeriod += Time.deltaTime;
        if (state == State.Off) {
            if (blinkPeriod > 2) {
                textToBlick.SetActive(false);
                blinkPeriod = 0;
            } else if (blinkPeriod > .5) {
                textToBlick.SetActive(true);
            }
        } else {
            textToBlick.SetActive(false);
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
            foreach (GameObject button in buttons) {
                button.SetActive(false);
            }
            state = State.Ready;
        } else {
            foreach (GameObject button in buttons) {
                button.SetActive(true);
            }
            state = State.NotReady;
        }
    }

}
