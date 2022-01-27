using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class CraftingTable : MonoBehaviour {

	public SpriteRenderer[] ItemsSR;
	public Image TimeBar;

	private Dictionary<string, int> items;

	// Store craft recipes
	public static Dictionary<string, Dictionary<string, int>> Recipes = new Dictionary<string, Dictionary<string, int>>() {
		{ "BluePill", new Dictionary<string, int>() { { "GreenPill", 2 } } },
		{ "GreenPill", new Dictionary<string, int>() { { "BluePill", 1 }, { "EmptySyringe", 1} } }
	};

	private string path;
	private int counter;

	private void Start() {
		TimeBar.transform.parent.gameObject.SetActive(false);
		counter = 0;
		path = "Prefabs/Treatments/Items/";
		items = new Dictionary<string, int>();
	}

	public bool ReceiveItem(GameObject item) {
		if(counter < ItemsSR.Length) {
			// We still have room to this new item
			ItemController ic = item.GetComponent<ItemController>();
			if(ic != null) {
				// Display sprite and store item
				ItemsSR[counter++].sprite = item.GetComponent<SpriteRenderer>().sprite;
				if (items.ContainsKey(ic.itemName))
					items[ic.itemName]++;
				else
					items.Add(ic.itemName, 1);

				return true;
			}
		}

		return false;
	}

	public GameObject GiveItem() {
		// Iterate over the recipes to find one who match our current items
		bool craftableFound = false;
		string craftName = "";
		foreach (KeyValuePair<string, Dictionary<string, int>> recipe in Recipes) {
			craftableFound = true;
			// We say it's good, then if we find a problem we say it's not good
			foreach(KeyValuePair<string, int> ingredient in recipe.Value) {
				if (!items.ContainsKey(ingredient.Key)) {
					craftableFound = false;
					break;
				} else if (items[ingredient.Key] != ingredient.Value) {
					craftableFound = false;
					break;
				}
			}

			// We found a recipe where we have everything
			if(craftableFound) {
				// But do we have too much?
				foreach(KeyValuePair<string, int> item in items) {
					// Check if we have an ingredient in recipe named like our item
					if(!recipe.Value.ContainsKey(item.Key)) {
						craftableFound = false;
						break;
						// Check if this ingredients need the same amount that we have
                    } else if (recipe.Value[item.Key] != item.Value) {
						craftableFound = false;
						break;
                    }
                }
				if(craftableFound) {
					// If we're here, then we have exactly what we need
					// We remember the name of the item to craft and we exit the loop victorious
					craftName = recipe.Key;
					break;
				}
			}
		}

		// We found a craft ! Let's do it
		if (craftableFound) {
			ClearTable();

			return Instantiate(Resources.Load<GameObject>(path + craftName));
		}

		// We can't craft anything, but it's full, so we clear it
		if(counter == ItemsSR.Length) {
			ClearTable();
		}

		// We could empty it no matter what maybe

		return null;
	}

	private void ClearTable() {
		foreach (SpriteRenderer sr in ItemsSR)
			sr.sprite = null;

		items.Clear();
		counter = 0;
	}

}
