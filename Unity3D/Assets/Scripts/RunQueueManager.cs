using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RunQueueManager : MonoBehaviour {

    public Button VisualButton, ExecButton;

    // Use this for initialization
    void Start () {
        Button visualsButton = VisualButton.GetComponent<Button>();
        Button execButton = ExecButton.GetComponent<Button>();
        visualsButton.onClick.AddListener(VisualClicked);
        execButton.onClick.AddListener(ExecClicked);
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    void VisualClicked()
    {
        Debug.Log("visual");
    }

    void ExecClicked()
    {

    }
}
