using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Kyantest : MonoBehaviour {

    public Sprite[] sprites;

    private void Start() {

        for(int i = 0; i < sprites.Length; i++) {
            GameObject second = new GameObject(sprites[i].name, typeof(SpriteRenderer), typeof(BoxCollider2D));
            second.transform.SetParent(transform);
            Vector2 pivot = new Vector2(sprites[i].pivot.x / sprites[i].rect.width, sprites[i].pivot.y / sprites[i].rect.height);
            second.transform.position = new Vector3(i, 0f, 0f);
            SpriteRenderer sr = second.GetComponent<SpriteRenderer>();
            sr.sprite = sprites[i];
            Vector2 offset = pivot - new Vector2(.5f, .5f);
            second.transform.position += (Vector3) offset;
            sr.drawMode = SpriteDrawMode.Sliced;

            if (sr.size.x > sr.size.y) {
                sr.size = new Vector2(1f, sr.size.y / sr.size.x);
            } else {
                sr.size = new Vector2(sr.size.x / sr.size.y, 1f);
            }

            BoxCollider2D bc2D = second.GetComponent<BoxCollider2D>();
            bc2D.size = Vector2.one;
            bc2D.offset = -offset;
        }
    }
}
