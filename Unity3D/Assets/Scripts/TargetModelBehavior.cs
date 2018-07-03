using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HoloToolkit.Unity.InputModule;
using UnityEngine.XR.WSA.Input;

namespace RosSharp.RosBridgeClient
{
    //[RequireComponent(typeof(RosConnector))]
    public class TargetModelBehavior : MonoBehaviour, INavigationHandler, IInputClickHandler
    {

        public string PlanTopic = "/goal_pose";

        public string Id { get; set; }
        public string PrevId { get; set; }
        public string NextId { get; set; }
        public string PrevShadowId { get; set; }
        public string NextShadowId { get; set; }
        public string RightOpen { get; set; }
        public string LeftOpen { get; set; }
        public bool IsShadow { get; set; }

        public MoveItGoalPublisher MoveItGoalPublisher;

        public GameObject UrdfModel; // the root gameobject of your robot
        public GameObject TargetModel;

        // Use this for initialization
        void Awake()
        {
            Debug.Log("hi hello");
            Id = IdGenerator.Instance.CreateId(gameObject); // IdGenerator keeps references to each object
            PrevId = "";
            NextId = "";
            PrevShadowId = "";
            NextShadowId = "";
            RightOpen = "0";
            LeftOpen = "0";
            IsShadow = false;
            if (IdGenerator.Instance.FirstWaypoint == null)
            {
                IdGenerator.Instance.FirstWaypoint = gameObject;
                PrevId = "START";
            }
            IdGenerator.Instance.OutOfBounds[Id] = false;
            
        }

        public void MakeRed()
        {
            Renderer[] rs = gameObject.GetComponentsInChildren<Renderer>();
            foreach (Renderer r in rs)
            {
                r.material.color = new Color(0.95f, 0.56f, 0.56f);
            }
        }

        public void MakeGreen()
        {
            IsShadow = false;
            Renderer[] rs = gameObject.GetComponentsInChildren<Renderer>();
            foreach (Renderer r in rs)
            {
                r.material.color = new Color(0.56f, 0.95f, 0.82f);
            }
        }

