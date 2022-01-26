using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Container : MonoBehaviour {

    public GameObject item;

    private void Start() {
        // Display item
        transform.GetChild(0).GetComponent<SpriteRenderer>().sprite = item.GetComponent<SpriteRenderer>().sprite;
    }

    public GameObject GiveItem() {
        return Instantiate(item);
    }

}
