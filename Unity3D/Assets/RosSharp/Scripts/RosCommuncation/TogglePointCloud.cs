using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TogglePointCloud : MonoBehaviour {

    public GameObject pointCloudView;
    public GameObject rightHand;
    public GameObject leftHand;
    private bool buttonPressed = false;
    private bool pointCloudToggled = false;


    // Use this for initialization
    void Start () {
        pointCloudView.SetActive(false);
        rightHand.GetComponent<Renderer>().material.color = new Color(0.0f, 0.0f, 0.0f, 0.5f);
        leftHand.GetComponent<Renderer>().material.color = new Color(0.0f, 0.0f, 0.0f, 0.5f);
        
	}
	
	// Update is called once per frame
	void Update () {
        if (Input.GetButton("Left_track_button")) {
            buttonPressed = true;
        }

        else if(!Input.GetButton("Left_track_button") && buttonPressed) {
            if (pointCloudToggled) {
                pointCloudView.SetActive(false);
                pointCloudToggled = false;
            }
            else {
                pointCloudView.SetActive(true);
                pointCloudToggled = true;
            }
            buttonPressed = false;
        }
		
	}
}
