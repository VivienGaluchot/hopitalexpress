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
        public ActionWhen onLock;
    };

    public List<ShowWhen> showWhens;

    [System.Serializable]
    public struct InputIcon {
        public GameObject target;
        public PlayerInput.Kind showFor;
    };

    public List<InputIcon> inputIcons;


    public Color joysticUnselectedColor = new Color(1, 1, 1, .75f);

    public GameObject hairPrev;
    public GameObject hairNext;
    public GameObject skinPrev;
    public GameObject skinNext;
    public GameObject clothesPrev;
    public GameObject clothesNext;

    public GameObject playerObject;


    private List<(Button prevBtn, Button nextBtn, Image prevImg, Image nextImg)> rows;

    private int rowSelect = 0;

    private float lastSelect = 0;


    public enum State {
        Off,
        NotReady,
        Ready,
        Locked,
    };

    public State state { get; private set; } = State.Off;


    private PlayerInput playerInput;

    private PlayerSelectionController selectionController;


    private float blinkPeriod = 0;
    

    void Start() {
        state = State.Off;
        rows = new List<(Button prevBtn, Button nextBtn, Image prevImg, Image nextImg)>() {
            {(prevBtn:hairPrev.GetComponent<Button>(), nextBtn:hairNext.GetComponent<Button>(),
              prevImg:hairPrev.GetComponent<Image>(), nextImg:hairNext.GetComponent<Image>())},
            {(prevBtn:skinPrev.GetComponent<Button>(), nextBtn:skinNext.GetComponent<Button>(),
              prevImg:skinPrev.GetComponent<Image>(), nextImg:skinNext.GetComponent<Image>())},
            {(prevBtn:clothesPrev.GetComponent<Button>(), nextBtn:clothesNext.GetComponent<Button>(),
              prevImg:clothesPrev.GetComponent<Image>(), nextImg:clothesNext.GetComponent<Image>())}
        };

        var ctr = playerObject.GetComponent<PlayerWalkController>();
        hairNext.GetComponent<Button>().onClick.AddListener(() => { ctr.hair.GetComponent<SkinManager>().NextSkinIndex(); });
        hairPrev.GetComponent<Button>().onClick.AddListener(() => { ctr.hair.GetComponent<SkinManager>().PreviousSkinIndex(); });
        skinNext.GetComponent<Button>().onClick.AddListener(() => { ctr.skin.GetComponent<SkinManager>().NextSkinIndex(); });
        skinPrev.GetComponent<Button>().onClick.AddListener(() => { ctr.skin.GetComponent<SkinManager>().PreviousSkinIndex(); });
        clothesNext.GetComponent<Button>().onClick.AddListener(() => { ctr.clothes.GetComponent<SkinManager>().NextSkinIndex(); });
        clothesPrev.GetComponent<Button>().onClick.AddListener(() => { ctr.clothes.GetComponent<SkinManager>().PreviousSkinIndex(); });
    }

    void Update() {
        // change state on action
        lastSelect += Time.deltaTime;
        if (playerInput != null) {
            if (playerInput.GetAction2()) {
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
                    rows[rowSelect].nextBtn.onClick.Invoke();
                    lastSelect = 0;
                }
                if (playerInput.GetX() < -.5 && lastSelect > maxSelectFreq) {
                    rows[rowSelect].prevBtn.onClick.Invoke();
                    lastSelect = 0;
                }
            }
        }

        // buttons selection for joystick
        if (playerInput != null && playerInput.kind != PlayerInput.Kind.Keyboard) {
            for (int i = 0; i < rows.Count; i++) {
                if (i == rowSelect) {
                    rows[i].prevImg.color = Color.white;
                    rows[i].nextImg.color = Color.white;
                } else {
                    rows[i].prevImg.color = joysticUnselectedColor;
                    rows[i].nextImg.color = joysticUnselectedColor;
                }
            }
        }

        // blink
        blinkPeriod += Time.deltaTime;
        bool isBlinkOn = true;
        if (blinkPeriod < 1.5) {
            isBlinkOn = true;
        } else if (blinkPeriod < 2) {
            isBlinkOn = false;
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
            if (state == State.Locked) {
                // when locked only show when the player input is set to hide unselected players
                sh.target.SetActive(playerInput != null &&
                    (sh.onLock == ActionWhen.Show || (sh.onLock == ActionWhen.Blink && isBlinkOn)));
            }
        }
    }

    public void Enable(PlayerInput input, PlayerSelectionController controller) {
        if (state == State.Off) {
            state = State.NotReady;
            playerInput = input;
            selectionController = controller;

            foreach (InputIcon inputIcon in inputIcons) {
                inputIcon.target.SetActive(inputIcon.showFor == playerInput.kind);
            }
            
            // only enable button interaction for keyboard user
            for (int i = 0; i < rows.Count; i++) {
                rows[i].prevBtn.enabled = playerInput.kind == PlayerInput.Kind.Keyboard;
                rows[i].nextBtn.enabled = playerInput.kind == PlayerInput.Kind.Keyboard;
            }
        }
    }

    public void Disable() {
        state = State.Off;
        playerInput = null;
    }

    public void Lock() {
        state = State.Locked;
        if (playerInput != null) {
            Player.SkinData skin = playerObject.GetComponent<PlayerWalkController>().GetSkinData();
            Player.All.Add(new Player(playerInput, skin));
        }
    }

}
