using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using HoloToolkit.Unity;
using RosSharp.RosBridgeClient;
using HoloToolkit.Unity.InputModule;
using System;

public class Group : MonoBehaviour, IInputClickHandler
{
    public string GID; //group id
    public Dictionary<string, GameObject> SIDToObj { get; set; }
    public Dictionary<string, bool> OutOfBounds { get; set; }
    public GameObject FirstWaypoint { get; set; }
    public GameObject LastSmartGripper { get; set; }
    public int NumPoints { get; set; }
    public StagingManager StagingManager;
    public Button GroupButton;
    public bool InTrainingQueue;
    public Button CopyButton;
    public string SlotNum;
    private string prevSlotNum;

    // Use this for initialization
    void Awake () {
        GID = IdGenerator.Instance.CreateGID(this);
        SIDToObj = new Dictionary<string, GameObject>();
        SIDToObj.Add("START", null);
        OutOfBounds = new Dictionary<string, bool>();
        NumPoints = 1;
        InTrainingQueue = true;
        CopyButton = null;
        SlotNum = "";
        prevSlotNum = "";
    }

    public void SetOutOfBounds(string id)
    {
        SIDToObj[id].GetComponent<TargetModelBehavior>().MakeRed();
        OutOfBounds[id] = true;
    }

    public void SetInBounds(string id)
    {
        SIDToObj[id].GetComponent<TargetModelBehavior>().MakeGreen();
        OutOfBounds[id] = false;
    }

    public void AddGripper()
    {
        int pointNumber = ++NumPoints;
        // make a new smart gripper
        GameObject newObj = Instantiate(LastSmartGripper);
        newObj.GetComponent<TargetModelBehavior>().Init(GID, LastSmartGripper.GetComponent<TargetModelBehavior>().SID, null);//, "0");
        Vector3 offset = new Vector3(0.1f, 0.0f, 0.0f);
        newObj.transform.Find("Text").GetComponent<TextMesh>().text = pointNumber.ToString();
        newObj.transform.position = newObj.transform.position + offset;
        newObj.GetComponent<TargetModelBehavior>().RightOpen =
            LastSmartGripper.GetComponent<TargetModelBehavior>().RightOpen;

        // set the last smart gripper's next pointer to the newly instantiated obj's id
        LastSmartGripper.GetComponent<TargetModelBehavior>().NextId =
            newObj.GetComponent<TargetModelBehavior>().SID;

        SIDToObj.Add(newObj.GetComponent<TargetModelBehavior>().SID, newObj);
        newObj.GetComponent<TargetModelBehavior>().UpdateNumbering();

        // change the last smart gripper
        LastSmartGripper = newObj;
        LastSmartGripper.GetComponent<TargetModelBehavior>().SendPlanRequest("1", "");
    }

    public void DeleteGripper(string id)
    {
        TargetModelBehavior delGripper = SIDToObj[id].GetComponent<TargetModelBehavior>();
        int pointNumber = --NumPoints;
        if (delGripper.PrevId == "START") // case where we are at the head of the list
        {
            if (delGripper.NextId == "")
            {
                NumPoints++;
                return;
            } else
            {
                StagingManager.FirstGripperEver = SIDToObj[delGripper.NextId];
                SIDToObj[delGripper.NextId].GetComponent<TargetModelBehavior>().PrevId = "START";
                FirstWaypoint = SIDToObj[delGripper.NextId];
                SIDToObj[delGripper.NextId].GetComponent<TargetModelBehavior>().UpdateNumbering();
                SIDToObj[delGripper.NextId].GetComponent<TargetModelBehavior>().SendPlanRequest("1", "");
            }
        } else if (delGripper.NextId == "") // case where we are the tail of the list
        {
            SIDToObj[delGripper.PrevId].GetComponent<TargetModelBehavior>().NextId = "";
            SIDToObj[delGripper.PrevId].GetComponent<TargetModelBehavior>().SendPlanRequest("1", "");
            LastSmartGripper = SIDToObj[delGripper.PrevId];
        } else // case where we are somewhere in the middle of the list
        {
            SIDToObj[delGripper.NextId].GetComponent<TargetModelBehavior>().PrevId = delGripper.PrevId;
            SIDToObj[delGripper.PrevId].GetComponent<TargetModelBehavior>().NextId = delGripper.NextId;
            SIDToObj[delGripper.NextId].GetComponent<TargetModelBehavior>().SendPlanRequest("0", "");
            SIDToObj[delGripper.PrevId].GetComponent<TargetModelBehavior>().SendPlanRequest("0", "");
            FirstWaypoint.GetComponent<TargetModelBehavior>().UpdateNumbering();
            FirstWaypoint.GetComponent<TargetModelBehavior>().SendPlanRequest("1", "");
        }
        Destroy(SIDToObj[id]);
        SIDToObj.Remove(id);
    }

    public void HideGroup()
    {
        var colors = GroupButton.colors;
        colors.normalColor = new Color(0.94f, 0.65f, 0.93f);
        GroupButton.colors = colors;
        List<GameObject> wayPointList = this.TraverseGroup();
        foreach (GameObject p in wayPointList)
        {
            p.SetActive(false);
        }

    } 

