using UnityEngine;

public class PlayerWalkController : WalkController {

	public GameObject hair;

	public GameObject skin;

	public GameObject clothes;

	public float maxSpeed;

	public float speepRate = 1;


	protected PlayerInput playerInput;


    protected override void Start() {
		base.Start();
	}

	protected virtual void FixedUpdate() {
		if (GameController.instance == null || GameController.instance.IsPlaying()) {
			bool isInAction = false;
			if (GetComponent<PlayerActionController>() && GetComponent<PlayerActionController>().enabled) {
				isInAction = GetComponent<PlayerActionController>().GetAction() != PlayerActionController.Actions.nothing;
			}
			if (playerInput != null && !isSeated && !isInAction) {
				Vector3 input = Vector2.ClampMagnitude(new Vector2(playerInput.GetX(), playerInput.GetY()), 1);
				if (input.sqrMagnitude > (0.1 * 0.1)) {
					SetStoppedDirection(input);
				}
				rb2D.velocity = input * maxSpeed * speepRate;
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

	public PlayerInput GetInput() {
		return playerInput;
	}

}
