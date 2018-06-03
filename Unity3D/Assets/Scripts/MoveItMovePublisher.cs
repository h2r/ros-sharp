using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RosSharp.RosBridgeClient;

public class MoveItMovePublisher : MonoBehaviour {

    public string ButtonName;
    public string Topic;

    private RosSocket rosSocket;
    private int publicationId;
    private bool isDown = false;

    public DisplayTrajectoryReceiver DisplayTrajectoryReceiver;

    // Use this for initialization
    void Start () {
        rosSocket = GetComponent<RosConnector>().RosSocket;
        publicationId = rosSocket.Advertise(Topic, "std_msgs/String");
    }
	
	// Update is called once per frame
	void Update () {
        if (Input.GetAxis(ButtonName) > 0.99f && !isDown) {
            Debug.Log("Sending move request");
            rosSocket.Publish(publicationId, new StandardString());
            isDown = true;
            DisplayTrajectoryReceiver.DestroyTrail();
            DisplayTrajectoryReceiver.UrdfModel.SetActive(false);
        }
        if (Input.GetAxis(ButtonName) < 0.01f) {
            isDown = false;
        }
    }
}
