using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HoloToolkit.Unity.InputModule;
using UnityEngine.XR.WSA.Input;

namespace RosSharp.RosBridgeClient
{
    [RequireComponent(typeof(RosConnector))]
    public class TargetModelBehavior : MonoBehaviour, IInputClickHandler, INavigationHandler
    {

        public string PlanTopic = "/goal_pose";

        public string Id { get; set; }
        public string PrevId { get; set; }
        public string NextId { get; set; }
        public string RightOpen { get; set; }
        public string LeftOpen { get; set; }
        public bool OutOfBounds { get; set; }

        public MoveItGoalPublisher MoveItGoalPublisher;

        public GameObject UrdfModel; // the root gameobject of your robot
        public GameObject TargetModel;

        // Use this for initialization
        void Awake()
        {
            Debug.Log("hi hello");
            Id = IdGenerator.Instance.CreateId();
            PrevId = "";
            NextId = "";
            RightOpen = "0";
            LeftOpen = "0";
            if (IdGenerator.Instance.PointsToStart == false)
            {
                IdGenerator.Instance.PointsToStart = true;
                PrevId = "START";
            }
            OutOfBounds = false;
            Debug.Log("got here");
        }

        GeometryPose UpdateMessageContents(GameObject TargetModel)
        {
            GeometryPose TargetPose = new GeometryPose();
            Vector3 position = TargetModel.transform.position - UrdfModel.transform.position;
            Quaternion rotation = UrdfModel.transform.rotation * TargetModel.transform.rotation;
            TargetPose.position = GetGeometryPoint(position.Unity2Ros());
            TargetPose.position = new GeometryPoint
            {
                x = -TargetPose.position.x,
                y = -TargetPose.position.y,
                z = TargetPose.position.z
            };
            TargetPose.orientation = GetGeometryQuaternion(rotation.Unity2Ros());
            return TargetPose;
        }

        void PlanHandler(object args)
        {
            return;
        }

        private GeometryPoint GetGeometryPoint(Vector3 position)
        {
            GeometryPoint geometryPoint = new GeometryPoint
            {
                x = position.x,
                y = position.y,
                z = position.z
            };
            return geometryPoint;
        }
        private GeometryQuaternion GetGeometryQuaternion(Quaternion quaternion)
        {
            GeometryQuaternion geometryQuaternion = new GeometryQuaternion
            {
                x = quaternion.x,
                y = quaternion.y,
                z = quaternion.z,
                w = quaternion.w
            };
            return geometryQuaternion;
        }

        public void SendPlanRequest()
        {
            MoveItGoalPublisher.LastManipulatedGripper = this.gameObject;
            MoveitTarget moveitTarget = new MoveitTarget();
            moveitTarget.right_arm = UpdateMessageContents(TargetModel);
            moveitTarget.id.data = Id;
            moveitTarget.prev_id.data = PrevId;
            moveitTarget.next_id.data = NextId;
            moveitTarget.right_open.data = RightOpen;
            moveitTarget.left_open.data = LeftOpen;
            MoveItGoalPublisher.PublishPlan(moveitTarget);
        }

        void IInputClickHandler.OnInputClicked(InputClickedEventData eventData)
        {
            Debug.Log("tap");
            // TODO: Graphically change gripper
            RightOpen = (RightOpen == "0") ? "1" : "0"; // toggle gripper state
            this.SendPlanRequest();
        }

        void INavigationHandler.OnNavigationStarted(NavigationEventData eventData)
        {
            
        }

        void INavigationHandler.OnNavigationUpdated(NavigationEventData eventData)
        {
            
        }

        void INavigationHandler.OnNavigationCompleted(NavigationEventData eventData)
        {
            this.SendPlanRequest();
        }

        void INavigationHandler.OnNavigationCanceled(NavigationEventData eventData)
        {
            this.SendPlanRequest();
        }
    }
}
