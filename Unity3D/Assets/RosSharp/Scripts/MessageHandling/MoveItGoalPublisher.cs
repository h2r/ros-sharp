using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RosSharp.RosBridgeClient {

    [RequireComponent(typeof(RosConnector))]
    public class MoveItGoalPublisher : MonoBehaviour {

        public string PlanTopic;
        public string ExecuteTopic;

        public GameObject UrdfModel; // the root gameobject of your robot
        public GameObject LeftTargetModel; // the goal target
        public GameObject RightTargetModel; // the goal target

        private RosSocket rosSocket;
        private int planPublicationId;
        private int executePublicationId;

        //private MoveitTarget MoveitTarget = new MoveitTarget();

        // Use this for initialization
        void Start() {
            rosSocket = GetComponent<RosConnector>().RosSocket;
            planPublicationId = rosSocket.Advertise(PlanTopic, "ros_reality_bridge_msgs/MoveitTarget");
            executePublicationId = rosSocket.Advertise(ExecuteTopic, "std_msgs/String");
            this.ResetBackend();
            //FadeManager.AssertIsInitialized();
        }

        public void PublishPlan(MoveitTarget moveitTarget) {
            rosSocket.Publish(planPublicationId, moveitTarget);
        }

        public void PublishMove(string groups) {
            Debug.Log("Sending execute message");
            StandardString groupData = new StandardString();
            groupData.data = groups;
            rosSocket.Publish(executePublicationId, groupData);
        }

        void PlanHandler(object args) {
            return;
        }

        public void ResetBackend()
        {
            rosSocket.Publish(planPublicationId, new MoveitTarget());
        }
    }
}
