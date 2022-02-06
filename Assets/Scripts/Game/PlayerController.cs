using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour {

	private GameController gc;
	private int id;
	[SerializeField] private Transform PlaceHolder;
	private float speed;
	private Rigidbody2D rb2D;

	private List<GameObject> seatTargets, itemTargets, containerTargets, fauteuilTargets;
	private GameObject trashTarget, exitTarget, craftingTableTarget;
	private GameObject HeldGO;


	enum HeldTypes {
		none,
		item,
		fauteuil
	}

	private HeldTypes heldType;

	enum Actions {
		nothing,
		gathering,
		crafting
	}
	private Actions action;
	private CraftingTableController craftTable;
	private ContainerController containerGathered;

	private CapsuleCollider2D detectionCollider;

	private WalkController perso;

	public void Initialize(int _id, GameController parent, float _speed) {
		id = _id;
		gc = parent;
		speed = _speed;
	}

	private void Start() {
		action = Actions.nothing;
		seatTargets = new List<GameObject>();
		itemTargets = new List<GameObject>();
		containerTargets = new List<GameObject>();
		fauteuilTargets = new List<GameObject>();
		heldType = HeldTypes.none;
		detectionCollider = GetComponent<CapsuleCollider2D>();
		perso = GetComponent<WalkController>();
		rb2D = GetComponent<Rigidbody2D>();
	}

	private void Update() {
		if (action == Actions.gathering && Input.GetButtonUp("Fire" + id)) {
			// Button released, we stop gathering
			action = Actions.nothing;
			containerGathered.StopGatherItem();
		}

		if (action == Actions.crafting && Input.GetButtonUp("Fire" + id)) {
			action = Actions.nothing;
			craftTable.StopCraftItem();
		}

		if (Input.GetButtonDown("Fire" + id)) {
			// Are we already holding something?
			switch (heldType) {
				case HeldTypes.item:
					// We try to give the item, if we can't, then drop it
					if (!TryGiveItemToPatient())
						if (!TryTakeFromContainer(true))
							if (!TryPutItemInCraft())
								if (!TryPutInTrash())
									TryDropItem();
					break;
				case HeldTypes.fauteuil:
					if (!TryTakePatientFromSeatToFauteuil())
						if (!TryPutPatientFromFauteuilToSeat())
							if (!TryPutFromFauteuilToTrash())
								if (!TryPutPatientFromFauteuilToExit())
									TryDropFauteuil();
					break;
				case HeldTypes.none:
					// Holding nothing, can we grab something?
					if (!TryTakeFauteuil())
						if (!TryTakeItemFromGround())
							if (!TryTakeItemFromCraft())
								TryTakeFromContainer();
					break;
			}
		}
	}

	void FixedUpdate() {
		if (action == Actions.nothing) {
			// Can only move if doing nothing
			float horiz = Input.GetAxis("Joy" + id + "X"), vert = Input.GetAxis("Joy" + id + "Y");
			Vector3 input = Vector2.ClampMagnitude(new Vector2(horiz, vert), 1);
			if (input.sqrMagnitude > (0.1 * 0.1)) {
				perso.SetStoppedDirection(input);
			}
			rb2D.velocity = input * speed;
		}
		Vector3 vDir = Vector3.down;
		switch (perso.direction) {
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

	// Sort list items by distance from the player (closer first)
	private void SortListByDistance(List<GameObject> theList) {
		theList.Sort(delegate (GameObject x, GameObject y) {
			if (Vector3.Distance(transform.position, x.transform.position) > Vector3.Distance(transform.position, y.transform.position))
				return 1;
			else
				return -1;
		});
	}

	private bool TryTakeItemFromGround() {
		// Look for item nearby
		if (itemTargets.Count > 0) {
			// Sort by distance
			SortListByDistance(itemTargets);
			HoldMyBeer(itemTargets[0]);
			heldType = HeldTypes.item;
			return true;
		}

		return false;
	}

	private bool TryDropItem() {
		HeldGO.transform.parent = null;
		HeldGO.transform.rotation = Quaternion.identity;
		HeldGO.transform.position = transform.position + (Vector3)detectionCollider.offset * 2;
		HeldGO.GetComponent<Rigidbody2D>().simulated = true;
		HeldGO = null;
		heldType = HeldTypes.none;
		return true;
	}

	private bool TryTakeFauteuil() {
		// Look for item nearby
		if (fauteuilTargets.Count > 0) {
			// Sort by distance
			SortListByDistance(fauteuilTargets);
			fauteuilTargets[0].GetComponent<FauteuilController>().SetHolder(transform.gameObject);
			HeldGO = fauteuilTargets[0];
			heldType = HeldTypes.fauteuil;
			return true;
		}
		return false;
	}

	private bool TryDropFauteuil() {
		HeldGO.GetComponent<FauteuilController>().SetHolder(null);
		HeldGO = null;
		heldType = HeldTypes.none;
		return true;
	}

	private bool TryGiveItemToPatient() {
		// Look for closer patient to give item
		if (seatTargets.Count > 0) {
			SortListByDistance(seatTargets);

			GameObject occupiedSeat = null;
			for (int i = 0; i < seatTargets.Count; i++) {
				if (seatTargets[i].GetComponent<SeatController>().isHolding) {
					occupiedSeat = seatTargets[i];
					break;
				}
			}

			if (occupiedSeat != null) {
				occupiedSeat.GetComponent<SeatController>().goHeld.GetComponent<PatientController>().TakeItem(HeldGO);
				HeldGO = null;
				heldType = HeldTypes.none;

				return true;
			}
		}
		return false;
	}

	private bool TryPutItemInCraft() {
		if (craftingTableTarget != null) {
			if (craftingTableTarget.GetComponent<CraftingTableController>().ReceiveItem(HeldGO)) {
				Destroy(HeldGO);
				heldType = HeldTypes.none;

				return true;
			}
		}

		return false;
	}

	private bool TryTakeItemFromCraft() {
		if (craftingTableTarget != null) {
			var craftAnswer = craftingTableTarget.GetComponent<CraftingTableController>().StartCraftingItem(this);
			if (craftAnswer.craftedItem != null) {
				ReceiveItemFromContainer(craftAnswer.craftedItem);
				return true;
			} else {
				if (craftAnswer.isCrafting) {
					action = Actions.crafting;
					craftTable = craftingTableTarget.GetComponent<CraftingTableController>();
					return true;
				}
			}
		}

		return false;
	}

	private bool TryTakeFromContainer(bool hasItem = false) {
		if (containerTargets.Count > 0) {
			SortListByDistance(containerTargets);

			GameObject container = null;
			if (hasItem) {
				ItemController ic = HeldGO.GetComponent<ItemController>();
				if (ic == null)
					return false;
				string itemName = ic.itemType.ToString();
				for (int i = 0; i < containerTargets.Count; i++) {
					if (containerTargets[i].GetComponent<ContainerController>().askedItemName == itemName) {
						container = containerTargets[i];
						break;
					}
				}
			} else
				container = containerTargets[0];

			if (container) {
				var containerAnswer = container.GetComponent<ContainerController>().StartGatherItem(this, HeldGO);
				if (containerAnswer.givenItem != null) {
					ReceiveItemFromContainer(containerAnswer.givenItem);

					return true;
				} else {
					if (containerAnswer.gathering) {
						action = Actions.gathering;
						containerGathered = container.GetComponent<ContainerController>();

						return true;
					}
				}
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

	private bool TryTakePatientFromSeatToFauteuil() {
		// Look for a not empty seat in seatTargets
		if (seatTargets.Count > 0) {
			// Step 1 : sort by distance
			SortListByDistance(seatTargets);

			// Step 2 : look for occupied seat
			GameObject occupiedSeat = null;
			for (int i = 0; i < seatTargets.Count; i++) {
				if (seatTargets[i].GetComponent<SeatController>().isHolding) {
					occupiedSeat = seatTargets[i];
					break;
				}
			}

			// Step 3 : Take patient from seat if found
			if (occupiedSeat != null)  {
				return occupiedSeat.GetComponent<SeatController>().TryTansfertTo(HeldGO.GetComponent<FauteuilController>().seat);
			}
		}

		return false;
	}

	private bool TryPutPatientFromFauteuilToSeat() {
		// Look for an empty seat in seatTargets
		if (seatTargets.Count > 0) {
			// Step 1 : sort by distance
			SortListByDistance(seatTargets);

			// Step 2 : look for empty seat
			GameObject emptySeat = null;
			for (int i = 0; i < seatTargets.Count; i++) {
				if (!seatTargets[i].GetComponent<SeatController>().isHolding) {
					emptySeat = seatTargets[i];
					break;
				}
			}

			// Step 3 : Allocate patient to fauteuil seat if found
			if (emptySeat != null) {
				return HeldGO.GetComponent<FauteuilController>().seat.TryTansfertTo(emptySeat.GetComponent<SeatController>());
			}
		}

		return false;
	}

	private bool TryPutFromFauteuilToTrash() {
		if (trashTarget != null) {
			var target = HeldGO.GetComponent<FauteuilController>().seat.GiveHold();
			if (target) {
				PatientController pc = target.GetComponent<PatientController>();
				if (pc != null)
					gc.PatientDead(pc);
				Destroy(target);
				trashTarget.GetComponent<Animator>().SetTrigger("activate");
				return true;
			}
		}
		return false;
	}

	private bool TryPutPatientFromFauteuilToExit() {
		if (exitTarget && HeldGO.GetComponent<FauteuilController>().seat.isHolding) {
			var target = HeldGO.GetComponent<FauteuilController>().seat.goHeld;
			if (target.GetComponent<PatientController>().state == PatientController.States.cured) {
				HeldGO.GetComponent<FauteuilController>().seat.GiveHold();
				gc.PatientCured(target.GetComponent<PatientController>());
				Destroy(target);
				return true;
			}
		}
		return false;
	}

	private bool TryPutInTrash() {
		if (trashTarget != null) {
			PatientController pc = HeldGO.GetComponent<PatientController>();
			if (pc != null)
				gc.PatientDead(pc);

			Destroy(HeldGO);
			HeldGO = null;
			heldType = HeldTypes.none;

			trashTarget.GetComponent<Animator>().SetTrigger("activate");

			return true;
		}

		return false;
	}

	private void HoldMyBeer(GameObject Beer) {
		HeldGO = Beer;
		HeldGO.GetComponent<Rigidbody2D>().simulated = false;
		HeldGO.transform.parent = PlaceHolder;
		HeldGO.transform.localRotation = Quaternion.identity;
		HeldGO.transform.localPosition = Vector3.zero;
	}

	private void OnTriggerEnter2D(Collider2D collision) {
		if (collision.gameObject.tag == "Seat" || collision.gameObject.tag == "Machine" || collision.gameObject.tag == "Fauteuil") {
			if (!seatTargets.Contains(collision.gameObject))
				seatTargets.Add(collision.gameObject);
		}
		if (collision.gameObject.tag == "Item") {
			if (!itemTargets.Contains(collision.gameObject))
				itemTargets.Add(collision.gameObject);
		}
		if (collision.gameObject.tag == "Container") {
			if (!containerTargets.Contains(collision.gameObject))
				containerTargets.Add(collision.gameObject);
		}
		if (collision.gameObject.tag == "Trash") {
			trashTarget = collision.gameObject;
		}
		if (collision.gameObject.tag == "Exit") {
			exitTarget = collision.gameObject;
		}
		if (collision.gameObject.tag == "CraftingTable") {
			craftingTableTarget = collision.gameObject;
		}
		if (collision.gameObject.tag == "Fauteuil") {
			if (!fauteuilTargets.Contains(collision.gameObject))
				fauteuilTargets.Add(collision.gameObject);
		}
	}

	private void OnTriggerExit2D(Collider2D collision) {
		if (seatTargets.Contains(collision.gameObject))
			seatTargets.Remove(collision.gameObject);
		if (itemTargets.Contains(collision.gameObject))
			itemTargets.Remove(collision.gameObject);
		if (containerTargets.Contains(collision.gameObject))
			containerTargets.Remove(collision.gameObject);
		if (trashTarget = collision.gameObject)
			trashTarget = null;
		if (exitTarget = collision.gameObject)
			exitTarget = null;
		if (craftingTableTarget = collision.gameObject)
			craftingTableTarget = null;
		if (fauteuilTargets.Contains(collision.gameObject))
			fauteuilTargets.Remove(collision.gameObject);
	}
}
