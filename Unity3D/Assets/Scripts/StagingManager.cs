using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using RosSharp.RosBridgeClient;

public class StagingManager : MonoBehaviour {

    public MoveItGoalPublisher MoveItGoalPublisher;
    public DisplayTrajectoryReceiver DisplayTrajectoryReceiver;

    public Button GroupButton, GripperButton;
    public Button FirstPoseButton;
    public GameObject FirstGripperEver;

    public List<Button> GroupButtonList;
    public Button currentGroupButton;
    
    

    void Start()
    {
        Button visualsButton = GroupButton.GetComponent<Button>();
        Button execButton = GripperButton.GetComponent<Button>();
        visualsButton.onClick.AddListener(NewGroupClicked);
        execButton.onClick.AddListener(NewGripperClicked);

        this.GroupInit(true);
    }

    void NewGroupClicked()
    {
        Debug.Log("hfjdhfjdhf");
        if (GroupButtonList.Count < 10)
        {
            Debug.Log("here");
            currentGroupButton.GetComponent<Group>().HideGroup();
            this.GroupInit(false);
        }
        
    }

    private void GroupInit(bool firstGroup)
    {
        GameObject groupFirstWaypoint;
        
        if (firstGroup) {
            currentGroupButton = FirstPoseButton;
            groupFirstWaypoint = FirstGripperEver;
        } else
        {
            currentGroupButton = Instantiate(FirstPoseButton);
            currentGroupButton.transform.parent = GameObject.Find("Slot" + GroupButtonList.Count.ToString()).transform;
            currentGroupButton.transform.localPosition = new Vector3(0.0f, 0.0f, 0.0f);
            currentGroupButton.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
            currentGroupButton.GetComponentInChildren<Text>().text = "Group" + GroupButtonList.Count.ToString();


            groupFirstWaypoint = Instantiate(FirstGripperEver);
            groupFirstWaypoint.SetActive(true);
            Debug.Log(groupFirstWaypoint.transform.position);
        }
        string gid = currentGroupButton.GetComponent<Group>().GID;
        groupFirstWaypoint.GetComponent<TargetModelBehavior>().Init(gid, "START", null);


        string firstWaypointSID = groupFirstWaypoint.GetComponent<TargetModelBehavior>().SID;

        IdGenerator.Instance.GIDtoGroup[gid].StagingManager = this;
        IdGenerator.Instance.GIDtoGroup[gid].FirstWaypoint = groupFirstWaypoint;
        IdGenerator.Instance.GIDtoGroup[gid].SIDToObj.Add(firstWaypointSID, currentGroupButton.GetComponent<Group>().FirstWaypoint);
        IdGenerator.Instance.GIDtoGroup[gid].OutOfBounds.Add(firstWaypointSID, false);
        IdGenerator.Instance.GIDtoGroup[gid].LastSmartGripper = IdGenerator.Instance.GIDtoGroup[gid].FirstWaypoint;

        GroupButtonList.Add(currentGroupButton);
        IdGenerator.Instance.GIDtoGroup[gid].ShowGroup();
    }

    void NewGripperClicked()
    {
        // make a new target model
        string gid = currentGroupButton.GetComponent<Group>().GID;
        IdGenerator.Instance.GIDtoGroup[gid].AddGripper();
    }

    public void ChangeCurrentGroup(Button b)
    {
        currentGroupButton.GetComponent<Group>().HideGroup();
        currentGroupButton = b;
        currentGroupButton.GetComponent<Group>().ShowGroup();
    }
}
