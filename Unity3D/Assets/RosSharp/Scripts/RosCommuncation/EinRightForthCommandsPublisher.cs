using RosSharp.RosBridgeClient;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class EinRightForthCommandsPublisher : Publisher {

    public GameObject leftController;
    public GameObject rightController;

    // 0 means lying down, 1 means sitting down, 2 means standing up
    // Originally, we assume all dogs start in the standing position
    private static int[] dogHeightList = { 2, 2, 2, 2 };

    private float currRightControlHeight = 0f;
    private float prevRightControlHeight = 0f;
    private bool rightTriggerPressed = false;

    private StandardString message;

    private static string[] dogList = { "Spot", "Crystal", "Peanut", "Pluto"};
    private int currDog = 0;
    private static int totalDogs = dogList.Length - 1;

    private bool currLftGrpButtonState = false;
    private bool prevLftGrpButtonState = false;
    private bool currLftTriggerState = false;
    private bool prevLftTriggerState = false;
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
    protected override void Start ()  {
        //base.Start();

        rosSocket = GetComponent<RosConnector>().RosSocket;

        publicationId = rosSocket.Advertise(Topic, "std_msgs/String");
        message = new StandardString();
    }

    // Update is called once per frame
    void Update() {

        if (Input.GetAxis("left grip button") > 0.8f) {
            prevLftGrpButtonState = false;
            currLftGrpButtonState = true;
        }
        if ((Input.GetAxis("left grip button") < 0.1f) && currLftGrpButtonState && !prevLftGrpButtonState) {
            currLftGrpButtonState = false;
            prevLftGrpButtonState = true;
        }
        if (!currLftGrpButtonState && prevLftGrpButtonState) {
            message.data = "dogSummon" + dogList[currDog] + "\n" + "dogGetSensoryMotorStatesInfinite";
            rosSocket.Publish(publicationId, message);
            Debug.Log(dogList[currDog] + " was summoned");
            prevLftGrpButtonState = false;
        }

        // Walking forward and back with right track pad
        if (Input.GetAxis("Right Vertical Pad") < -0.8f) {
            currRgtTrackUpState = true;
            prevRgtTrackUpState = false;
        }
        if (Math.Abs(Input.GetAxis("Right Vertical Pad")) <= 0.1f && currRgtTrackUpState && !prevRgtTrackUpState) {
            currRgtTrackUpState = false;
            prevRgtTrackUpState = true;
        }
        if (!currRgtTrackUpState && prevRgtTrackUpState && dogHeightList[currDog] == 2) {
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
        if (!currRgtTrackDownState && prevRgtTrackDownState && dogHeightList[currDog] == 2) {
            message.data = "2 dogWalkBackwardSeconds";
            rosSocket.Publish(publicationId, message);
            Debug.Log("The dog was told to walk backwards!");
            prevRgtTrackDownState = false;
        }

        // Turning right and left with right track pad
        if (Input.GetAxis("Right Horizontal Pad") > 0.8f) {
            Debug.Log("The right button was pressed");
            currRgtTrackRightState = true;
            prevRgtTrackRightState = false;
        }
        if (Math.Abs(Input.GetAxis("Right Horizontal Pad")) <= 0.1f && currRgtTrackRightState && !prevRgtTrackRightState) {
            currRgtTrackRightState = false;
            prevRgtTrackRightState = true;
        }
        if (!currRgtTrackRightState && prevRgtTrackRightState && dogHeightList[currDog] == 2) {
            message.data = "2 dogTurnRightSeconds";
            rosSocket.Publish(publicationId, message);
            Debug.Log("The dog was told to turn right!");
            prevRgtTrackRightState = false;
        }

        if (Input.GetAxis("Right Horizontal Pad") < -0.8f) {
            currRgtTrackLeftState = true;
            prevRgtTrackLeftState = false;
        }
        if (Math.Abs(Input.GetAxis("Right Horizontal Pad")) <= 0.1f && currRgtTrackLeftState && !prevRgtTrackLeftState) {
            currRgtTrackLeftState = false;
            prevRgtTrackLeftState = true;
        }
        if (!currRgtTrackLeftState && prevRgtTrackLeftState && dogHeightList[currDog] == 2) {
            message.data = "2 dogTurnLeftSeconds";
            rosSocket.Publish(publicationId, message);
            Debug.Log("The dog was told to turn left!");
            prevRgtTrackLeftState = false;
        }

        // Switching between dogs with the left controller's horizontal pad

        if (Input.GetAxis("Left Horizontal Pad") > 0.8f) {
            //Debug.Log("Left pad swiped");
            currLftTrackLeftState = true;
            prevLftTrackLeftState = false;
        }
        if (Math.Abs(Input.GetAxis("Left Horizontal Pad")) <= 0.1f && currLftTrackLeftState && !prevLftTrackLeftState) {
            currLftTrackLeftState = false;
            prevLftTrackLeftState = true;
        }
        if (!currLftTrackLeftState && prevLftTrackLeftState) {
            currDog += 1;
            if (currDog > totalDogs) {
                currDog = 0;
            }
            prevLftTrackLeftState = false;
            Debug.Log("The current dog is " + dogList[currDog]);
        }

        if (Input.GetAxis("Left Horizontal Pad") < -0.8f) {
            currLftTrackRightState = true;
            prevLftTrackRightState = false;
        }
        if (Math.Abs(Input.GetAxis("Left Horizontal Pad")) <= 0.1f && currLftTrackRightState && !prevLftTrackRightState) {
            currLftTrackRightState = false;
            prevLftTrackRightState = true;
        }
        if (!currLftTrackRightState && prevLftTrackRightState) {
            currDog -= 1;
            if (currDog < 0) {
                currDog = totalDogs;
            }
            prevLftTrackRightState = false;
            Debug.Log("The current dog is " + dogList[currDog]);
        }

        // Sitting and standing up for the dog
        if ((Input.GetAxis("Right Trigger") > 0.5) && !rightTriggerPressed) {
            currRightControlHeight = rightController.transform.position[1];
            prevRightControlHeight = rightController.transform.position[1];
            rightTriggerPressed = true;
            Debug.Log(currRightControlHeight);
        }
        if ((Input.GetAxis("Right Trigger") < 0.5) && rightTriggerPressed) {
            currRightControlHeight = rightController.transform.position[1];
            Debug.Log(currRightControlHeight);
            if (currRightControlHeight - prevRightControlHeight > 0.3f) {
                int currDogHeight = dogHeightList[currDog];
                if (currDogHeight == 0) {
                    message.data = "demoDogSit";
                    dogHeightList[currDog] = 1;
                }
                else if (currDogHeight == 1) {
                    message.data = "demoDogStandUp";
                    dogHeightList[currDog] = 2;
                }
                else {
                    message.data = "";
                }
                rosSocket.Publish(publicationId, message);
                Debug.Log("The dog was told to" + message.data);
                prevRightControlHeight = currRightControlHeight;
            }
            else if (currRightControlHeight - prevRightControlHeight < -0.3f) {
                int currDogHeight = dogHeightList[currDog];
                if (currDogHeight == 2) {
                    message.data = "demoDogSit";
                    dogHeightList[currDog] = 1;
                }
                else if (currDogHeight == 1) {
                    message.data = "demoDogLieDown";
                    dogHeightList[currDog] = 0;
                }
                else {
                    message.data = "";
                }
                rosSocket.Publish(publicationId, message);
                Debug.Log("The dog was told to" + message.data);
                prevRightControlHeight = currRightControlHeight;
            }
            else if (Math.Abs(currRightControlHeight - prevRightControlHeight) < 0.3) {
                prevRightControlHeight = currRightControlHeight;
            }
            rightTriggerPressed = false;

        }

        if (Input.GetAxis("Left Trigger") > 0.5f)
        {
            //Debug.Log("Left trigger");
            currLftTriggerState = true;
            prevLftTriggerState = false;
        }
        if (Math.Abs(Input.GetAxis("Left Trigger")) <= 0.5f && currLftTriggerState && !prevLftTriggerState)
        {
            currLftTriggerState = false;
            prevLftTriggerState = true;
        }
        if (!currLftTriggerState && prevLftTriggerState)
        {
            message.data = "demoDogWagYourTail";
            rosSocket.Publish(publicationId, message);
            Debug.Log("The dog was told to wag its tail");
            prevLftTriggerState = false;
        }

        if (Input.GetAxis("Left Track Press") > 0.5)
        {
            Debug.Log("Wakanda!");
        }
    }
}
