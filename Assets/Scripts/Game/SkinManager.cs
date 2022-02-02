using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;


[ExecuteAlways]
public class SkinManager : MonoBehaviour {

    public int frameSelected = 0;

    public int framePerDirection = 1;

    public int skinSelected = 0;

    public string spritePath;

    public GameObject perso;

    public List<GameObject> annexLayers;


    private SpriteRenderer spriteRenderer = null;

    private PersoAnimator persoAnimator = null;

    private string loadedSpritePath = null;

    private Dictionary<string, Sprite> spriteSheet = new Dictionary<string, Sprite>();


    // --------------------------
    // Callbacks
    // --------------------------

    private void Start() {
        spriteRenderer = GetComponent<SpriteRenderer>();
        persoAnimator = perso.GetComponent<PersoAnimator>();
        loadedSpritePath = null;
        load();
    }
    private void Awake() {
        Start();
    }
    private void OnValidate() {
        Start();
    }

    // Runs after the animation
    private void LateUpdate() {
        applyReplacement();
    }


    // --------------------------
    // Internals
    // --------------------------

    private void load() {
        if (loadedSpritePath != null || loadedSpritePath != spritePath)  {
            spriteSheet.Clear();
            Sprite[] sprites = Resources.LoadAll<Sprite>(spritePath);
            foreach (var x in sprites) {
                spriteSheet.Add(x.name, x);
            }
        }
        loadedSpritePath = spritePath;
    }

    // compute actual skin sprite name and update it
    // "azaef53454_<nombre>" => "azaef53454_<new index>"
    private void applyReplacement() {
        load();

        var regex = new Regex(@"^(.*_)(\d+)$");
        var match = regex.Match(spriteRenderer.sprite.name);
        string initialPrefix = match.Groups[1].ToString();

        int dirIndex = 0;
        if (persoAnimator.direction == PersoAnimator.Dir.Down) {
            dirIndex = 0;
        }
        if (persoAnimator.direction == PersoAnimator.Dir.Up) {
            dirIndex = 1;
        }
        if (persoAnimator.direction == PersoAnimator.Dir.Right) {
            dirIndex = 2;
        }
        if (persoAnimator.direction == PersoAnimator.Dir.Left) {
            dirIndex = 3;
        }
        foreach (GameObject child in annexLayers) {
            var cmp = child.GetComponent<SkinManager>();
            cmp.frameSelected = frameSelected;
            cmp.skinSelected = skinSelected;
            cmp.applyReplacement();
        }
        string selectedSpriteName = initialPrefix + (frameSelected + dirIndex * framePerDirection + skinSelected * framePerDirection * 4).ToString();

        if (spriteSheet.ContainsKey(selectedSpriteName)) {
            spriteRenderer.sprite = spriteSheet[selectedSpriteName];
        } else {
            Debug.LogWarning(("skin sprite not found ", selectedSpriteName, this, loadedSpritePath));
        }
    }
}
