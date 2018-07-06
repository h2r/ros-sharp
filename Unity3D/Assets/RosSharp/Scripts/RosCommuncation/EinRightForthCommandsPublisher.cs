using RosSharp.RosBridgeClient;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

/* QUICKSTART BUTTON GUIDE FOR MOVO:
 * Forward and backward movement: Right Trackpad up and down
 * Turning: Right Trackpad left and right
 * Tilting head: Left trackpad up and down
 * Panning head: Left trackpad left and right
 * Torso Up: Right Controller Menu button (above trackpad)
 * Torso Down: Left Controller Menu button (above trackpad)
 * Move Right Arm to position: Right Grip button squeeze (robot's right arm will move position and orientation of your controller in VR)
 * Move Left Arm to position: Left Grip button squeeze (robot's left arm will move position and orientation of your controller in VR)
 * Open Right gripper: Hold Right Controller Trigger down (closed by default, so release trigger to close)
 * Open Left gripper: Hold Left Controller Trigger down (closed by default, so release trigger to close) */



/* This script takes input from a user's hand controllers and publishes relevant commands to the ROS Topic
 * /ein/right/forth_commands. Commands on this topic are executed by the robot via Ein. 
 * Note: Controller input is obtained using Unity Input for OpenVR (see here: https://docs.unity3d.com/Manual/OpenVRControllers.html)
 * Because of this, button's must be named exactly as specified in Unity's Input Manager (located in Edit/Project Settings/Input).
 * If buttons are unresponsive, then this is probably the issue.
  */

// The class for this script extends the Publisher class of ROS# which was authored by Dr. Martin Bischoff
public class EinRightForthCommandsPublisher : Publisher {

    /* The script required three GameObjects - the user's controllers and the robot model.
     * Note: The controllers don't have to be the actual controller game objects, but rather
     * anything that moves and rotates with the controllers (so perhaps a child of the actual
     * controller game objects). This is because only the position and rotation is obtained from this
     * GameObject, not any of the button presses.
     */
    public GameObject leftController;
    public GameObject rightController;
    public GameObject Robot;

    // Declaring a variable for the message that will be published on the ROS topic
    private StandardString message;


    /* Below are a set of variables used to keep track of button states. These are necessary because the main loop of
       this script runs every 0.1 seconds - so simply publishing a command if a button is pressed would cause multiple
       commands to be published on every click, which is undesirable behavior. Keeping track of previous button states
       enables the script to detect clicks and holding of buttons and publish commands accordingly. */
    /**************************************************************************************************************/

    // currArm decides which arm is currently being controlled. False is right and True is left. 
    // It starts with right by default.
    private bool currArm = false;
    private bool firstTime = true;

    // Booleans to keep track of the state of the grip buttons
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

    /**************************************************************************************************************/

    // Used for initialization
    protected override void Start() {
        rosSocket = GetComponent<RosConnector>().RosSocket;
        publicationId = rosSocket.Advertise(Topic, "std_msgs/String");
        message = new StandardString();
        message.data = "baseGoCfg"; // This command is a quirk of the Movo. Base doesn't move if this isn't sent first.
        rosSocket.Publish(publicationId, message);
        InvokeRepeating("SendControls", .1f, .1f);
    }

    void SendControls() {
        /* Convert the Unity position of the hand controller to a ROS position (scaled) and offset whetre the robot is.
           This is where the user wants to move the robot's left/right hand */
        Vector3 outLeftPos = UnityToRosPositionAxisConversion(leftController.transform.position - Robot.transform.position);
        Vector3 outRightPos = UnityToRosPositionAxisConversion(rightController.transform.position - Robot.transform.position);
        
        //Convert the Unity rotation of the hand controller to a ROS rotation (scaled, quaternions)
        Quaternion outLeftQuat = UnityToRosRotationAxisConversion(leftController.transform.rotation);
        Quaternion outRightQuat = UnityToRosRotationAxisConversion(rightController.transform.rotation);
        
        //construct the Ein message to be published
        message.data = "";
        
        String controllerPrefix = "";

        /* The below code block sends a forward or backward command to the robot if the Right Trackpad is
           tapped once. If it is held, it sends a command once every 0.5 seconds to allow smooth
           forward and backward motion. */
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

        /* The below code block sends a left or right turn command to the robot if the Right Trackpad is
           tapped once. If it is held, it sends a command once every 0.5 seconds to allow smooth
           motion. */
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

        /* The below code block sends a tilt up or down command to the robot's head if the Right Trackpad is
           tapped once. If it is held, it sends a command once every 0.5 seconds to allow smooth
           motion. */
        if (Input.GetAxis("Left_trackpad_vertical") > 0.8) {
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

        /* The below code block sends a pan right or left command to the robot's head if the Right Trackpad is
           tapped once. If it is held, it sends a command once every 0.5 seconds to allow smooth
           motion. */
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

        /* The below code sends a torso up or down command to the robot's head if the Right Trackpad is
           tapped once. If it is held, it sends a command once every 0.2 seconds to allow smooth
            motion. */
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


        /* The below code moves the robot's arms to the user's controller's position and orientation when the
         * relevant arm's grip buttons are pressed. If the robot's arms are UNABLE to reach the position, nothing
         will happen. Also, it may take upto 10 seconds for the arms to begin moving to complicated positions.*/

        if (Input.GetAxis("Left_grip") > 0.5f && !leftGripPressed) {
            leftGripPressed = true;
        }
        //if deadman switch held in, move to new pose
        if (Input.GetAxis("Left_grip") < 0.5f && leftGripPressed) {
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
        }

        // The below code block opens and closes the robot's grippers if the relevant controller's
        // trigger button is held
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

        /* This statement first checks that there is something to publish, then takes the message
           (which may contain several commands built-up over the previous code blocks) and publishes
           it onto the ROS Network. */
        if (message.data != "") {
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
        Quaternion temp = (new Quaternion(-qIn.w, qIn.y, qIn.x, -qIn.z));
        return temp;
    }
}