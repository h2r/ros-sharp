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

        private MoveitTarget MoveitTarget = new MoveitTarget();

        // Use this for initialization
        void Start() {
            rosSocket = GetComponent<RosConnector>().RosSocket;
            // planPublicationId = rosSocket.Advertise(PlanTopic, "ros_reality_bridge_msgs/MoveitTarget");
            // executePublicationId = rosSocket.Advertise(ExecuteTopic, "std_msgs/String");
        }

        public void PublishPlan() {
            // MoveitTarget.left_arm = UpdateMessageContents(LeftTargetModel);
            MoveitTarget.right_arm = UpdateMessageContents(RightTargetModel);
            Debug.Log("Sending plan request");
            rosSocket.Publish(planPublicationId, MoveitTarget);
        }

        public void PublishMove() {
            Debug.Log("Sending execute message");
            rosSocket.Publish(executePublicationId, new StandardString());
        }

        void PlanHandler(object args) {
            return;
        }

        GeometryPose UpdateMessageContents(GameObject TargetModel) {

            GeometryPose TargetPose = new GeometryPose();

            Vector3 position = TargetModel.transform.position - UrdfModel.transform.position;
            Quaternion rotation = UrdfModel.transform.rotation * TargetModel.transform.rotation;
            TargetPose.position = GetGeometryPoint(position.Unity2Ros());
            TargetPose.position = new GeometryPoint {
                x = -TargetPose.position.x,
                y = -TargetPose.position.y,
                z = TargetPose.position.z
            };
            TargetPose.orientation = GetGeometryQuaternion(rotation.Unity2Ros());

            return TargetPose;
        }

        private GeometryPoint GetGeometryPoint(Vector3 position) {
            GeometryPoint geometryPoint = new GeometryPoint {
                x = position.x,
                y = position.y,
                z = position.z
            };
            return geometryPoint;
        }
        private GeometryQuaternion GetGeometryQuaternion(Quaternion quaternion) {
            GeometryQuaternion geometryQuaternion = new GeometryQuaternion {
                x = quaternion.x,
                y = quaternion.y,
                z = quaternion.z,
                w = quaternion.w
            };
            return geometryQuaternion;
        }
    }
}
