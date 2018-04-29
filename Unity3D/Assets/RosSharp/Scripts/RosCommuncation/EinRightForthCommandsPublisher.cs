using RosSharp.RosBridgeClient;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class EinRightForthCommandsPublisher : Publisher {

    public string Topic = "ein/right/forth_commands";
    private StandardString message;

    //@TODO: Use this list to summon different dogs with the left controller and thus switch
    private static string[] dogList = { "Spot", "Crystal", "Peanut", "Pluto"};
    private int currDog = 0;
    private static int totalDogs = dogList.Length - 1;

    private bool currLftGrpButtonState = false;
    private bool prevLftGrpButtonState = false;
    private bool currLftTrackLeftState = false;
    private bool prevLftTrackLeftState = false;
    private bool currLftTrackRightState = false;
    private bool prevLftTrackRightState = false;


    private bool currRgtTrackUpState = false;
    private bool prevRgtTrackUpState = false;
    private bool currRgtTrackDownState = false;
    private bool prevRgtTrackDownState = false;
    private bool currRgtTrackRightState = false;
    private bool prevRgtTrackRightState = false;
    private bool currRgtTrackLeftState = false;
    private bool prevRgtTrackLeftState = false;


    // Use this for initialization
    void Start () {
        rosSocket = GetComponent<RosConnector>().RosSocket;

        publicationId = rosSocket.Advertise("ein/right/forth_commands", "std_msgs/String");
        message = new StandardString();
    }
	
	// Update is called once per frame
	void Update () {
        
        if (Input.GetAxis("left grip button") > 0.8f) {
            prevLftGrpButtonState = false;
            currLftGrpButtonState = true;
        }
        if ((Input.GetAxis("left grip button") < 0.1f) && currLftGrpButtonState && !prevLftGrpButtonState) {
            currLftGrpButtonState = false;
            prevLftGrpButtonState = true;
        }
        if(!currLftGrpButtonState && prevLftGrpButtonState) {
            message.data = "dogSummon" + dogList[currDog];
            rosSocket.Publish(publicationId, message);
            Debug.Log(dogList[currDog] + " was summoned");
            prevLftGrpButtonState = false;
        }

        // Walking forward and back with right track pad
        if(Input.GetAxis("Right Vertical Pad") < -0.8f) {
            currRgtTrackUpState = true;
            prevRgtTrackUpState = false;
        }
        if(Math.Abs(Input.GetAxis("Right Vertical Pad")) <= 0.1f && currRgtTrackUpState && !prevRgtTrackUpState) {
            currRgtTrackUpState = false;
            prevRgtTrackUpState = true;
        }
        if(!currRgtTrackUpState && prevRgtTrackUpState) {
            message.data = "2 dogWalkForwardSeconds";
            rosSocket.Publish(publicationId, message);
            Debug.Log("The dog was told to walk forwards!");
            prevRgtTrackUpState = false;
        }

        if (Input.GetAxis("Right Vertical Pad") > 0.8f) {
            currRgtTrackDownState = true;
            prevRgtTrackDownState = false;
        }
        if (Math.Abs(Input.GetAxis("Right Vertical Pad")) <= 0.1f && currRgtTrackDownState && !prevRgtTrackDownState) {
            currRgtTrackDownState = false;
            prevRgtTrackDownState = true;
        }
        if (!currRgtTrackDownState && prevRgtTrackDownState) {
            message.data = "2 dogWalkBackwardSeconds";
            rosSocket.Publish(publicationId, message);
            Debug.Log("The dog was told to walk backwards!");
            prevRgtTrackDownState = false;
        }

        // Turning right and left with right track pad
        if (Input.GetAxis("Right Horizontal Pad") < -0.8f) {
            Debug.Log("The right button was pressed");
            currRgtTrackRightState = true;
            prevRgtTrackRightState = false;
        }
        if (Math.Abs(Input.GetAxis("Right Horizontal Pad")) <= 0.1f && currRgtTrackRightState && !prevRgtTrackRightState) {
            currRgtTrackRightState = false;
            prevRgtTrackRightState = true;
        }
        if (!currRgtTrackRightState && prevRgtTrackRightState) {
            message.data = "2 dogTurnRightSeconds";
            rosSocket.Publish(publicationId, message);
            Debug.Log("The dog was told to turn right!");
            prevRgtTrackRightState = false;
        }

        if (Input.GetAxis("Right Horizontal Pad") > 0.8f) {
            currRgtTrackLeftState = true;
            prevRgtTrackLeftState = false;
        }
        if (Math.Abs(Input.GetAxis("Right Horizontal Pad")) <= 0.1f && currRgtTrackLeftState && !prevRgtTrackLeftState) {
            currRgtTrackLeftState = false;
            prevRgtTrackLeftState = true;
        }
        if (!currRgtTrackLeftState && prevRgtTrackLeftState) {
            message.data = "2 dogTurnLeftSeconds";
            rosSocket.Publish(publicationId, message);
            Debug.Log("The dog was told to turn left!");
            prevRgtTrackLeftState = false;
        }

        if (Input.GetAxis("Left Horizontal Pad") > 0.8f) {
            currLftTrackLeftState = true;
            prevLftTrackLeftState = false;
        }
        if (Math.Abs(Input.GetAxis("Left Horizontal Pad")) <= 0.1f && currLftTrackLeftState && !prevLftTrackLeftState) {
            currLftTrackLeftState = false;
            prevLftTrackLeftState = true;
        }
        if (!currLftTrackLeftState && prevLftTrackLeftState) {
            currDog += 1;
            if(currDog > totalDogs) {
                currDog = 0;
            }
            prevLftTrackLeftState = false;
            Debug.Log("The current dog is " + dogList[currDog]);

        }

    }
}
