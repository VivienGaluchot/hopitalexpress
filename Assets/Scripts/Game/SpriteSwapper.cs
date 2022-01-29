using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteSwapper : MonoBehaviour {
    public string skinIndex;
    public string spriteSheetName;
    
    private string loadedSpriteSheetName;

    private Dictionary<string, Sprite> spriteSheet;

    private SpriteRenderer spriteRenderer;

    private void Start() {
        this.spriteRenderer = GetComponent<SpriteRenderer>();

        this.LoadSpriteSheet();
    }

    // Runs after the animation has done its work
    private void LateUpdate()
    {
        // Check if the sprite sheet name has changed (possibly manually in the inspector)
        if (this.loadedSpriteSheetName != this.spriteSheetName) {
            // Load the new sprite sheet
            this.LoadSpriteSheet();
        }

        // TODO don't work when len skinIndex > 1
        var currentSprite = this.spriteRenderer.sprite.name;
        var newSprite = this.skinIndex + currentSprite.Substring(1);

        // Swap out the sprite to be rendered by its name
        // Important: The name of the sprite must be the same!
        this.spriteRenderer.sprite = this.spriteSheet[newSprite];
    }

    // Loads the sprites from a sprite sheet
    private void LoadSpriteSheet() {
        // Load the sprites from a sprite sheet file (png). 
        // Note: The file specified must exist in a folder named Resources
        var sprites = Resources.LoadAll<Sprite>(this.spriteSheetName);
        this.spriteSheet = new Dictionary<string, Sprite>();
        foreach (var x in sprites) {
            this.spriteSheet.Add(x.name, x);
        }

        // Remember the name of the sprite sheet in case it is changed later
        this.loadedSpriteSheetName = this.spriteSheetName;
    }
}
