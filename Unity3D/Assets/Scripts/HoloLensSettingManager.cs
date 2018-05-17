using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

public class HoloLensSettingManager : MonoBehaviour {

	// Use this for initialization
	void Start () {
        if (XRDevice.SetTrackingSpaceType(TrackingSpaceType.RoomScale)) {
            Debug.Log("Roomscale tracking enabled");
        } else {
            Debug.LogError("Roomscale tracking failed");
        }
	}
}
