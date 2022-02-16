using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerSkinSelectorController : MonoBehaviour {

    public enum ActionWhen {
        Hide,
        Blink,
        Show
    };

    [System.Serializable]
    public struct ShowWhen {
        public GameObject target;
        public ActionWhen onOff;
        public ActionWhen onNotReady;
        public ActionWhen onReady;
    };

    public List<ShowWhen> showWhens;

    public GameObject playerObject;

    public GameObject headPrev;
    public GameObject headNext;
    public GameObject skinPrev;
    public GameObject skinNext;
    public GameObject clothesPrev;
    public GameObject clothesNext;

    private List<(Button prev, Button next)> rows;

    private int rowSelect = 0;

    private float lastSelect = 0;


    public enum State {
        Off,
        NotReady,
        Ready,
    };

    public State state { get; private set; } = State.Off;


    private PlayerInput playerInput;

    private PlayerSelectionController selectionController;


    private float blinkPeriod = 0;
    

    void Start() {
        state = State.Off;
        rows = new List<(Button prev, Button next)>() {
            {(prev:headPrev.GetComponent<Button>(), next:headNext.GetComponent<Button>())},
            {(prev:skinPrev.GetComponent<Button>(), next:skinNext.GetComponent<Button>())},
            {(prev:clothesPrev.GetComponent<Button>(), next:clothesNext.GetComponent<Button>())}
        };
    }

    void Update() {
        // change state on action
        lastSelect += Time.deltaTime;
        if (playerInput != null) {
            if (playerInput.GetAction0()) {
                if (state == State.NotReady) {
                    state = State.Ready;
                }
            }
            if (playerInput.GetAction1()) {
                if (state == State.NotReady) {
                    selectionController.DisablePlayer(playerInput);
                } else if (state == State.Ready) {
                    state = State.NotReady;
                }
            }

            if (state == State.NotReady) {
                const float maxSelectFreq = .2f;
                if (playerInput.GetY() > .5 && lastSelect > maxSelectFreq) {
                    rowSelect -= 1;
                    lastSelect = 0;
                }
                if (playerInput.GetY() < -.5 && lastSelect > maxSelectFreq) {
                    rowSelect += 1;
                    lastSelect = 0;
                }
                if (rowSelect < 0) {
                    rowSelect = rows.Count - 1;
                }
                if (rowSelect >= rows.Count) {
                    rowSelect = 0;
                }
                if (playerInput.GetX() > .5 && lastSelect > maxSelectFreq) {
                    rows[rowSelect].next.GetComponent<Button>().onClick.Invoke();
                    lastSelect = 0;
                }
                if (playerInput.GetX() < -.5 && lastSelect > maxSelectFreq) {
                    rows[rowSelect].prev.GetComponent<Button>().onClick.Invoke();
                    lastSelect = 0;
                }
            }
        }

        // buttons selection for joystick
        // TODO find out why this is not really working
        if (playerInput != PlayerInput.Keyboard) {
            for (int i = 0; i < rows.Count; i++) {
                ColorBlock prevC = rows[i].prev.colors;
                ColorBlock nextC = rows[i].next.colors;
                if (i == rowSelect) {
                    prevC.normalColor = Color.gray;
                    nextC.normalColor = Color.gray;
                } else {
                    prevC.normalColor = Color.white;
                    nextC.normalColor = Color.white;
                }
                rows[i].prev.colors = prevC;
                rows[i].next.colors = nextC;
            }
        }

        // blink
        blinkPeriod += Time.deltaTime;
        bool isBlinkOn = true;
        if (blinkPeriod < 1.5) {
            if (state != State.Ready) {
                isBlinkOn = true;
            }
        } else if (blinkPeriod < 2) {
            if (state != State.Ready) {
                isBlinkOn = false;
            }
        } else {
            blinkPeriod = 0;
        }
        
        // update show whens
        foreach (ShowWhen sh in showWhens) {
            if (state == State.Off) {
                sh.target.SetActive(sh.onOff == ActionWhen.Show || (sh.onOff == ActionWhen.Blink && isBlinkOn));
            }
            if (state == State.NotReady) {
                sh.target.SetActive(sh.onNotReady == ActionWhen.Show || (sh.onNotReady == ActionWhen.Blink && isBlinkOn));
            }
            if (state == State.Ready) {
                sh.target.SetActive(sh.onReady == ActionWhen.Show || (sh.onReady == ActionWhen.Blink && isBlinkOn));
            }
        }
    }

    public void Enable(PlayerInput input, PlayerSelectionController controller) {
        if (state == State.Off) {
            state = State.NotReady;
            playerInput = input;
            selectionController = controller;
            
            // only enable button for keyboard
            // when enabled, the button can be clicked by the mouse
            for (int i = 0; i < rows.Count; i++) {
                rows[i].prev.enabled = playerInput == PlayerInput.Keyboard;
                rows[i].next.enabled = playerInput == PlayerInput.Keyboard;
            }
        }
    }

    public void Disable() {
        state = State.Off;
    }

}
