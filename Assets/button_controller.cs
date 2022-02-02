using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class button_controller : MonoBehaviour
{
    // Start is called before the first frame update
    private Button mybutton;
    void Start()
    {
        GetComponent<Button>().Select();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
