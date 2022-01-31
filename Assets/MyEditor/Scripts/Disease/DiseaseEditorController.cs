using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;

public class DiseaseEditorController : MonoBehaviour {

    public Dropdown Treatment;
    public InputField Name, Lifespan, Points;
    public Image SickFaceImage;
    public Transform ScrollviewContent;

    private void Start() {
        FetchSickFaces();
        FetchTreatmentDDOptions();
    }

    private void FetchTreatmentDDOptions() {
        string[] treatments = Directory.GetFiles(Path.Combine(Application.dataPath, "MyEditor/Data/Treatment"));
        List<string> treatmentsList = new List<string>();
        foreach (string s in treatments) {
            treatmentsList.Add(Path.GetFileName(s));
        }
        Treatment.ClearOptions();
        Treatment.AddOptions(treatmentsList);
    }

    private void FetchSickFaces() {
        Sprite[] sickFaces = Resources.LoadAll<Sprite>("Illustrations/Perso/Faces");
        for(int i = 0; i < sickFaces.Length; i += 4) {
            GameObject newSprite = new GameObject(sickFaces[i].name, typeof(Button), typeof(Image));
            newSprite.transform.SetParent(ScrollviewContent);
            Sprite displayed = sickFaces[i];
            newSprite.GetComponent<Button>().onClick.AddListener(delegate { DisplaySprite(displayed); });
            newSprite.GetComponent<Image>().sprite = sickFaces[i];
        }
    }

    private void DisplaySprite(Sprite s) {
        SickFaceImage.sprite = s;
    }
}
