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
    private bool firstTime = true;

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

    // Variables to track previous state of menu buttons
    private bool leftMenuButtonPressed = false;
    private int timeStepsLeftMenuPressed = 0;
    private bool rightMenuButtonPressed = false;
    private int timeStepsRightMenuPressed = 0;

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

        // Driving forward and backwards code
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

        // Turning robot base code
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

        // Tilting code
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

        // Panning code
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
            leftTrackpadRightPressed = false;
            leftTrackpadLeftPressed = false;
            timeStepsLeftRightPressed = 0;
            timeStepsLeftLeftPressed = 0;
        }

        // Torso moving up and down code
        if (Input.GetButton("Left_menu_button")) {
            if (!leftMenuButtonPressed) {
                message.data += "\n torsoDown";
                leftMenuButtonPressed = true;
            }
            else {
                if (timeStepsLeftMenuPressed == 1) {
                    message.data += "\n torsoDown";
                    timeStepsLeftMenuPressed = 0;
                }
                else {
                    timeStepsLeftMenuPressed += 1;
                }
            }
        }
        else if (Input.GetButton("Right_menu_button")) {
            if (!rightMenuButtonPressed) {
                message.data += "\n torsoUp";
                rightMenuButtonPressed = true;
            }
            else {
                if (timeStepsRightMenuPressed == 1) {
                    message.data += "\n torsoUp";
                    timeStepsRightMenuPressed = 0;
                }
                else {
                    timeStepsRightMenuPressed += 1;
                }
            }
        }
        else {
            leftMenuButtonPressed = false;
            rightMenuButtonPressed = false;
            timeStepsLeftMenuPressed = 0;
            timeStepsRightMenuPressed = 0;
        }


        // Arms moving to a position code

        if (Input.GetAxis("Left_grip") > 0.5f && !leftGripPressed) {
            leftGripPressed = true;
        }
        //if deadman switch held in, move to new pose
        if (Input.GetAxis("Left_grip") < 0.5f && leftGripPressed) {
            //construct message to move to new pose for the robot end effector
            if (firstTime) {
                controllerPrefix = "switchToLeftArm \n";
                firstTime = false;
            }
            else if (!currArm) {
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
            if (firstTime) {
                controllerPrefix = "switchToRightArm \n";
                firstTime = false;
            }
            else if (currArm) {
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
            if (firstTime) {
                controllerPrefix = "switchToLeftArm \n";
                firstTime = false;
            }
            else if (!currArm) {
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
            if (firstTime) {
                controllerPrefix = "switchToRightArm \n";
                firstTime = false;
            }
            else if (currArm) {
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
            Debug.Log(message.data);
            rosSocket.Publish(publicationId, message);
        }
    }

    //Convert 3D Unity position to ROS position 
    Vector3 UnityToRosPositionAxisConversion(Vector3 rosIn) {
        return new Vector3(rosIn.z, -rosIn.x, rosIn.y);
    }

    //Convert 4D Unity quaternion to ROS quaternion
    Quaternion UnityToRosRotationAxisConversion(Quaternion qIn) {

        Quaternion temp = (new Quaternion(-qIn.w, -qIn.y, qIn.x, -qIn.z));
        return temp;

        //return new Quaternion(-qIn.z, qIn.x, -qIn.w, -qIn.y);
        //return new Quaternion(-qIn.z, qIn.w, -qIn.x, -qIn.y);
        //return new Quaternion(-qIn.z, qIn.w, -qIn.x, -qIn.y);
        //return new Quaternion(-qIn.z, qIn.x, qIn.w, qIn.y);
    }
}