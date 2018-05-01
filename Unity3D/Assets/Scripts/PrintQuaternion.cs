using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrintQuaternion : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        string s = string.Format("{0} {1} {2} {3}", transform.rotation.x, transform.rotation.y, transform.rotation.z, transform.rotation.w);
        Debug.Log(s);
    }
}