    public void ShowGroup()
    {
        var colors = GroupButton.colors;
        colors.normalColor = new Color(0.99f, 0.96f, 0.66f);
        GroupButton.colors = colors;
        List<GameObject> wayPointList = this.TraverseGroup();
        foreach (GameObject p in wayPointList)
        {
            p.SetActive(true);
        }
    }

    public List<GameObject> TraverseGroup()
    {
        List<GameObject> wayPointList = new List<GameObject>();
        GameObject curr = FirstWaypoint;
        while (true)
        {
            wayPointList.Add(curr);
            //if (curr.GetComponent<TargetModelBehavior>().PrevShadowId != "")
            //{
            //    wayPointList.Add(SIDToObj[curr.GetComponent<TargetModelBehavior>().PrevShadowId]);
            //}
            string nextId = curr.GetComponent<TargetModelBehavior>().NextId;
            if (nextId != "")
            {
                curr = SIDToObj[nextId];
            }
            else
            {
                break;
            }
        }
        return wayPointList;
    }

    public void InitCopy(string gid)
    {
        List<GameObject> sourceWaypoints = this.TraverseGroup();
        List<GameObject> targetWaypoints = new List<GameObject>();
        Dictionary<string, int> ListIndex = new Dictionary<string, int>();
        for(int i = 0; i < sourceWaypoints.Count; i++)
        {
            GameObject tp = Instantiate(sourceWaypoints[i]);
            tp.GetComponent<TargetModelBehavior>().Init(gid, null, null);//, sourceWaypoints[i].GetComponent<TargetModelBehavior>().RightOpen);
            ListIndex.Add(sourceWaypoints[i].GetComponent<TargetModelBehavior>().SID, i);
            targetWaypoints.Add(tp);
        }
        for(int i = 0; i < sourceWaypoints.Count; i++)
        {
            if(sourceWaypoints[i].GetComponent<TargetModelBehavior>().PrevId != "")
            {
                if (sourceWaypoints[i].GetComponent<TargetModelBehavior>().PrevId == "START")
                {
                    targetWaypoints[i].GetComponent<TargetModelBehavior>().PrevId = "START";
                } else
                {
                    targetWaypoints[i].GetComponent<TargetModelBehavior>().PrevId =
                        targetWaypoints[ListIndex[sourceWaypoints[i].GetComponent<TargetModelBehavior>().PrevId]]
                        .GetComponent<TargetModelBehavior>().SID;
                }
            }
            if (sourceWaypoints[i].GetComponent<TargetModelBehavior>().NextId != "")
            {
                targetWaypoints[i].GetComponent<TargetModelBehavior>().NextId =
                    targetWaypoints[ListIndex[sourceWaypoints[i].GetComponent<TargetModelBehavior>().NextId]]
                    .GetComponent<TargetModelBehavior>().SID;
            }
            //if (sourceWaypoints[i].GetComponent<TargetModelBehavior>().PrevShadowId != "")
            //{
            //    targetWaypoints[i].GetComponent<TargetModelBehavior>().PrevShadowId =
            //        targetWaypoints[ListIndex[sourceWaypoints[i].GetComponent<TargetModelBehavior>().PrevShadowId]]
            //        .GetComponent<TargetModelBehavior>().SID;
            //}
            //if (sourceWaypoints[i].GetComponent<TargetModelBehavior>().NextShadowId != "")
            //{
            //    targetWaypoints[i].GetComponent<TargetModelBehavior>().NextShadowId =
            //        targetWaypoints[ListIndex[sourceWaypoints[i].GetComponent<TargetModelBehavior>().NextShadowId]]
            //        .GetComponent<TargetModelBehavior>().SID;
            //} 
            targetWaypoints[i].GetComponent<TargetModelBehavior>().RightOpen = sourceWaypoints[i].GetComponent<TargetModelBehavior>().RightOpen;
            targetWaypoints[i].GetComponent<TargetModelBehavior>().LeftOpen = sourceWaypoints[i].GetComponent<TargetModelBehavior>().LeftOpen;
            //targetWaypoints[i].GetComponent<TargetModelBehavior>().IsShadow = sourceWaypoints[i].GetComponent<TargetModelBehavior>().IsShadow;
        }

        // OutOfBounds = new Dictionary<string, bool>();
        for(int i = 0; i < sourceWaypoints.Count; i++)
        {
            GameObject.Find("dummy").GetComponent<Group>().SIDToObj
                .Add(targetWaypoints[i].GetComponent<TargetModelBehavior>().SID, targetWaypoints[i]);
            //if (!targetWaypoints[i].GetComponent<TargetModelBehavior>().IsShadow) {
            targetWaypoints[i].GetComponent<TargetModelBehavior>().SendPlanRequest("0", "");
            //}
            if (targetWaypoints[i].GetComponent<TargetModelBehavior>().PrevId == "START")
            {
                GameObject.Find("dummy").GetComponent<Group>().FirstWaypoint =
                    GameObject.Find("dummy").GetComponent<Group>().SIDToObj[targetWaypoints[i].GetComponent<TargetModelBehavior>().SID];
            }
            if (targetWaypoints[i].GetComponent<TargetModelBehavior>().NextId == "")
            {
                GameObject.Find("dummy").GetComponent<Group>().LastSmartGripper =
                    GameObject.Find("dummy").GetComponent<Group>().SIDToObj[targetWaypoints[i].GetComponent<TargetModelBehavior>().SID];
            }
        }
        GameObject.Find("dummy").transform.localPosition = new Vector3(0.0f, 0.0f, 0.0f);
        GameObject.Find("dummy").GetComponent<Group>().SlotNum = prevSlotNum;
        GameObject.Find("dummy").GetComponent<Group>().NumPoints = NumPoints;
        GameObject.Find("dummy").GetComponent<Group>().HideGroup();
        GameObject.Find("dummy").GetComponent<Group>().InTrainingQueue = true;
        GameObject.Find("dummy").name = "Button";
    }

