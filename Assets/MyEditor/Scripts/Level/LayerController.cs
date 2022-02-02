using UnityEngine;
using UnityEngine.UI;

public class LayerController : MonoBehaviour {

    [SerializeField] private Text LayerText;
    private LevelEditorController lec;

    public int currentLayer { get; private set; }

    private void Start() {
        lec = GetComponent<LevelEditorController>();
        currentLayer = 0;
        LayerText.text = currentLayer.ToString();
    }

    public void TryUpLayer() {
        if(currentLayer < lec.grids.Length - 1) {
            currentLayer++;
            LayerText.text = currentLayer.ToString();
        }
    }

    public void TryDownLayer() {
        if(currentLayer > 0) {
            currentLayer--;
            LayerText.text = currentLayer.ToString();
        }
    }
}
