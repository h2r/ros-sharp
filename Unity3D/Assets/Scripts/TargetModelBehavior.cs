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

        public string GID { get; set; }
        public string SID { get; set; }
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
            SID = "";
            PrevId = "";
            NextId = "";
            PrevShadowId = "";
            NextShadowId = "";
            RightOpen = "0";
            LeftOpen = "0";
            IsShadow = false;
            GID = "";
        }

        public void Init(string gid, string prevId, string nextId)
        {
            SID = IdGenerator.Instance.CreateSID(gid);
            GID = gid;
            if (prevId != null)
            {
                PrevId = prevId;
            }
            if (nextId != null)
            {
                NextId = nextId;
            }
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
            MoveitTarget moveitTarget = new MoveitTarget();
            moveitTarget.right_arm = UpdateMessageContents(TargetModel);
            moveitTarget.sid.data = SID;
            moveitTarget.gid.data = GID;
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
                newObj.GetComponent<TargetModelBehavior>().NextId = SID;
                newObj.GetComponent<TargetModelBehavior>().MakeYellow();
                this.SetInterpTransform(newObj, IdGenerator.Instance.GIDtoGroup[GID].SIDToObj[PrevId], gameObject);
                PrevShadowId = newObj.GetComponent<TargetModelBehavior>().SID;
                IdGenerator.Instance.GIDtoGroup[GID].SIDToObj[PrevId].GetComponent<TargetModelBehavior>().NextShadowId = PrevShadowId;
                newObj.transform.Find("Text").GetComponent<TextMesh>().text = "";
            }
            if (createNext)
            {
                GameObject newObj = Instantiate(gameObject);
                newObj.GetComponent<TargetModelBehavior>().PrevId = SID;
                newObj.GetComponent<TargetModelBehavior>().NextId = NextId;
                newObj.GetComponent<TargetModelBehavior>().MakeYellow();
                this.SetInterpTransform(newObj, gameObject, IdGenerator.Instance.GIDtoGroup[GID].SIDToObj[NextId]);
                NextShadowId  = newObj.GetComponent<TargetModelBehavior>().SID;
                IdGenerator.Instance.GIDtoGroup[GID].SIDToObj[NextId].GetComponent<TargetModelBehavior>().PrevShadowId = NextShadowId;
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
                    IdGenerator.Instance.GIDtoGroup[GID].SIDToObj[PrevId].GetComponent<TargetModelBehavior>().NextId = SID;
                    IdGenerator.Instance.GIDtoGroup[GID].SIDToObj[NextId].GetComponent<TargetModelBehavior>().PrevId = SID;
                }
                UpdateNumbering();
                this.SendPlanRequest();
            }
            
        }

        public void UpdateNumbering()
        {
            Debug.Log("In update #");
            Debug.Log(GID);
            Debug.Log(IdGenerator.Instance.GIDtoGroup);
            Debug.Log(IdGenerator.Instance.GIDtoGroup[GID].FirstWaypoint);
            GameObject curr = IdGenerator.Instance.GIDtoGroup[GID].FirstWaypoint;
            int num = 1;
            while (true)
            {
                curr.transform.Find("Text").GetComponent<TextMesh>().text = num.ToString();
                if(curr.GetComponent<TargetModelBehavior>().NextId != "")
                {
                    curr = IdGenerator.Instance.GIDtoGroup[GID].SIDToObj[curr.GetComponent<TargetModelBehavior>().NextId];
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
                IdGenerator.Instance.GIDtoGroup[GID].SIDToObj[PrevId].GetComponent<TargetModelBehavior>().NextId = SID;
                IdGenerator.Instance.GIDtoGroup[GID].SIDToObj[NextId].GetComponent<TargetModelBehavior>().PrevId = SID;
            } else // moving a green gripper
            {
                if (PrevShadowId != "") // case where we just need to update position
                {
                    SetInterpTransform(IdGenerator.Instance.GIDtoGroup[GID].SIDToObj[PrevShadowId], 
                        IdGenerator.Instance.GIDtoGroup[GID].SIDToObj[PrevId], gameObject);
                } else
                {
                    if (PrevId != "START")
                    {
                        this.MakeShadow(true, false);
                    }
                }
                if (NextShadowId != "") // case where we just need to update position
                {
                    SetInterpTransform(IdGenerator.Instance.GIDtoGroup[GID].SIDToObj[NextShadowId], 
                        gameObject,
                        IdGenerator.Instance.GIDtoGroup[GID].SIDToObj[NextId]);
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