    public void ManipulationStarted()
    {
        if(StagingManager.currentGroupButton.GetComponent<Group>().GID != GID)
        {
            Debug.Log("We in here fam");
            StagingManager.ChangeCurrentGroup(GroupButton);
        }
        if (InTrainingQueue)
        {
            CopyButton = Instantiate(gameObject.GetComponent<Button>());
            CopyButton.name = "dummy";
            var colors = GroupButton.colors;
            colors.normalColor = new Color(0.94f, 0.65f, 0.93f);
            CopyButton.colors = colors;
            prevSlotNum = SlotNum;
            CopyButton.transform.parent = GameObject.Find("Slot" + SlotNum).transform;
            CopyButton.transform.localPosition = new Vector3(0.0f, 0.0f, 0.0f);
            CopyButton.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
            CopyButton.GetComponentInChildren<Text>().text = "Group" + SlotNum;
        }       
    }

    public void ManipulationEnded()
    {

        if (InTrainingQueue)
        {
            GameObject argMin = null;
            float min = Single.PositiveInfinity;
            string index = SlotNum;
            for (int i = 10; i < 20; i++)
            {
                float distToSlot = Vector2.Distance(GameObject.Find("Slot" + i.ToString()).transform.position, gameObject.transform.position);
                // Debug.Log(distToSlot);
                if (distToSlot < 0.6f && distToSlot < min)
                {
                    min = distToSlot;
                    argMin = GameObject.Find("Slot" + i.ToString());
                    index = i.ToString();
                }
            }
            if (argMin != null && argMin.transform.childCount == 0)
            {
                InTrainingQueue = false;
                gameObject.transform.parent = argMin.transform;
                gameObject.transform.localPosition = new Vector3(0.0f, 0.0f, 0.0f);
                gameObject.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
                SlotNum = index;
                this.InitCopy(CopyButton.GetComponent<Group>().GID);
                CopyButton = null;
            }
            else
            {
                if (CopyButton != null)
                {
                    IdGenerator.Instance.GIDtoGroup.Remove(CopyButton.GetComponent<Group>().GID);
                    Destroy(GameObject.Find("dummy"));
                    CopyButton = null;
                    gameObject.transform.localPosition = new Vector3(0.0f, 0.0f, 0.0f);
                }

            }
        } else // case where we are on the run queue
        {
            GameObject argMin = null;
            float min = Single.PositiveInfinity;
            float distToSlot = 0.0f;
            string index = SlotNum;
            for (int i = 10; i < 20; i++)
            {
                distToSlot = Vector2.Distance(GameObject.Find("Slot" + i.ToString()).transform.position, gameObject.transform.position);
                if (distToSlot < min)
                {
                    min = distToSlot;
                    argMin = GameObject.Find("Slot" + i.ToString());
                    index = i.ToString();
                }
            }
            if (distToSlot > 0.6)
            {
                Debug.Log("CASE WHERE I WANT TO DELETE");
                foreach (KeyValuePair<string, GameObject> entry in SIDToObj)
                {
                    Destroy(entry.Value);
                }
                IdGenerator.Instance.GIDtoGroup.Remove(GID);
                Destroy(gameObject);
                StagingManager.currentGroupButton = GameObject.Find("Slot0").transform.GetComponentInChildren<Button>();
                StagingManager.currentGroupButton.GetComponent<Group>().ShowGroup();
            }
            else if (argMin.transform.childCount == 0)
            {
                gameObject.transform.parent = argMin.transform;
                gameObject.transform.localPosition = new Vector3(0.0f, 0.0f, 0.0f);
                gameObject.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
                SlotNum = index;
            }
            else
            {
                gameObject.transform.localPosition = new Vector3(0.0f, 0.0f, 0.0f);
            }
        }
            
    }

    public void OnInputClicked(InputClickedEventData eventData)
    {
        if (!eventData.used)
        {
            StagingManager.ChangeCurrentGroup(GroupButton);
            eventData.Use();
        }
    }
}