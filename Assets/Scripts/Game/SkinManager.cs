using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

public class SkinManager : MonoBehaviour {

    public int selected = 0;

    public int skinOffset = 0;

    public string spritePath;


    // --------------------------
    // Callbacks
    // --------------------------

    private void Awake() {
        this.spriteRenderer = GetComponent<SpriteRenderer>();
        load();
    }

    private void Start() {
        this.spriteRenderer = GetComponent<SpriteRenderer>();
        load();
    }

    // Runs after the animation
    private void LateUpdate() {
        applyReplacement();
    }


    // --------------------------
    // Internals
    // --------------------------

    private string loadedSpritePath;
    private SpriteRenderer spriteRenderer;
    private Dictionary<string, Sprite> spriteSheet;

    private void load() {
        if (this.loadedSpritePath != this.spritePath)  {
            Sprite[] sprites = Resources.LoadAll<Sprite>(this.spritePath);
            this.spriteSheet = new Dictionary<string, Sprite>();
            foreach (var x in sprites) {
                this.spriteSheet.Add(x.name, x);
            }
        }
        this.loadedSpritePath = this.spritePath;
    }

    // compute actual skin sprite name and update it
    // "azaef53454_<nombre>" => "azaef53454_<nombre + selected * skinOffset>"
    private void applyReplacement() {
        load();

        var regex = new Regex(@"^(.*_)(\d+)$");
        var match = regex.Match(this.spriteRenderer.sprite.name);
        var initialPrefix = match.Groups[1].ToString();
        var initialIndex = int.Parse(match.Groups[2].ToString());

        var selectedSpriteName = initialPrefix + (initialIndex + selected * skinOffset).ToString();

        if (this.spriteSheet.ContainsKey(selectedSpriteName)) {
            this.spriteRenderer.sprite = this.spriteSheet[selectedSpriteName];
        } else {
            Debug.LogWarning(("skin sprite not found ", selectedSpriteName));
        }
    }
}
