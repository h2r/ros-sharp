using RosSharp.RosBridgeClient;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

// This script publishes commands to the topic /ein/right/forth_commands
public class EinRightForthCommandsPublisher : Publisher {

    public GameObject leftController;
    public GameObject rightController;
    public GameObject Robot;

    private StandardString message;

    // The boolean deciding which arm is currently being controlled. False is right and True is left. 
    // Starts with right by default.
    private bool currArm = false;

    // Booleans to keep track of the previous state of the grip buttons
    private bool leftGripPressed = false;
    private bool rightGripPressed = false;

    private bool rightGripperClosed = true;
    private bool leftGripperClosed = true;

    // Use this for initialization
    protected override void Start() {
        rosSocket = GetComponent<RosConnector>().RosSocket;

        publicationId = rosSocket.Advertise(Topic, "std_msgs/String");
        message = new StandardString();
        InvokeRepeating("SendControls", .1f, .1f);
    }

    void SendControls() {
        //Convert the Unity position of the hand controller to a ROS position (scaled)
        Vector3 outLeftPos = UnityToRosPositionAxisConversion(leftController.transform.position - Robot.transform.position);
        Vector3 outRightPos = UnityToRosPositionAxisConversion(rightController.transform.position - Robot.transform.position);
        //Convert the Unity rotation of the hand controller to a ROS rotation (scaled, quaternions)
        Quaternion outLeftQuat = UnityToRosRotationAxisConversion(leftController.transform.rotation);
        Quaternion outRightQuat = UnityToRosRotationAxisConversion(rightController.transform.rotation);
        //construct the Ein message to be published
        message.data = "";
        //Allows movement control with controllers if menu is disabled
        String controllerPrefix = "";

        if(Input.GetAxis("Left_grip") > 0.5f && !leftGripPressed) {
            leftGripPressed = true;
        }
        //if deadman switch held in, move to new pose
        if (Input.GetAxis("Left_grip") < 0.5f && leftGripPressed) {
            //construct message to move to new pose for the robot end effector
            if (!currArm) {
                controllerPrefix = "switchToLeftArm \n";
                currArm = true;
            }            
            message.data = controllerPrefix + outLeftPos.x + " " + outLeftPos.y + " " + outLeftPos.z + " " +
            outLeftQuat.x + " " + outLeftQuat.y + " " + outLeftQuat.z + " " + outLeftQuat.w + " moveToEEPose";
            leftGripPressed = false;
            //if touchpad is pressed (Crane game), incrementally move in new direction
        }
        if (Input.GetAxis("Right_grip") > 0.5f && !rightGripPressed) {
            rightGripPressed = true;
        }
        if (Input.GetAxis("Right_grip") < 0.5f && rightGripPressed) {
            //construct message to move to new pose for the robot end effector
            if (currArm) {
                controllerPrefix = "switchToRightArm \n";
                currArm = false;
            }
            message.data = controllerPrefix + outRightPos.x + " " + outRightPos.y + " " + outRightPos.z + " " +
            outRightQuat.x + " " + outRightQuat.y + " " + outRightQuat.z + " " + outRightQuat.w + " moveToEEPose";
            rightGripPressed = false;
            //if touchpad is pressed (Crane game), incrementally move in new direction
        }

        //If trigger pressed, open the gripper. Else, close gripper
        if (Input.GetAxis("Left_trigger") > 0.5f && leftGripperClosed) {
            if (!currArm) {
                controllerPrefix = "\n" + "switchToLeftArm \n";
                currArm = true;
            }
            message.data += controllerPrefix + " openGripper ";
            leftGripperClosed = false;
        }
        else if (Input.GetAxis("Left_trigger") < 0.5f && !leftGripperClosed) {
            message.data += controllerPrefix + " closeGripper";
            leftGripperClosed = true;
        }

        if (Input.GetAxis("Right_trigger") > 0.5f && rightGripperClosed) {
            if (currArm) {
                controllerPrefix = "\n" + "switchToRightArm \n";
                currArm = false;
            }
            message.data += controllerPrefix + " openGripper";
            rightGripperClosed = false;
        }
        else if (Input.GetAxis("Right_trigger") < 0.5f && !rightGripperClosed) {
            message.data += controllerPrefix + " closeGripper ";
            rightGripperClosed = true;
        }

        //Send the message to the websocket client (i.e: publish message onto ROS network)
        Debug.Log(message.data);
        rosSocket.Publish(publicationId, message);
    }

    //Convert 3D Unity position to ROS position 
    Vector3 UnityToRosPositionAxisConversion(Vector3 rosIn) {
        return new Vector3(rosIn.z, -rosIn.x, rosIn.y);
    }

