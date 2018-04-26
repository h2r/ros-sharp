using RosSharp.RosBridgeClient;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EinRightForthCommandsPublisher : Publisher {

    public string Topic = "ein/right/forth_commands";
    private StandardString message;



    // Use this for initialization
    void Start () {
        rosSocket = GetComponent<RosConnector>().RosSocket;

        // Get the controller component of this gameobject

        publicationId = rosSocket.Advertise("ein/right/forth_commands", "std_msgs/String");
        message = new StandardString();
    }
	
	// Update is called once per frame
	void Update () {
        if (Input.GetKeyDown("joystick button 2") || Input.GetAxis("left grip button") > 0.5f) {
            message.data = "\"aibo\" import\ndogSummonSpot";
            rosSocket.Publish(publicationId, message);
            Debug.Log("button press");
        }



    }
}
