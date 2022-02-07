using UnityEngine;

public class CellController : MonoBehaviour {
	public int row { get; private set; }
	public int column { get; private set; }

	public void Setup(int i, int j) {
		row = i;
		column = j;
	}
}