    //Convert 4D Unity quaternion to ROS quaternion
    Quaternion UnityToRosRotationAxisConversion(Quaternion qIn) {

        Quaternion temp = (new Quaternion(qIn.x, qIn.z, -qIn.y, qIn.w)) * (new Quaternion(0, 1, 0, 0));
        return temp;

        //return new Quaternion(-qIn.z, qIn.x, -qIn.w, -qIn.y);
        //return new Quaternion(-qIn.z, qIn.w, -qIn.x, -qIn.y);
        //return new Quaternion(-qIn.z, qIn.w, -qIn.x, -qIn.y);
        //return new Quaternion(-qIn.z, qIn.x, qIn.w, qIn.y);
    }


    //// Update is called once per frame
    //void Update() {

    //    if (Input.GetAxis("left grip button") > 0.8f) {
    //        prevLftGrpButtonState = false;
    //        currLftGrpButtonState = true;
    //    }
    //    if ((Input.GetAxis("left grip button") < 0.1f) && currLftGrpButtonState && !prevLftGrpButtonState) {
    //        currLftGrpButtonState = false;
    //        prevLftGrpButtonState = true;
    //    }
    //    if (!currLftGrpButtonState && prevLftGrpButtonState) {
    //        message.data = "dogSummon" + dogList[currDog] + "\n" + "dogGetSensoryMotorStatesInfinite";
    //        rosSocket.Publish(publicationId, message);
    //        Debug.Log(dogList[currDog] + " was summoned");
    //        prevLftGrpButtonState = false;
    //    }

    //    // Walking forward and back with right track pad
    //    if (Input.GetAxis("Right Vertical Pad") < -0.8f) {
    //        currRgtTrackUpState = true;
    //        prevRgtTrackUpState = false;
    //    }
    //    if (Math.Abs(Input.GetAxis("Right Vertical Pad")) <= 0.1f && currRgtTrackUpState && !prevRgtTrackUpState) {
    //        currRgtTrackUpState = false;
    //        prevRgtTrackUpState = true;
    //    }
    //    if (!currRgtTrackUpState && prevRgtTrackUpState && dogHeightList[currDog] == 2) {
    //        message.data = "2 dogWalkForwardSeconds";
    //        rosSocket.Publish(publicationId, message);
    //        Debug.Log("The dog was told to walk forwards!");
    //        prevRgtTrackUpState = false;
    //    }

    //    if (Input.GetAxis("Right Vertical Pad") > 0.8f) {
    //        currRgtTrackDownState = true;
    //        prevRgtTrackDownState = false;
    //    }
    //    if (Math.Abs(Input.GetAxis("Right Vertical Pad")) <= 0.1f && currRgtTrackDownState && !prevRgtTrackDownState) {
    //        currRgtTrackDownState = false;
    //        prevRgtTrackDownState = true;
    //    }
    //    if (!currRgtTrackDownState && prevRgtTrackDownState && dogHeightList[currDog] == 2) {
    //        message.data = "2 dogWalkBackwardSeconds";
    //        rosSocket.Publish(publicationId, message);
    //        Debug.Log("The dog was told to walk backwards!");
    //        prevRgtTrackDownState = false;
    //    }

    //    // Turning right and left with right track pad
    //    if (Input.GetAxis("Right Horizontal Pad") > 0.8f) {
    //        Debug.Log("The right button was pressed");
    //        currRgtTrackRightState = true;
    //        prevRgtTrackRightState = false;
    //    }
    //    if (Math.Abs(Input.GetAxis("Right Horizontal Pad")) <= 0.1f && currRgtTrackRightState && !prevRgtTrackRightState) {
    //        currRgtTrackRightState = false;
    //        prevRgtTrackRightState = true;
    //    }
    //    if (!currRgtTrackRightState && prevRgtTrackRightState && dogHeightList[currDog] == 2) {
    //        message.data = "2 dogTurnRightSeconds";
    //        rosSocket.Publish(publicationId, message);
    //        Debug.Log("The dog was told to turn right!");
    //        prevRgtTrackRightState = false;
    //    }

    //    if (Input.GetAxis("Right Horizontal Pad") < -0.8f) {
    //        currRgtTrackLeftState = true;
    //        prevRgtTrackLeftState = false;
    //    }
    //    if (Math.Abs(Input.GetAxis("Right Horizontal Pad")) <= 0.1f && currRgtTrackLeftState && !prevRgtTrackLeftState) {
    //        currRgtTrackLeftState = false;
    //        prevRgtTrackLeftState = true;
    //    }
    //    if (!currRgtTrackLeftState && prevRgtTrackLeftState && dogHeightList[currDog] == 2) {
    //        message.data = "2 dogTurnLeftSeconds";
    //        rosSocket.Publish(publicationId, message);
    //        Debug.Log("The dog was told to turn left!");
    //        prevRgtTrackLeftState = false;
    //    }