        public void MakeYellow()
        {
            IsShadow = true;
            Renderer[] rs = gameObject.GetComponentsInChildren<Renderer>();
            foreach (Renderer r in rs)
            {
                r.material.color = new Color(0.93f, 0.95f, 0.56f);
            }
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

        public void MakeShadow(bool createPrev, bool createNext)
        {
            if (createPrev)
            {
                GameObject newObj = Instantiate(gameObject);
                newObj.GetComponent<TargetModelBehavior>().PrevId = PrevId;
                newObj.GetComponent<TargetModelBehavior>().NextId = Id;
                newObj.GetComponent<TargetModelBehavior>().MakeYellow();
                this.SetInterpTransform(newObj, IdGenerator.Instance.IdToObj[PrevId], gameObject);
                PrevShadowId = newObj.GetComponent<TargetModelBehavior>().Id;
                IdGenerator.Instance.IdToObj[PrevId].GetComponent<TargetModelBehavior>().NextShadowId = PrevShadowId;
                newObj.transform.Find("Text").GetComponent<TextMesh>().text = "";
            }
            if (createNext)
            {
                GameObject newObj = Instantiate(gameObject);
                newObj.GetComponent<TargetModelBehavior>().PrevId = Id;
                newObj.GetComponent<TargetModelBehavior>().NextId = NextId;
                newObj.GetComponent<TargetModelBehavior>().MakeYellow();
                this.SetInterpTransform(newObj, gameObject, IdGenerator.Instance.IdToObj[NextId]);
                NextShadowId  = newObj.GetComponent<TargetModelBehavior>().Id;
                IdGenerator.Instance.IdToObj[NextId].GetComponent<TargetModelBehavior>().PrevShadowId = NextShadowId;
                newObj.transform.Find("Text").GetComponent<TextMesh>().text = "";
            }
        }

        private void SetInterpTransform(GameObject obj, GameObject from, GameObject to)
        {
            obj.transform.position = Vector3.Lerp(from.transform.position, to.transform.position, 0.5f); ;
            obj.transform.rotation = Quaternion.Slerp(from.transform.rotation, to.transform.rotation, 0.5f);
        }

        void IInputClickHandler.OnInputClicked(InputClickedEventData eventData)
        {
            if (!eventData.used)
            {
                eventData.Use();
                Debug.Log("tap");
                Vector3 offset = new Vector3(0.02f, 0.0f, 0.0f);
                Debug.Log(RightOpen);
                if (RightOpen == "0")
                {
                    RightOpen = "1";
                    gameObject.transform.GetChild(1).transform.localPosition -= offset;
                    gameObject.transform.GetChild(2).transform.localPosition += offset;
                }
                else
                {
                    RightOpen = "0";
                    gameObject.transform.GetChild(1).transform.localPosition += offset;
                    gameObject.transform.GetChild(2).transform.localPosition -= offset;
                }

                if (IsShadow) // case where someone moved a yellow gripper
                {
                    this.MakeShadow(true, true);
                    this.MakeGreen();
                    IdGenerator.Instance.IdToObj[PrevId].GetComponent<TargetModelBehavior>().NextId = Id;
                    IdGenerator.Instance.IdToObj[NextId].GetComponent<TargetModelBehavior>().PrevId = Id;
                }
                UpdateNumbering();
                this.SendPlanRequest();
            }
            
        }

        public void UpdateNumbering()
        {
            GameObject curr = IdGenerator.Instance.FirstWaypoint;
            int num = 1;
            while (true)
            {
                curr.transform.Find("Text").GetComponent<TextMesh>().text = num.ToString();
                if(curr.GetComponent<TargetModelBehavior>().NextId != "")
                {
                    curr = IdGenerator.Instance.IdToObj[curr.GetComponent<TargetModelBehavior>().NextId];
                    num++;
                } else
                {
                    break;
                }
               
            }
        }

        void INavigationHandler.OnNavigationStarted(NavigationEventData eventData)
        {
            //Debug.Log("ONs");
        }

        void INavigationHandler.OnNavigationUpdated(NavigationEventData eventData)
        {
            //Debug.Log("ONup");
        }

        void INavigationHandler.OnNavigationCompleted(NavigationEventData eventData)
        {
            // if you are a yellow gripper, we need to become green, get networked in etc.
            if(IsShadow) // case where someone moved a yellow gripper
            {
                this.MakeShadow(true, true);
                this.MakeGreen();
                IdGenerator.Instance.IdToObj[PrevId].GetComponent<TargetModelBehavior>().NextId = Id;
                IdGenerator.Instance.IdToObj[NextId].GetComponent<TargetModelBehavior>().PrevId = Id;
            } else // moving a green gripper
            {
                Debug.Log("(0)");
                if (PrevShadowId != "") // case where we just need to update position
                {
                    Debug.Log("(1)");
                    SetInterpTransform(IdGenerator.Instance.IdToObj[PrevShadowId], IdGenerator.Instance.IdToObj[PrevId], gameObject);
                } else
                {
                    Debug.Log("(2)");
                    if (PrevId != "START")
                    {
                        Debug.Log("(3)");
                        this.MakeShadow(true, false);
                    }
                }
                if (NextShadowId != "") // case where we just need to update position
                {
                    Debug.Log("(4)");
                    SetInterpTransform(IdGenerator.Instance.IdToObj[NextShadowId], gameObject, IdGenerator.Instance.IdToObj[NextId]);
                }
            }

            // update the position of the gripper
            UpdateNumbering();
            this.SendPlanRequest();
        }

        void INavigationHandler.OnNavigationCanceled(NavigationEventData eventData)
        {
            this.SendPlanRequest();
        }
    }
}
