using System.Collections.Generic;
using UnityEngine;

public class PlayerActionController : MonoBehaviour {

	[SerializeField] private Transform PlaceHolder;
	private Rigidbody2D rb2D;
	private CircleCollider2D detectionCollider;
	private float range;
	private PlayerWalkController walk;

	// Hold object

	private GameObject HeldGO;

	enum HeldTypes {
		none,
		item,
		fauteuil
	}

	private HeldTypes heldType;

	// Actions

	public delegate bool TryTargetedAction(GameObject target);

	private HashSet<GameObject> targets = new HashSet<GameObject>();

	private Dictionary<(HeldTypes, string), List<TryTargetedAction>> targetedActions;

	public delegate bool TryUntargetedAction();

	private Dictionary<HeldTypes, List<TryUntargetedAction>> untargetedActions;
	

	public enum Actions {
		nothing,
		gathering,
		crafting
	}

	private Actions action;
	private CraftingTableController craftTable;
	private ContainerController containerGathered;

    private void Awake() {
		action = Actions.nothing;
		heldType = HeldTypes.none;
		detectionCollider = GetComponent<CircleCollider2D>();
		walk = GetComponent<PlayerWalkController>();
		rb2D = GetComponent<Rigidbody2D>();
		range = Vector3.Distance(transform.position, detectionCollider.offset * 2);

		targets = new HashSet<GameObject>();
		targetedActions = new Dictionary<(HeldTypes, string), List<TryTargetedAction>>() {
			// nothing in hand
			{ (HeldTypes.none, "Container"), new List<TryTargetedAction>() { TryTakeFromContainer } },
			{ (HeldTypes.none, "Item"), new List<TryTargetedAction>() { TryTakeItemFromGround } },
			{ (HeldTypes.none, "Fauteuil"), new List<TryTargetedAction>() { TryTakeFauteuil } },
			{ (HeldTypes.none, "CraftingTable"), new List<TryTargetedAction>() { TryTakeItemFromCraft } },

			// item in hand
			{ (HeldTypes.item, "Seat"), new List<TryTargetedAction>() { TryGiveItemToPatient } },
			{ (HeldTypes.item, "Fauteuil"), new List<TryTargetedAction>() { TryGiveItemToPatient } },
			{ (HeldTypes.item, "Machine"), new List<TryTargetedAction>() { TryGiveItemToPatient } },
			{ (HeldTypes.item, "Container"), new List<TryTargetedAction>() { TryExchangeFromContainer } },
			{ (HeldTypes.item, "CraftingTable"), new List<TryTargetedAction>() { TryPutItemInCraft } },
			{ (HeldTypes.item, "Trash"), new List<TryTargetedAction>() { TryPutItemInTrash } },

			// fauteuil in hand
			{ (HeldTypes.fauteuil, "Player"), new List<TryTargetedAction>() { TryTakePlayerToFauteuil } },
			{ (HeldTypes.fauteuil, "Seat"), new List<TryTargetedAction>() { TryTakeFromSeatToFauteuil, TryPutFromFauteuilToSeat } },
			{ (HeldTypes.fauteuil, "Fauteuil"), new List<TryTargetedAction>() { TryTakeFromSeatToFauteuil, TryPutFromFauteuilToSeat } },
			{ (HeldTypes.fauteuil, "Machine"), new List<TryTargetedAction>() { TryTakeFromSeatToFauteuil, TryPutFromFauteuilToSeat } },
			{ (HeldTypes.fauteuil, "Trash"), new List<TryTargetedAction>() { TryPutFromFauteuilToTrash } },
			{ (HeldTypes.fauteuil, "Exit"), new List<TryTargetedAction>() { TryPutPatientFromFauteuilToExit } },
		};
		untargetedActions = new Dictionary<HeldTypes, List<TryUntargetedAction>>() {
			// item in hand
			{ HeldTypes.item, new List<TryUntargetedAction>() { TryDropItem } },

			// fauteuil in hand
			{ HeldTypes.fauteuil, new List<TryUntargetedAction>() { DropPlayerFromFauteuil, TryDropFauteuil } },
		};
	}

	private void Update() {
		if(GameController.instance == null || GameController.instance.IsPlaying()) {
			if (action == Actions.gathering && walk.GetInput().GetAction0Up()) {
				// Button released, we stop gathering
				action = Actions.nothing;
				containerGathered.StopGatherItem();
			}

			if (action == Actions.crafting && walk.GetInput().GetAction0Up()) {
				action = Actions.nothing;
				craftTable.StopCraftItem();
			}

			if (walk.GetInput().GetAction0()) {
				PerformAction();
			}
		}
	}