    //    // Switching between dogs with the left controller's horizontal pad

    //    if (Input.GetAxis("Left Horizontal Pad") > 0.8f) {
    //        //Debug.Log("Left pad swiped");
    //        currLftTrackLeftState = true;
    //        prevLftTrackLeftState = false;
    //    }
    //    if (Math.Abs(Input.GetAxis("Left Horizontal Pad")) <= 0.1f && currLftTrackLeftState && !prevLftTrackLeftState) {
    //        currLftTrackLeftState = false;
    //        prevLftTrackLeftState = true;
    //    }
    //    if (!currLftTrackLeftState && prevLftTrackLeftState) {
    //        currDog += 1;
    //        if (currDog > totalDogs) {
    //            currDog = 0;
    //        }
    //        prevLftTrackLeftState = false;
    //        Debug.Log("The current dog is " + dogList[currDog]);
    //    }

    //    if (Input.GetAxis("Left Horizontal Pad") < -0.8f) {
    //        currLftTrackRightState = true;
    //        prevLftTrackRightState = false;
    //    }
    //    if (Math.Abs(Input.GetAxis("Left Horizontal Pad")) <= 0.1f && currLftTrackRightState && !prevLftTrackRightState) {
    //        currLftTrackRightState = false;
    //        prevLftTrackRightState = true;
    //    }
    //    if (!currLftTrackRightState && prevLftTrackRightState) {
    //        currDog -= 1;
    //        if (currDog < 0) {
    //            currDog = totalDogs;
    //        }
    //        prevLftTrackRightState = false;
    //        Debug.Log("The current dog is " + dogList[currDog]);
    //    }

    //    // Sitting and standing up for the dog
    //    if ((Input.GetAxis("Right Trigger") > 0.5) && !rightTriggerPressed) {
    //        currRightControlHeight = rightController.transform.position[1];
    //        prevRightControlHeight = rightController.transform.position[1];
    //        rightTriggerPressed = true;
    //        Debug.Log(currRightControlHeight);
    //    }
    //    if ((Input.GetAxis("Right Trigger") < 0.5) && rightTriggerPressed) {
    //        currRightControlHeight = rightController.transform.position[1];
    //        Debug.Log(currRightControlHeight);
    //        if (currRightControlHeight - prevRightControlHeight > 0.3f) {
    //            int currDogHeight = dogHeightList[currDog];
    //            if (currDogHeight == 0) {
    //                message.data = "demoDogSit";
    //                dogHeightList[currDog] = 1;
    //            }
    //            else if (currDogHeight == 1) {
    //                message.data = "demoDogStandUp";
    //                dogHeightList[currDog] = 2;
    //            }
    //            else {
    //                message.data = "";
    //            }
    //            rosSocket.Publish(publicationId, message);
    //            Debug.Log("The dog was told to" + message.data);
    //            prevRightControlHeight = currRightControlHeight;
    //        }
    //        else if (currRightControlHeight - prevRightControlHeight < -0.3f) {
    //            int currDogHeight = dogHeightList[currDog];
    //            if (currDogHeight == 2) {
    //                message.data = "demoDogSit";
    //                dogHeightList[currDog] = 1;
    //            }
    //            else if (currDogHeight == 1) {
    //                message.data = "demoDogLieDown";
    //                dogHeightList[currDog] = 0;
    //            }
    //            else {
    //                message.data = "";
    //            }
    //            rosSocket.Publish(publicationId, message);
    //            Debug.Log("The dog was told to" + message.data);
    //            prevRightControlHeight = currRightControlHeight;
    //        }
    //        else if (Math.Abs(currRightControlHeight - prevRightControlHeight) < 0.3) {
    //            prevRightControlHeight = currRightControlHeight;
    //        }
    //        rightTriggerPressed = false;

    //    }

    //    if (Input.GetAxis("Left Trigger") > 0.5f) {
    //        //Debug.Log("Left trigger");
    //        currLftTriggerState = true;
    //        prevLftTriggerState = false;
    //    }
    //    if (Math.Abs(Input.GetAxis("Left Trigger")) <= 0.5f && currLftTriggerState && !prevLftTriggerState) {
    //        currLftTriggerState = false;
    //        prevLftTriggerState = true;
    //    }
    //    if (!currLftTriggerState && prevLftTriggerState) {
    //        message.data = "demoDogWagYourTail";
    //        rosSocket.Publish(publicationId, message);
    //        Debug.Log("The dog was told to wag its tail");
    //        prevLftTriggerState = false;
    //    }

    //    if (Input.GetAxis("Left Track Press") > 0.5) {
    //        Debug.Log("Wakanda!");
    //    }
    //}
}