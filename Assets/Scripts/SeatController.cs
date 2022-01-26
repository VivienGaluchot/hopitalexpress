using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SeatController : MonoBehaviour {

	public GameObject goHeld { get; protected set; }

	public bool isHolding { get; protected set; }

	public virtual bool ReceiveHold(GameObject target) {
		if(!isHolding) {
			goHeld = target;
			isHolding = true;

			return true;
		}

		return false;
	}

	public virtual GameObject GiveHold() {
		if(isHolding) {
			GameObject returnGO = goHeld;
			goHeld = null;
			isHolding = false;

			return returnGO;
		}

		return null;
	}
}
