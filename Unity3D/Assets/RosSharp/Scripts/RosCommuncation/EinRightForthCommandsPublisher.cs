using RosSharp.RosBridgeClient;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class EinRightForthCommandsPublisher : Publisher {

    public string Topic = "ein/right/forth_commands";
    private StandardString message;
    private bool currLftGrpButtonState = false;
    private bool prevLftGrpButtonState = false;
    private bool currRgtTrackUpState = false;
    private bool prevRgtTrackUpState = false;
    private bool currRgtTrackDownState = false;
    private bool prevRgtTrackDownState = false;
    private bool currRgtTrackCenterState = false;

    // Use this for initialization
    void Start () {
        rosSocket = GetComponent<RosConnector>().RosSocket;

        // Get the controller component of this gameobject

        publicationId = rosSocket.Advertise("ein/right/forth_commands", "std_msgs/String");
        message = new StandardString();
    }
	
	// Update is called once per frame
	void Update () {
        // Summoning via left grip button
        if (Input.GetAxis("left grip button") > 0.5f) {
            prevLftGrpButtonState = false;
            currLftGrpButtonState = true;
        }
        if ((Input.GetAxis("left grip button") < 0.5f) && currLftGrpButtonState && !prevLftGrpButtonState) {
            currLftGrpButtonState = false;
            prevLftGrpButtonState = true;
        }
        if(!currLftGrpButtonState && prevLftGrpButtonState) {
            message.data = "dogSummonSpot";
            rosSocket.Publish(publicationId, message);
            Debug.Log("Spot was summoned");
            prevLftGrpButtonState = false;
        }

        // Walking forward and back with right track pad
        if(Input.GetAxis("Right Vertical Pad") > 0.15f) {
            currRgtTrackUpState = true;
            prevRgtTrackUpState = false;
        }
        if(Math.Abs(Input.GetAxis("Right Vertical Pad")) <= 0.5f && currRgtTrackUpState && !prevRgtTrackUpState) {
            currRgtTrackUpState = false;
            prevRgtTrackUpState = true;
        }
        if(!currRgtTrackUpState && prevRgtTrackUpState) {
            message.data = "2 dogWalkForwardSeconds";
            rosSocket.Publish(publicationId, message);
            Debug.Log("The dog was told to walk forward!");
            prevRgtTrackUpState = false;
        }

        if (Input.GetAxis("Right Vertical Pad") < -0.15f) {
            currRgtTrackDownState = true;
            prevRgtTrackDownState = false;
        }
        if (Math.Abs(Input.GetAxis("Right Vertical Pad")) <= 0.5f && currRgtTrackDownState && !prevRgtTrackDownState) {
            currRgtTrackDownState = false;
            prevRgtTrackDownState = true;
        }
        if (!currRgtTrackDownState && prevRgtTrackDownState) {
            message.data = "2 dogWalkBackwardSeconds";
            //rosSocket.Publish(publicationId, message);
            Debug.Log("The dog was told to walk backward!");
            prevRgtTrackDownState = false;
        }
    }
}
