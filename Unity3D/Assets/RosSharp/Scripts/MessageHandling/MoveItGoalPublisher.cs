using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RosSharp.RosBridgeClient {

    [RequireComponent(typeof(RosConnector))]
    public class MoveItGoalPublisher : MonoBehaviour {

        public string PlanTopic;
        public string ExecuteTopic;

        public GameObject UrdfBaseModel; // the root gameobject of your robot
        public GameObject UrdfEndEffector; // the gameobject of the end effector of your robot
        public GameObject TargetModel; // the goal target


        private RosSocket rosSocket;
        private int planPublicationId;
        private int executePublicationId;

        private GeometryPose TargetPose = new GeometryPose();

        // Use this for initialization
        void Start() {
            rosSocket = GetComponent<RosConnector>().RosSocket;
            planPublicationId = rosSocket.Advertise(PlanTopic, "geometry_msgs/Pose");
            executePublicationId = rosSocket.Advertise(ExecuteTopic, "std_msgs/String");
        }

        public void PublishPlan() {
            UpdateMessageContents();
            Debug.Log("Sending plan request");
            rosSocket.Publish(planPublicationId, TargetPose);
        }

        public void PublishMove() {
            Debug.Log("Sending execute message");
            rosSocket.Publish(executePublicationId, new StandardString());
        }

        void PlanHandler(object args) {
            return;
        }

        void UpdateMessageContents() {

            Vector3 position = TargetModel.transform.position - UrdfBaseModel.transform.position;
            Quaternion rotation = UrdfBaseModel.transform.rotation * TargetModel.transform.rotation;
            TargetPose.position = GetGeometryPoint(position.Unity2Ros());
            TargetPose.position = new GeometryPoint {
                x = -TargetPose.position.x,
                y = -TargetPose.position.y,
                z = TargetPose.position.z
            };
            TargetPose.orientation = GetGeometryQuaternion(rotation.Unity2Ros());

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
