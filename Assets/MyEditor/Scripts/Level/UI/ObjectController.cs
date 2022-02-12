using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectController : MonoBehaviour {

	private Sprite sprite;
	private string path, prefabTag;
	private Vector3 childPos;

	public void SetInformations(Sprite sprite, string path, string prefabTag, Vector3 childPos) {
		this.sprite = sprite;
		this.path = path;
		this.prefabTag = prefabTag;
		this.childPos = childPos;
	}

	public void SendInformations() {
		LevelEditorController.instance.SetFollower(sprite, path, prefabTag, childPos);
	}
}
