using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * This script toggles the point cloud depth view from the Movo's kinect on and off when the 
 * Left Controller trackpad is clicked. It is intended that users will toggle off point cloud view
 * when driving the robot (because it can induce motion sickness to look at the point cloud and drive around) 
 * and toggle it on when attempting to use the hands to pick up or manipulate objects).
 * 
 * This script also makes the copies of the Movo's hands (that are attached to the user's controller) appear transparent
 * so as not to be confused with the Movo's actual hands in the scene
 */

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
