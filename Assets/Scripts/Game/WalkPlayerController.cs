using UnityEngine;

public class WalkPlayerController : WalkController {

	public GameObject hair;

	public GameObject skin;

	public GameObject clothes;

	public float speed;


	private PlayerInput playerInput;


    protected override void Start() {
		base.Start();
	}

	void FixedUpdate() {
		// TODO to test in level selection
		// put in common with player controller to also work in game
		if (GetComponent<PlayerController>() == null || !GetComponent<PlayerController>().enabled) {
			if (playerInput != null) {
				Vector3 input = Vector2.ClampMagnitude(new Vector2(playerInput.GetX(), playerInput.GetY()), 1);
				if (input.sqrMagnitude > (0.1 * 0.1)) {
					SetStoppedDirection(input);
				}
				if (rb2D.simulated) {
					rb2D.velocity = input * speed;
				}
			}
		}
	}

	public void SetupForPlayer(Player playerData) {
		playerInput = playerData.input;
		hair.GetComponent<SkinManager>().SetSkinIndex(playerData.skin.headId);
		skin.GetComponent<SkinManager>().SetSkinIndex(playerData.skin.skinId);
		clothes.GetComponent<SkinManager>().SetSkinIndex(playerData.skin.clothesId);
	}

	public Player.SkinData GetSkinData() {
		var skinData = new Player.SkinData();
		skinData.headId = hair.GetComponent<SkinManager>().GetSkinIndex();
		skinData.skinId = skin.GetComponent<SkinManager>().GetSkinIndex();
		skinData.clothesId = clothes.GetComponent<SkinManager>().GetSkinIndex();
		return skinData;
	}

}
