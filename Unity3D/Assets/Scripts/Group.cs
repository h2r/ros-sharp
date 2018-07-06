using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using HoloToolkit.Unity;
using RosSharp.RosBridgeClient;

public class Group : MonoBehaviour
{
    public string GID; //group id
    public Dictionary<string, GameObject> SIDToObj { get; set; }
    public Dictionary<string, bool> OutOfBounds { get; set; }
    public GameObject FirstWaypoint { get; set; }
    public GameObject LastSmartGripper { get; set; }
    public int NumPoints { get; set; }
    public StagingManager StagingManager;
    public RunQueueManager RunQueueManager;
    public Button GroupButton;

    // Use this for initialization
    void Awake () {
        GID = IdGenerator.Instance.CreateGID(this);
        SIDToObj = new Dictionary<string, GameObject>();
        SIDToObj.Add("START", null);
        OutOfBounds = new Dictionary<string, bool>();
        NumPoints = 1;
        Button groupButton = GroupButton.GetComponent<Button>();
        groupButton.onClick.AddListener(HandleClick);
        // Note: Staging Manager initializes the rest of the fields
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

    public void HandleClick()
    {
        StagingManager.ChangeCurrentGroup(GroupButton);
    }
    
}
