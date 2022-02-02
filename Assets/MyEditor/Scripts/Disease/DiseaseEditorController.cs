using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;

public class DiseaseEditorController : MonoBehaviour {

    public Dropdown Treatment;
    public InputField Name, Lifespan, Points;
    public Image SickFaceImage;
    public int sickFaceID;
    public Transform ScrollviewContent;

    private void Start() {
        FetchSickFaces();
        FetchTreatmentDDOptions();
    }

    private void FetchTreatmentDDOptions() {
        string[] treatments = Directory.GetFiles(Path.Combine(Application.dataPath, "MyEditor/Data/Treatment"));
        List<string> treatmentsList = new List<string>();
        foreach (string s in treatments) {
            if(!s.EndsWith(".meta"))
                treatmentsList.Add(Path.GetFileNameWithoutExtension(s));
        }
        Treatment.ClearOptions();
        Treatment.AddOptions(treatmentsList);
    }

    private void FetchSickFaces() {
        Sprite[] sickFaces = Resources.LoadAll<Sprite>("Illustrations/Perso/Faces");
        int counter = 0;
        for (int i = 0; i < sickFaces.Length - 16; i += 8) {
            GameObject newSprite = new GameObject(sickFaces[i].name, typeof(Button), typeof(Image));
            newSprite.transform.SetParent(ScrollviewContent);
            Sprite displayed = sickFaces[i];
            int faceID = counter;
            newSprite.GetComponent<Button>().onClick.AddListener(delegate { DisplaySprite(displayed, faceID); });
            newSprite.GetComponent<Image>().sprite = sickFaces[i];
            counter++;
        }
    }

    private void DisplaySprite(Sprite s, int faceID) {
        SickFaceImage.sprite = s;
        sickFaceID = faceID;
    }
}
