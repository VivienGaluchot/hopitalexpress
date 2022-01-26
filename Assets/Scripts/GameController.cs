using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour {

	// new patient can only spawn on welcome seats
	[SerializeField] private GameObject[] WelcomeSeats;
	[SerializeField] private GameObject Patient;
	[SerializeField] private float spawnRate;

	private float elapsedTime;

	private int counter;

	private void Update() {
		if(elapsedTime > spawnRate) {
			GameObject emptySeat = null;
			// Looking for an empty welcome seat
			for(int i = 0; i < WelcomeSeats.Length; i++) {
				if(!WelcomeSeats[i].GetComponent<SeatController>().isHolding) {
					emptySeat = WelcomeSeats[i];
					break;
				}
			}

			// An empty welcome seat was found
			if(emptySeat != null) {
				GameObject newPatient = Instantiate(Patient, emptySeat.transform.position, emptySeat.transform.rotation);
				newPatient.name = "patient " + counter++;
				if(!emptySeat.GetComponent<SeatController>().ReceiveHold(newPatient)) {
					// If a problem occurs, delete the patient
					Destroy(newPatient);
				}
				elapsedTime -= spawnRate;
			}

		} else
			elapsedTime += Time.deltaTime;
	}

}
