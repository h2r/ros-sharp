using RosSharp.RosBridgeClient;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


// The class for this script extends the Publisher class of ROS# which was authored by Dr. Martin Bischoff
public class ImitationLearningPublisher : Publisher {

    // Declaring a variable for the message that will be published on the ROS topic
    private StandardString message;

    private Boolean recording = false;
    private Boolean rightTrackpadPressed = false;

    public GameObject recordingText;

    // Used for initialization
    protected override void Start() {
        rosSocket = GetComponent<RosConnector>().RosSocket;
        publicationId = rosSocket.Advertise(Topic, "std_msgs/String");
        message = new StandardString();
        recordingText.SetActive(false);
        InvokeRepeating("SendRecording", .1f, .1f);
    }

    void SendRecording() {
        message.data = "";
        if(Input.GetButton("Right_trackpad_button")) {
            rightTrackpadPressed = true;
        }
        else {
            if(rightTrackpadPressed) {
                if (recording) {
                    recording = false;
                    recordingText.SetActive(false);
                    message.data = "0";
                }
                else {
                    recording = true;
                    recordingText.SetActive(true);
                    message.data = "1";
                }
            }
            if (message.data != "") {
                Debug.Log(message.data);
                rosSocket.Publish(publicationId, message);
                rightTrackpadPressed = false;
            }
        }
    }
}