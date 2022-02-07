using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;


[ExecuteAlways]
public class SkinManager : MonoBehaviour {

	public int frameSelected = 0;

	public int framePerDirection = 1;

	public int skinSelected = 0;

	public int skinMin = 0;

	public int skinMax = 0;

	public string spritePath;

	public GameObject perso;

	public List<GameObject> annexLayers;


	public bool enableSeatFrame = false;

	public int seatFrame = 0;

	public bool enableSeatTransform = false;

	public Vector2 seatPosition = new Vector2(0, 0);

	public Vector2 seatScale = new Vector2(1, 1);

	private (bool isPending, Vector2 pos, Vector2 scale) preSteatTransform = (false, new Vector2(), new Vector2());


	private SpriteRenderer spriteRenderer = null;

	private WalkController walkController = null;

	private string loadedSpritePath = null;

	private Dictionary<string, Sprite> spriteSheet = new Dictionary<string, Sprite>();


	// --------------------------
	// Callbacks
	// --------------------------

	private void Start() {
		spriteRenderer = GetComponent<SpriteRenderer>();
		walkController = perso.GetComponent<WalkController>();
		loadedSpritePath = null;
		load();
	}
	private void Awake() {
		Start();
	}
	private void OnValidate() {
		Start();
	}

	public void NextSkinIndex() {
		skinSelected=skinSelected+1;
		if (skinSelected>skinMax) {
			skinSelected=skinMin;
		}
	}

	public void PreviousSkinIndex() {
		skinSelected=skinSelected-1;
		if (skinSelected<skinMin) {
			skinSelected=skinMax;
		}
	}

	public void Update() {
		if (walkController.isSeated && enableSeatTransform) {
			if (!preSteatTransform.isPending) {
				preSteatTransform.pos = transform.localPosition;
				preSteatTransform.scale = transform.localScale;
				preSteatTransform.isPending = true;
			}
			transform.localPosition = seatPosition;
			transform.localScale = seatScale;
		} else if (preSteatTransform.isPending) {
			transform.localPosition = preSteatTransform.pos;
			transform.localScale = preSteatTransform.scale;
			preSteatTransform.isPending = false;
		}
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
		if (walkController.direction == WalkController.Dir.Down) {
			dirIndex = 0;
		} else if (walkController.direction == WalkController.Dir.Up) {
			dirIndex = 1;
		} else if (walkController.direction == WalkController.Dir.Right) {
			dirIndex = 2;
		} else if (walkController.direction == WalkController.Dir.Left) {
			dirIndex = 3;
		}

		int frame = frameSelected;
		if (walkController.isSeated && enableSeatFrame) {
			frame = seatFrame;
		}

		foreach (GameObject child in annexLayers) {
			var cmp = child.GetComponent<SkinManager>();
			cmp.frameSelected = frame;
			cmp.skinSelected = skinSelected;
			cmp.applyReplacement();
		}

		int index = frame + dirIndex * framePerDirection + skinSelected * framePerDirection * 4;
		string selectedSpriteName = initialPrefix + index.ToString();

		if (spriteSheet.ContainsKey(selectedSpriteName)) {
			spriteRenderer.sprite = spriteSheet[selectedSpriteName];
		} else {
			Debug.LogWarning(("skin sprite not found ", selectedSpriteName, walkController.direction, frameSelected, skinSelected, framePerDirection));
		}
	}
}
