using RosSharp.RosBridgeClient;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EinRightForthCommandsPublisher : Publisher {

    public string Topic = "ein/right/forth_commands";
    private StandardString message;
    private VRTK.VRTK_ControllerEvents controller;


    // Use this for initialization
    void Start () {
        rosSocket = GetComponent<RosConnector>().RosSocket;

        // Get the controller component of this gameobject
        controller = GetComponent<VRTK.VRTK_ControllerEvents>();

        publicationId = rosSocket.Advertise("ein/right/forth_commands", "std_msgs/String");
        message = new StandardString();
    }
	
	// Update is called once per frame
	void Update () {
        if (controller.gripPressed) {
            message.data = "dogSummonSpot";
            rosSocket.Publish(publicationId, message);
            Debug.Log("button press");
        }

        message.data = "dogSummonSpot";
        rosSocket.Publish(publicationId, message);


    }
}
