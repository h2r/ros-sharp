using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ResizeRobots : MonoBehaviour {

    public float scaleFactor = 1f;

    public Text text;

    // Update is called once per frame
    void Update () {
        if (Input.GetKey("joystick button 9")) {
            float x_pos = Input.GetAxisRaw("Right Horizontal");
            float result = Mathf.Lerp(0.99f, 1.01f, Mathf.InverseLerp(-1f, 1f, x_pos));
            foreach (Transform t in GetComponentsInChildren<Transform>()) {
                if (t.parent == this.transform) {
                    t.localScale = result * t.localScale;
                    text.text = "Current Scale: " + t.localScale.x.ToString();
                }
            }
            
        }
	}
}
