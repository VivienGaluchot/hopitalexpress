using UnityEngine;
using UnityEngine.UI;
using System;

public class LevelCameraController : MonoBehaviour {
    public static LevelCameraController instance;

	[SerializeField] private InputField camX;
	[SerializeField] private InputField camY;
    [SerializeField] private InputField camSize;
    [SerializeField] private Image fixButton;


    private Camera cam;
    private bool isFixed;

    private void Awake() {
        instance = this;
		cam = Camera.main;
    }

    private void Update() {
        if(!isFixed) {
            camX.text = cam.transform.position.x.ToString();
            camY.text = cam.transform.position.y.ToString();
            camSize.text = cam.orthographicSize.ToString();
        }
    }

    public void SetCamParams(float X, float Y, float size) {
        cam.transform.position = new Vector3(X, Y, -10f);
        cam.orthographicSize = size;

        isFixed = true;
        fixButton.color = Color.green;
    }

    public (float, float, float) GetCamParams() {
        float x = Single.Parse(camX.text), y = Single.Parse(camY.text), size = Single.Parse(camSize.text);
        return (x, y, size);
    }

    public void FixCamParams() {
        isFixed = !isFixed;
        fixButton.color = isFixed ? Color.green : Color.white;
    }
}
