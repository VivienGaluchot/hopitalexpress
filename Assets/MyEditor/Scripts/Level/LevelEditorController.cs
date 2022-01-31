using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelEditorController : MonoBehaviour {



    private void Start() {
        DisplayGrid();
    }

    private void DisplayGrid() {
        Transform parent = new GameObject("Cells").transform;
        int counter = 0;
        for(int i = 0; i < 100; i++) {
            for(int j = 0; j < 100; j++) {
                GameObject newGO = new GameObject("Cell" + counter++, typeof(SpriteRenderer)/*, typeof(BoxCollider2D)*/);
                newGO.transform.SetParent(parent);
                newGO.transform.position = new Vector3(-50+j, -50+i, 0);
                Sprite s = Sprite.Create(Texture2D.whiteTexture, new Rect(0, 0, 1, 1), new Vector2(.5f, .5f), 1f);
                newGO.GetComponent<SpriteRenderer>().sprite = s;
                newGO.GetComponent<SpriteRenderer>().color = (i + j) % 2 == 0 ? Color.black : Color.white;
            }
        }
        Debug.Log("done");
    }
}
