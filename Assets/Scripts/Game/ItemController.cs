using UnityEngine;

public enum Items {
	PiluleBleue,
	PiluleVerte,
	PiluleRouge,
	PiluleJaune,
	SeringueVide,
	SeringueBleue,
	SeringueVerte,
	SeringueRouge,
	SeringueJaune,
	Swab,
	SwabUsed,
	CovidResult
}

public class ItemController : MonoBehaviour {

	public Items itemType;

	public GameObject swapTo;

}