	protected void FixedUpdate() {
		if (GameController.instance == null || GameController.instance.IsPlaying()) {
			Vector3 vDir = Vector3.down;
			switch (walk.direction) {
				case WalkController.Dir.Down:
					vDir = Vector3.down;
					break;
				case WalkController.Dir.Right:
					vDir = Vector3.right;
					break;
				case WalkController.Dir.Left:
					vDir = Vector3.left;
					break;
				case WalkController.Dir.Up:
					vDir = Vector3.up;
					break;
			}
			float detectionOffset = (heldType != HeldTypes.fauteuil) ? 0.25f : 1f;
			detectionCollider.offset = detectionOffset * vDir;
		}
	}
	
	private void OnTriggerEnter2D(Collider2D collision) {
		targets.Add(collision.gameObject);
	}

	private void OnTriggerExit2D(Collider2D collision) {
		targets.Remove(collision.gameObject);
	}

	// Sort list items by distance from the player (closer first)
	private void SortListByDistance(List<GameObject> theList) {
		theList.Sort(delegate (GameObject x, GameObject y) {
			if (Vector3.Distance(transform.position, x.transform.position) > Vector3.Distance(transform.position, y.transform.position))
				return 1;
			else
				return -1;
		});
	}

	// Actions
	
	public Actions GetAction() {
		return action;
	}

