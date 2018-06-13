using HoloToolkit.Unity.InputModule;
using UnityEngine;

/// <summary>
/// GestureAction performs custom actions based on
/// which gesture is being performed.
/// </summary>
public class GestureAction : MonoBehaviour, INavigationHandler, IManipulationHandler
{
    [SerializeField]

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
        InputManager.Instance.PopModalInputHandler();
        // Debug.Log("hjfdhfjdhfjdhfjdhfj");
    }

    void INavigationHandler.OnNavigationCanceled(NavigationEventData eventData)
    {
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
        InputManager.Instance.PopModalInputHandler();
        Debug.Log("djugfkdhfkdjhf");
        Debug.Log(IdGenerator.Instance.CreateId());
    }

    void IManipulationHandler.OnManipulationCanceled(ManipulationEventData eventData)
    {
        InputManager.Instance.PopModalInputHandler();
    }
}