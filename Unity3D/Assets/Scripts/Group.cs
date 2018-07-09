using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using HoloToolkit.Unity;
using RosSharp.RosBridgeClient;
using HoloToolkit.Unity.InputModule;
using System;

public class Group : MonoBehaviour, INavigationHandler, IInputClickHandler
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

    // Use this for initialization
    void Awake () {
        GID = IdGenerator.Instance.CreateGID(this);
        SIDToObj = new Dictionary<string, GameObject>();
        SIDToObj.Add("START", null);
        OutOfBounds = new Dictionary<string, bool>();
        NumPoints = 1;
        InTrainingQueue = true;
        CopyButton = null;
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
        newObj.GetComponent<TargetModelBehavior>().Init(GID, LastSmartGripper.GetComponent<TargetModelBehavior>().SID, null);
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
        LastSmartGripper.GetComponent<TargetModelBehavior>().SendPlanRequest();
    }

    public void HideGroup()
    {
        var colors = GroupButton.colors;
        colors.normalColor = new Color(0.94f, 0.65f, 0.93f);
        GroupButton.colors = colors;
        Debug.Log("Hide this group");
        List<GameObject> wayPointList = this.TraverseGroup();
        foreach (GameObject p in wayPointList)
        {
            p.SetActive(false);
        }

    } 

    public void ShowGroup()
    {
        Debug.Log("show this group");
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
            Debug.Log(curr);
            Debug.Log(curr.GetComponent<TargetModelBehavior>().PrevShadowId);
            if (curr.GetComponent<TargetModelBehavior>().PrevShadowId != "")
            {
                wayPointList.Add(SIDToObj[curr.GetComponent<TargetModelBehavior>().PrevShadowId]);
            }
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
            tp.GetComponent<TargetModelBehavior>().Init(gid, null, null);
            Debug.Log("Look at me : " + sourceWaypoints[i].GetComponent<TargetModelBehavior>().SID);
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
            if (sourceWaypoints[i].GetComponent<TargetModelBehavior>().PrevShadowId != "")
            {
                targetWaypoints[i].GetComponent<TargetModelBehavior>().PrevShadowId =
                    targetWaypoints[ListIndex[sourceWaypoints[i].GetComponent<TargetModelBehavior>().PrevShadowId]]
                    .GetComponent<TargetModelBehavior>().SID;
            }
            if (sourceWaypoints[i].GetComponent<TargetModelBehavior>().NextShadowId != "")
            {
                targetWaypoints[i].GetComponent<TargetModelBehavior>().NextShadowId =
                    targetWaypoints[ListIndex[sourceWaypoints[i].GetComponent<TargetModelBehavior>().NextShadowId]]
                    .GetComponent<TargetModelBehavior>().SID;
            } 
            targetWaypoints[i].GetComponent<TargetModelBehavior>().RightOpen = sourceWaypoints[i].GetComponent<TargetModelBehavior>().RightOpen;
            targetWaypoints[i].GetComponent<TargetModelBehavior>().LeftOpen = sourceWaypoints[i].GetComponent<TargetModelBehavior>().LeftOpen;
            targetWaypoints[i].GetComponent<TargetModelBehavior>().IsShadow = sourceWaypoints[i].GetComponent<TargetModelBehavior>().IsShadow;
        }

        // OutOfBounds = new Dictionary<string, bool>();
        for(int i = 0; i < sourceWaypoints.Count; i++)
        {
            GameObject.Find("dummy").GetComponent<Group>().SIDToObj
                .Add(targetWaypoints[i].GetComponent<TargetModelBehavior>().SID, targetWaypoints[i]);
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

        //GameObject.Find("dummy").GetComponent<Group>().SIDToObj = SIDToObj;
        //GameObject.Find("dummy").GetComponent<Group>().OutOfBounds = OutOfBounds;
        GameObject.Find("dummy").GetComponent<Group>().NumPoints = NumPoints;
        GameObject.Find("dummy").GetComponent<Group>().HideGroup();
        GameObject.Find("dummy").name = "Button";
    }

    public void OnNavigationStarted(NavigationEventData eventData)
    {
        if (!eventData.used)
        {
            Debug.Log("MY GID IS: " + GID);
            Debug.Log("called");
            if (InTrainingQueue)
            {
                CopyButton = Instantiate(gameObject.GetComponent<Button>());
                CopyButton.name = "dummy";
                var colors = GroupButton.colors;
                colors.normalColor = new Color(0.94f, 0.65f, 0.93f);
                CopyButton.colors = colors;
                CopyButton.transform.parent = GameObject.Find("Slot" + (StagingManager.GroupButtonList.Count - 1).ToString()).transform;
                CopyButton.transform.localPosition = new Vector3(0.0f, 0.0f, 0.0f);
                CopyButton.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
                CopyButton.GetComponentInChildren<Text>().text = "Group" + (StagingManager.GroupButtonList.Count - 1).ToString();
            }
            eventData.Use();
        }
    }

    public void OnNavigationUpdated(NavigationEventData eventData){}

    public void OnNavigationCompleted(NavigationEventData eventData)
    {
        Debug.Log("I am here");
        Debug.Log(eventData.used);
        if (!eventData.used)
        {
            Debug.Log("MY GID IS: " + GID);
            if (InTrainingQueue)
            {
                GameObject argMin = null;
                float min = Single.PositiveInfinity;
                for (int i = 10; i < 20; i++)
                {
                    float distToSlot = Vector3.Distance(GameObject.Find("Slot" + i.ToString()).transform.position, gameObject.transform.position);
                    // Debug.Log(distToSlot);
                    if (distToSlot < 0.6f && distToSlot < min)
                    {
                        min = distToSlot;
                        argMin = GameObject.Find("Slot" + i.ToString());
                    }
                }
                if (argMin != null)
                {
                    Debug.Log("jfkdjfkdjfkd");
                    Debug.Log(min);
                    InTrainingQueue = false;
                    gameObject.transform.parent = argMin.transform;
                    gameObject.transform.localPosition = new Vector3(0.0f, 0.0f, 0.0f);
                    gameObject.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
                    // TODO: make the copied stuff real and bonified
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
                Debug.Log("jdhfjkdshfjkdshfjkdshfjkdshfkjdshfkjdshf");
                GameObject argMin = null;
                float min = Single.PositiveInfinity;
                for (int i = 10; i < 20; i++)
                {
                    float distToSlot = Vector3.Distance(GameObject.Find("Slot" + i.ToString()).transform.position, gameObject.transform.position);
                    Debug.Log(distToSlot);

                    if (distToSlot < 0.6f && distToSlot < min)
                    {
                        min = distToSlot;
                        argMin = GameObject.Find("Slot" + i.ToString());
                    }
                }
                Debug.Log(argMin);
                if (argMin != null && argMin.transform.childCount == 0)
                {
                    gameObject.transform.parent = argMin.transform;
                    gameObject.transform.localPosition = new Vector3(0.0f, 0.0f, 0.0f);
                    gameObject.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
                }
                else
                {
                    gameObject.transform.localPosition = new Vector3(0.0f, 0.0f, 0.0f);
                }
            }
            eventData.Use();
        }  
    }

    public void OnNavigationCanceled(NavigationEventData eventData)
    {
        this.OnNavigationCompleted(eventData);
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
