using UnityEngine;
using HoloToolkit.Unity.InputModule;
using RosSharp.RosBridgeClient;

public class SpeechHandler : MonoBehaviour, ISpeechHandler {

    public MoveItGoalPublisher MoveItGoalPublisher;

    void ISpeechHandler.OnSpeechKeywordRecognized(SpeechEventData eventData) {
        switch (eventData.RecognizedText.ToLower()) {
            case "plan":
                break;
        }
    }

    public void DoAction() {
        // TODO: Action
        Debug.Log("Plan in action");
        MoveItGoalPublisher.Publish();
    }
}