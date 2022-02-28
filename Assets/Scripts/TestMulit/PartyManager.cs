using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;

public class PartyManager : MonoBehaviour
{
    public GameObject hostButton;
    public GameObject hostIdText;
    public GameObject joinButton;
    public GameObject joinInput;

    void Start() {
        hostButton.GetComponent<Button>().onClick.AddListener(() => {
            NetworkManager.Singleton.StartHost();
            hostIdText.GetComponent<Text>().text = "#none";
            hostButton.GetComponent<Button>().interactable = false;
            joinButton.GetComponent<Button>().interactable = false;
            joinInput.GetComponent<InputField>().interactable = false;
        });

        joinButton.GetComponent<Button>().onClick.AddListener(() => {
            NetworkManager.Singleton.StartClient();
            hostButton.GetComponent<Button>().interactable = false;
            joinButton.GetComponent<Button>().interactable = false;
            joinInput.GetComponent<InputField>().interactable = false;
        });
    }

    void Update() {
        
    }
}
