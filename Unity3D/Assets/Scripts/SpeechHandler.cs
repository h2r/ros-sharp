using UnityEngine;
using HoloToolkit.Unity.InputModule;
using RosSharp.RosBridgeClient;

public class SpeechHandler : MonoBehaviour, ISpeechHandler {

    public MoveItGoalPublisher MoveItGoalPublisher;
    public DisplayTrajectoryReceiver DisplayTrajectoryReceiver;

    
    void ISpeechHandler.OnSpeechKeywordRecognized(SpeechEventData eventData) {
    }

    // Sends the goal position to MoveIt
    public void Plan() {
        Debug.Log("Plan");
        MoveItGoalPublisher.PublishPlan();
    }

    // Tells MoveIt to execute the plan
    public void Move() {
        Debug.Log("Execute");
        MoveItGoalPublisher.PublishMove(); // move the arm
        DisplayTrajectoryReceiver.loop = false; // stop the visualization
    }
}