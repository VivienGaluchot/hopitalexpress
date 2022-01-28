using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class CraftingTable : MonoBehaviour {

	public SpriteRenderer[] ItemsSR;
	public Image TimeBar;

	private Dictionary<Items, int> items;

	private readonly struct CraftingRecipe {
		public CraftingRecipe(Items result, float craftingTime, Dictionary<Items, int> ingredients) {
			_result = result;
			_craftingTime = craftingTime;
			_ingredients = ingredients;
        }

		public readonly Items _result;
		public readonly float _craftingTime;
		public readonly Dictionary<Items, int> _ingredients;
    }

	// Store craft recipes
	private static CraftingRecipe[] Recipes = new CraftingRecipe[2] {
		new CraftingRecipe(Items.BluePill, 3f, new Dictionary<Items, int>() { { Items.GreenPill, 2 } }),
		new CraftingRecipe(Items.GreenPill, 5f, new Dictionary<Items, int>() { { Items.BluePill, 1 }, {Items.EmptySyringe, 2 } }),
	};

	private string path;
	private int counter;

	private PlayerController target;
	private bool isCrafting;
	private float elapsedTime;
	private CraftingRecipe craftedItem;

	private void Start() {
		TimeBar.transform.parent.gameObject.SetActive(false);
		counter = 0;
		path = "Prefabs/Treatments/Items/";
		items = new Dictionary<Items, int>();
		isCrafting = false;
	}

    private void Update() {
		if (isCrafting) {
			elapsedTime += Time.deltaTime;
			TimeBar.fillAmount = elapsedTime / craftedItem._craftingTime;
			if(elapsedTime > craftedItem._craftingTime) {
				ClearTable();
				TimeBar.transform.parent.gameObject.SetActive(false);
				isCrafting = false;
				target.ReceiveItemFromContainer(Instantiate(Resources.Load<GameObject>(path + craftedItem._result)));
			}
		}
	}

	public void StopCraftItem() {
		TimeBar.transform.parent.gameObject.SetActive(false);
		isCrafting = false;
    }

	public bool ReceiveItem(GameObject item) {
		if(counter < ItemsSR.Length) {
			// We still have room to this new item
			ItemController ic = item.GetComponent<ItemController>();
			if(ic != null) {
				// Display sprite and store item
				ItemsSR[counter++].sprite = item.GetComponent<SpriteRenderer>().sprite;
				Items parsedItem = (Items)Enum.Parse(typeof(Items), ic.itemName);
				if (items.ContainsKey(parsedItem))
					items[parsedItem]++;
				else
					items.Add(parsedItem, 1);

				return true;
			}
		}

		return false;
	}

	public (GameObject craftedItem, bool isCrafting) StartCraftingItem(PlayerController player) {
		// Iterate over the recipes to find one who match our current items
		bool craftableFound = false;
		foreach (CraftingRecipe recipe in Recipes) {
			craftableFound = true;
			// We say it's good, then if we find a problem we say it's not good
			foreach (KeyValuePair<Items, int> ingredient in recipe._ingredients) {
				if (!items.ContainsKey(ingredient.Key)) {
					craftableFound = false;
					break;
				} else if (items[ingredient.Key] != ingredient.Value) {
					craftableFound = false;
					break;
				}
			}

			// We found a recipe where we have everything
			if (craftableFound) {
				// But do we have too much?
				foreach (KeyValuePair<Items, int> item in items) {
					// Check if we have an ingredient in recipe named like our item
					if (!recipe._ingredients.ContainsKey(item.Key)) {
						craftableFound = false;
						break;
						// Check if this ingredients need the same amount that we have
					} else if (recipe._ingredients[item.Key] != item.Value) {
						craftableFound = false;
						break;
					}
				}
				if (craftableFound) {
					// If we're here, then we have exactly what we need
					// We remember the item to craft and we exit the loop victorious
					craftedItem = recipe;
					break;
				}
			}
		}

		// We found a craft ! Let's do it
		if (craftableFound) {
			if (craftedItem._craftingTime == 0) {
				return (Instantiate(Resources.Load<GameObject>(path + craftedItem._result)), true);
			} else {
				if (player == null)
					return (null, false);

				target = player;
				elapsedTime = 0f;

				TimeBar.transform.parent.gameObject.SetActive(true);
				TimeBar.fillAmount = elapsedTime / craftedItem._craftingTime;
				isCrafting = true;
				return (null, true);
            }
		}

		// We can't craft anything, but it's full, so we clear it
		if (counter == ItemsSR.Length) {
			ClearTable();
		}

		return (null, false);
	}
	private void ClearTable() {
		foreach (SpriteRenderer sr in ItemsSR)
			sr.sprite = null;

		items.Clear();
		counter = 0;
	}

}
