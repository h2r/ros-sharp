using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HoloToolkit.Unity.InputModule;

namespace RosSharp.RosBridgeClient
{
    [RequireComponent(typeof(RosConnector))]
    public class TargetModelBehavior : MonoBehaviour, INavigationHandler, IManipulationHandler
    {

        public string PlanTopic = "/goal_pose";

        public string Id { get; set; }
        public string PrevId { get; set; }
        public string NextId { get; set; }

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
            if (IdGenerator.Instance.PointsToStart == false)
            {
                IdGenerator.Instance.PointsToStart = true;
                PrevId = "START";
            }
            // MoveItGoalPublisher.LastSmartGripper = this.gameObject;
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

        void INavigationHandler.OnNavigationStarted(NavigationEventData eventData)
        {
            InputManager.Instance.PushModalInputHandler(gameObject);
        }

        void INavigationHandler.OnNavigationUpdated(NavigationEventData eventData)
        {
            //// 2.c: Calculate a float rotationFactor based on eventData's NormalizedOffset.x multiplied by RotationSensitivity.
            //// This will help control the amount of rotation.
            //float rotationFactor = eventData.NormalizedOffset.x * RotationSensitivity;

            //// 2.c: transform.Rotate around the Y axis using rotationFactor.
            //transform.Rotate(new Vector3(0, -1 * rotationFactor, 0));
        }

        void INavigationHandler.OnNavigationCompleted(NavigationEventData eventData)
        {
            Debug.Log("SHOULD BE CALLED");
            MoveitTarget moveitTarget = new MoveitTarget();
            moveitTarget.right_arm = UpdateMessageContents(TargetModel);
            moveitTarget.id.data = Id;
            moveitTarget.prev_id.data = PrevId;
            moveitTarget.next_id.data = NextId;
            //Debug.Log(Id);
            //Debug.Log(PrevId);
            //Debug.Log(NextId);
            //Debug.Log("----------");

            MoveItGoalPublisher.PublishPlan(moveitTarget);
            InputManager.Instance.PopModalInputHandler();
        }

        void INavigationHandler.OnNavigationCanceled(NavigationEventData eventData)
        {
            MoveitTarget moveitTarget = new MoveitTarget();
            moveitTarget.right_arm = UpdateMessageContents(TargetModel);
            moveitTarget.id.data = Id;
            moveitTarget.prev_id.data = PrevId;
            moveitTarget.next_id.data = NextId;
            //Debug.Log(Id);
            //Debug.Log(PrevId);
            //Debug.Log(NextId);
            //Debug.Log("----------");

            MoveItGoalPublisher.PublishPlan(moveitTarget);
            InputManager.Instance.PopModalInputHandler();
        }

        void IManipulationHandler.OnManipulationStarted(ManipulationEventData eventData)
        {

            InputManager.Instance.PushModalInputHandler(gameObject);

        }

        void IManipulationHandler.OnManipulationUpdated(ManipulationEventData eventData)
        {

            // 4.a: Make this transform's position be the manipulationOriginalPosition + eventData.CumulativeDelta
            //transform.position = manipulationOriginalPosition + eventData.CumulativeDelta;


        }

        void IManipulationHandler.OnManipulationCompleted(ManipulationEventData eventData)
        {
            Debug.Log("WASSUP");
            InputManager.Instance.PopModalInputHandler();
            
        }

        void IManipulationHandler.OnManipulationCanceled(ManipulationEventData eventData)
        {
            InputManager.Instance.PopModalInputHandler();
        }

    }
}