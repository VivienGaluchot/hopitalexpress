using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ContainerController : MonoBehaviour {

	public GameObject askedItem, givenItem;
	public float askedTime;

	public string askedItemName { get; private set; }

	private float elapsedTime;

	private PlayerController target;

	private bool isGathering;

	public Image TimeBar;

	private void Start() {
		TimeBar.transform.parent.gameObject.SetActive(false);
		isGathering = false;

		if (askedItem != null)
			askedItemName = askedItem.GetComponent<ItemController>().itemName;
	}

	private void Update() {
		if(isGathering) {
			elapsedTime += Time.deltaTime;
			TimeBar.fillAmount = elapsedTime / askedTime;
			if (elapsedTime > askedTime) {
				isGathering = false;
				TimeBar.transform.parent.gameObject.SetActive(false);
				target.ReceiveItemFromContainer(Instantiate(givenItem));
			}
		}
	}

	public (GameObject givenItem, bool gathering) StartGatherItem(PlayerController player, GameObject item = null)  {
		if (askedItem == null || item != null && item.GetComponent<ItemController>().itemName == askedItemName) {
			if (askedTime == 0f) {
				return (Instantiate(givenItem), true);
			} else {
				if (player == null)
					return (null, false);

				target = player;
				elapsedTime = 0f;

				TimeBar.transform.parent.gameObject.SetActive(true);
				TimeBar.fillAmount = elapsedTime / askedTime;
				isGathering = true;
				return (null, true);
			}
		}

		return (null, false);
	}

	public void StopGatherItem() {
		TimeBar.transform.parent.gameObject.SetActive(false);
		isGathering = false;
	}
}
