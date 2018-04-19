using RosSharp.RosBridgeClient;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EinRightForthCommandsPublisher : Publisher {

    public string Topic = "\ein\right\forth_commands";


	// Use this for initialization
	void Start () {
        rosSocket = GetComponent<RosConnector>().RosSocket;

        publicationId = rosSocket.Advertise("/ein/right/forth_commands", "std_msgs/String");
        PublicationEvent += ReadMessage;
    }
	
	// Update is called once per frame
	void Update () {
		
	}
}
