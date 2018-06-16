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

    // Booleans to keep track of state of trigger
    private bool rightGripperClosed = true;
    private bool leftGripperClosed = true;

    // Variables to keep track the Right Trackpad.
    private bool rightTrackpadUpPressed = false;
    private int timeStepsRightUpPressed = 0;
    private bool rightTrackpadDownPressed = false;
    private int timeStepsRightDownPressed = 0;

    private bool rightTrackpadRightPressed = false;
    private int timeStepsRightRightPressed = 0;
    private bool rightTrackpadLeftPressed = false;
    private int timeStepsRightLeftPressed = 0;

    // Variables to keep track of the Left Trackpad.
    private bool leftTrackpadUpPressed = false;
    private int timeStepsLeftUpPressed = 0;
    private bool leftTrackpadDownPressed = false;
    private int timeStepsLeftDownPressed = 0;

    private bool leftTrackpadRightPressed = false;
    private int timeStepsLeftRightPressed = 0;
    private bool leftTrackpadLeftPressed = false;
    private int timeStepsLeftLeftPressed = 0;

    // Use this for initialization
    protected override void Start() {
        rosSocket = GetComponent<RosConnector>().RosSocket;

        publicationId = rosSocket.Advertise(Topic, "std_msgs/String");
        message = new StandardString();
        message.data = "baseGoCfg";
        rosSocket.Publish(publicationId, message);
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

        if (Input.GetAxis("Right_trackpad_vertical") > 0.8) {
            if (!rightTrackpadUpPressed) {
                message.data = "-0.2 baseSendXVel";
                rightTrackpadUpPressed = true;
            }
            else {
                if (timeStepsRightUpPressed == 4) {
                    message.data = "-0.2 baseSendXVel";
                    timeStepsRightUpPressed = 0;
                }
                else {
                    timeStepsRightUpPressed += 1;
                }
            }
        }
        else if (Input.GetAxis("Right_trackpad_vertical") < -0.8) {
            if (!rightTrackpadDownPressed) {
                message.data = "0.2 baseSendXVel";
                rightTrackpadDownPressed = true;
            }
            else {
                if (timeStepsRightDownPressed == 4) {
                    message.data = "0.2 baseSendXVel";
                    timeStepsRightDownPressed = 0;
                }
                else {
                    timeStepsRightDownPressed += 1;
                }
            }
        }
        else {
            rightTrackpadUpPressed = false;
            rightTrackpadDownPressed = false;
            timeStepsRightUpPressed = 0;
            timeStepsRightDownPressed = 0;
        }

        if (Input.GetAxis("Right_trackpad_horizontal") > 0.8) {
            if (!rightTrackpadRightPressed) {
                message.data = "-0.2 baseSendOZVel";
                rightTrackpadRightPressed = true;
            }
            else {
                if (timeStepsRightRightPressed == 4) {
                    message.data = "-0.2 baseSendOZVel";
                    timeStepsRightRightPressed = 0;
                }
                else {
                    timeStepsRightRightPressed += 1;
                }
            }
        }
        else if (Input.GetAxis("Right_trackpad_horizontal") < -0.8) {
            if (!rightTrackpadLeftPressed) {
                message.data = "0.2 baseSendOZVel";
                rightTrackpadLeftPressed = true;
            }
            else {
                if (timeStepsRightLeftPressed == 4) {
                    message.data = "0.2 baseSendOZVel";
                    timeStepsRightLeftPressed = 0;
                }
                else {
                    timeStepsRightLeftPressed += 1;
                }
            }
        }
        else {
            rightTrackpadRightPressed = false;
            rightTrackpadLeftPressed = false;
            timeStepsRightRightPressed = 0;
            timeStepsRightLeftPressed = 0;
        }



        if (Input.GetAxis("Left_trackpad_vertical") > 0.8) {
            Debug.Log("Left Up button pressed");
            if (!leftTrackpadUpPressed) {
                message.data += "\n tiltUp";
                leftTrackpadUpPressed = true;
            }
            else {
                if (timeStepsLeftUpPressed == 4) {
                    message.data += "\n tiltUp";
                    timeStepsLeftUpPressed = 0;
                }
                else {
                    timeStepsLeftUpPressed += 1;
                }
            }
        }
        else if (Input.GetAxis("Left_trackpad_vertical") < -0.8) {
            if (!leftTrackpadDownPressed) {
                message.data += "\n tiltDown";
                leftTrackpadDownPressed = true;
            }
            else {
                if (timeStepsLeftDownPressed == 4) {
                    message.data += "\n tiltDown";
                    timeStepsLeftDownPressed = 0;
                }
                else {
                    timeStepsLeftDownPressed += 1;
                }
            }
        }
        else {
            leftTrackpadUpPressed = false;
            leftTrackpadDownPressed = false;
            timeStepsLeftUpPressed = 0;
            timeStepsLeftDownPressed = 0;
        }

        if (Input.GetAxis("Left_trackpad_horizontal") > 0.8) {
            if (!leftTrackpadRightPressed) {
                message.data += "\n panUp";
                leftTrackpadRightPressed = true;
            }
            else {
                if (timeStepsLeftRightPressed == 4) {
                    message.data += "\n panUp";
                    timeStepsLeftRightPressed = 0;
                }
                else {
                    timeStepsLeftRightPressed += 1;
                }
            }
        }
        else if (Input.GetAxis("Left_trackpad_horizontal") < -0.8) {
            if (!leftTrackpadLeftPressed) {
                message.data += "\n panDown";
                leftTrackpadLeftPressed = true;
            }
            else {
                if (timeStepsLeftLeftPressed == 4) {
                    message.data += "\n panDown";
                    timeStepsLeftLeftPressed = 0;
                }
                else {
                    timeStepsLeftLeftPressed += 1;
                }
            }
        }
        else {
            rightTrackpadRightPressed = false;
            rightTrackpadLeftPressed = false;
            timeStepsRightRightPressed = 0;
            timeStepsRightLeftPressed = 0;
        }


        if (Input.GetAxis("Left_grip") > 0.5f && !leftGripPressed) {
            leftGripPressed = true;
        }
        //if deadman switch held in, move to new pose
        if (Input.GetAxis("Left_grip") < 0.5f && leftGripPressed) {
            //construct message to move to new pose for the robot end effector
            if (!currArm) {
                controllerPrefix = "switchToLeftArm \n";
                currArm = true;
            }            
            message.data += "\n" + controllerPrefix + outLeftPos.x + " " + outLeftPos.y + " " + outLeftPos.z + " " +
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
            message.data += "\n" + controllerPrefix + outRightPos.x + " " + outRightPos.y + " " + outRightPos.z + " " +
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

        
        if (message.data != "") {
            //Send the message to the websocket client (i.e: publish message onto ROS network)
            //Debug.Log(message.data);
            rosSocket.Publish(publicationId, message);
        }
    }

    //Convert 3D Unity position to ROS position 
    Vector3 UnityToRosPositionAxisConversion(Vector3 rosIn) {
        return new Vector3(rosIn.z, -rosIn.x, rosIn.y);
    }

    //Convert 4D Unity quaternion to ROS quaternion
    Quaternion UnityToRosRotationAxisConversion(Quaternion qIn) {

        Quaternion temp = (new Quaternion(-qIn.x, -qIn.y, -qIn.z, -qIn.w));
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