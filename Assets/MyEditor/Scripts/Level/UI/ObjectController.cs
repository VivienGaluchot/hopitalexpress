using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectController : MonoBehaviour {

	private Sprite sprite;
	private string path;
	private bool isSeat;
	private Vector3 childPos;

	public void SetInformations(Sprite sprite, string path, bool isSeat, Vector3 childPos) {
		this.sprite = sprite;
		this.path = path;
		this.isSeat = isSeat;
		this.childPos = childPos;
	}

	public void SendInformations() {
		LevelEditorController.instance.SetFollower(sprite, path, isSeat, childPos);
	}
}