	private void PerformAction() {
		// Targeted actions
		List<GameObject> objects = new List<GameObject>();
		foreach (GameObject go in targets) {
			objects.Add(go);
		}
		SortListByDistance(objects);
		bool isDone = false;
		foreach (GameObject go in objects) {
			Vector2 direction = go.transform.position - transform.position;
			RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, direction.magnitude, LayerMask.GetMask("Wall"));
			if(!hit.collider) {
				var key = (heldType, go.tag);
				if (targetedActions.ContainsKey(key)) {
					foreach (TryTargetedAction action in targetedActions[key]) {
						isDone = action(go);
						if (isDone) {
							break;
						}
					}
				}
				if (isDone) {
					break;
				}
			}
		}
		// Non targeted actions
		if (!isDone) {
			var key = heldType;
			if (untargetedActions.ContainsKey(key)) {
				foreach (TryUntargetedAction action in untargetedActions[key]) {
					isDone = action();
					if (isDone) {
						break;
					}
				}
			}
		}
	}

	private bool TryTakeFromContainer(GameObject target) {
		var containerAnswer = target.GetComponent<ContainerController>().StartGatherItem(this, HeldGO);
		if (containerAnswer.givenItem != null) {
			ReceiveItemFromContainer(containerAnswer.givenItem);
			return true;
		} else {
			if (containerAnswer.gathering) {
				action = Actions.gathering;
				containerGathered = target.GetComponent<ContainerController>();

				return true;
			}
		}
		return false;
	}

	private bool TryGiveItemToPatient(GameObject target) {
		var patient = target.GetComponent<SeatController>().goHeld;
		if (patient != null) {
			patient.GetComponent<PatientController>().TakeItem(HeldGO);
			HeldGO = null;
			heldType = HeldTypes.none;
			return true;
		}
		return false;
	}

	private bool TryExchangeFromContainer(GameObject target) {
		// check the container accepts the current item then exchange
		ItemController ic = HeldGO.GetComponent<ItemController>();
		if (ic == null)
			return false;

		string itemName = ic.itemType.ToString();
		if (target.GetComponent<ContainerController>().askedItemName != itemName) {
			return false;
		}

		var containerAnswer = target.GetComponent<ContainerController>().StartGatherItem(this, HeldGO);
		if (containerAnswer.givenItem != null) {
			ReceiveItemFromContainer(containerAnswer.givenItem);
			return true;
		} else {
			if (containerAnswer.gathering) {
				action = Actions.gathering;
				containerGathered = target.GetComponent<ContainerController>();
				return true;
			}
		}

		return false;
	}

	private bool TryTakeItemFromGround(GameObject target) {
		HoldMyBeer(target);
		heldType = HeldTypes.item;
		return true;
	}

	private bool TryDropItem() {
		HeldGO.transform.parent = null;
		HeldGO.transform.rotation = Quaternion.identity;

		RaycastHit2D hit = Physics2D.Raycast(transform.position, detectionCollider.offset, range, LayerMask.GetMask("Wall"));

		if(hit.collider) {
			HeldGO.transform.position = hit.point;
		} else {
			HeldGO.transform.position = transform.position + (Vector3)detectionCollider.offset * 2;
		}

		HeldGO.GetComponent<Rigidbody2D>().simulated = true;
		HeldGO = null;
		heldType = HeldTypes.none;
		return true;
	}

	private bool TryTakeFauteuil(GameObject target) {
		target.GetComponent<WalkFauteuilController>().SetHolder(transform.gameObject);
		HeldGO = target;
		heldType = HeldTypes.fauteuil;
		return true;
	}

	private bool TryDropFauteuil() {
		HeldGO.GetComponent<WalkFauteuilController>().SetHolder(null);
		HeldGO = null;
		heldType = HeldTypes.none;
		return true;
	}

	private bool TryPutItemInCraft(GameObject target) {
		if (target.GetComponent<CraftingTableController>().ReceiveItem(HeldGO)) {
			Destroy(HeldGO);
			heldType = HeldTypes.none;
			return true;
		}
		return false;
	}

	private bool TryTakeItemFromCraft(GameObject target) {
		var craftAnswer = target.GetComponent<CraftingTableController>().StartCraftingItem(this);
		if (craftAnswer.craftedItem != null) {
			ReceiveItemFromContainer(craftAnswer.craftedItem);
		} else {
			if (craftAnswer.isCrafting) {
				action = Actions.crafting;
				craftTable = target.GetComponent<CraftingTableController>();
			}
		}
		return true;
	}

	private bool TryPutItemInTrash(GameObject target) {
		Destroy(HeldGO);
		HeldGO = null;
		heldType = HeldTypes.none;
		target.GetComponent<Animator>().SetTrigger("activate");
		return true;
	}

	private bool TryTakeFromSeatToFauteuil(GameObject target) {
		return target.GetComponent<SeatController>().TryTansfertTo(HeldGO.GetComponent<WalkFauteuilController>().seat);
	}

	private bool TryPutFromFauteuilToSeat(GameObject target) {
		return HeldGO.GetComponent<WalkFauteuilController>().seat.TryTansfertTo(target.GetComponent<SeatController>());
	}

	private bool TryTakePlayerToFauteuil(GameObject target) {
		return HeldGO.GetComponent<WalkFauteuilController>().seat.ReceiveHold(target);
	}

	private bool DropPlayerFromFauteuil() {
		// check if player is held in fauteuil
		if (HeldGO.GetComponent<WalkFauteuilController>()?.seat.goHeld?.GetComponent<PlayerActionController>() == null) {
			return false;
		}
		var target = HeldGO.GetComponent<WalkFauteuilController>().seat.GiveHold();

		return true;
	}

	private bool TryPutFromFauteuilToTrash(GameObject target) {
		// check if patient is held in fauteuil
		if (HeldGO.GetComponent<WalkFauteuilController>()?.seat.goHeld?.GetComponent<PatientController>() == null) {
			return false;
		}
		var patient = HeldGO.GetComponent<WalkFauteuilController>().seat.GiveHold();
		if (patient) {
			patient.GetComponent<PatientController>().Exited();
			Destroy(patient);
			target.GetComponent<Animator>().SetTrigger("activate");
			return true;
		}
		return false;
	}

	private bool TryPutPatientFromFauteuilToExit(GameObject target) {
		// check if patient is held in fauteuil
		if (HeldGO.GetComponent<WalkFauteuilController>()?.seat.goHeld?.GetComponent<PatientController>() == null) {
			return false;
		}
		if (HeldGO.GetComponent<WalkFauteuilController>().seat.isHolding) {
			var patient = HeldGO.GetComponent<WalkFauteuilController>().seat.goHeld;
			if (patient.GetComponent<PatientController>().state == PatientController.States.cured) {
				HeldGO.GetComponent<WalkFauteuilController>().seat.GiveHold();
				patient.GetComponent<PatientController>().Exited();
				Destroy(patient);
				return true;
			}
		}
		return false;
	}

	public void ReceiveItemFromContainer(GameObject item) {
		action = Actions.nothing;
		Destroy(HeldGO);
		HoldMyBeer(item);
		heldType = HeldTypes.item;
	}

	private void HoldMyBeer(GameObject Beer) {
		HeldGO = Beer;
		HeldGO.GetComponent<Rigidbody2D>().simulated = false;
		HeldGO.transform.parent = PlaceHolder;
		HeldGO.transform.localRotation = Quaternion.identity;
		HeldGO.transform.localPosition = Vector3.zero;
	}
}
