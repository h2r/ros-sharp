using UnityEngine;
using HoloToolkit.Unity.InputModule;
using RosSharp.RosBridgeClient;

public class SpeechHandler : MonoBehaviour, ISpeechHandler {

    public MoveItGoalPublisher MoveItGoalPublisher;
    public DisplayTrajectoryReceiver DisplayTrajectoryReceiver;
    
    
    void ISpeechHandler.OnSpeechKeywordRecognized(SpeechEventData eventData) {
    }

    // Sends the goal position to MoveIt
    //public void Plan() {
    //    Debug.Log("Plan");
    //    MoveItGoalPublisher.PublishPlan();
    //}

    public void New()
    {
        // make a new target model
        if (MoveItGoalPublisher.LastSmartGripper == null) return;

        // make a new smart gripper
        GameObject newObj = Instantiate(MoveItGoalPublisher.LastSmartGripper);
        Vector3 offset = new Vector3(0.1f, 0.0f, 0.0f);
        newObj.transform.position = newObj.transform.position + offset;
        newObj.GetComponent<TargetModelBehavior>().RightOpen = 
            MoveItGoalPublisher.LastSmartGripper.GetComponent<TargetModelBehavior>().RightOpen;

        // set the last smart gripper's next pointer to the newly instantiated obj's id
        MoveItGoalPublisher.LastSmartGripper.GetComponent<TargetModelBehavior>().NextId = 
            newObj.GetComponent<TargetModelBehavior>().Id;

        // set prev pointerS
        newObj.GetComponent<TargetModelBehavior>().PrevId =
            MoveItGoalPublisher.LastSmartGripper.GetComponent<TargetModelBehavior>().Id;

        // change the last smart gripper
        MoveItGoalPublisher.LastSmartGripper = newObj;
        MoveItGoalPublisher.LastSmartGripper.GetComponent<TargetModelBehavior>().SendPlanRequest();
        DisplayTrajectoryReceiver.NumPoints++;
    }

    // Tells MoveIt to execute the plan
    public void Move() {
        Debug.Log("Move");
        MoveItGoalPublisher.PublishMove(); // move the arm
        DisplayTrajectoryReceiver.loop = false; // stop the visualization
    }

    public void Begin()
    {
        Debug.Log("Start");
    }

    public void Stop()
    {
        Debug.Log("Stop");
    }

    public void Next()
    {
        Debug.Log("Next");
    }

    public void Open()
    {
        Debug.Log("Open");
    }

    public void Close()
    {
        Debug.Log("Close");
    }
}