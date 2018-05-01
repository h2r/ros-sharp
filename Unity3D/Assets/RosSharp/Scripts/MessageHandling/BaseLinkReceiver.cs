using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RosSharp.RosBridgeClient {
    public class BaseLinkReceiver : MessageReceiver {
        public override Type MessageType { get { return (typeof(StandardString)); } }
        private StandardString message;
        public GameObject base_link;
        public float scale = 1f;
        private Vector3 currPos;
        private Quaternion currRot;

        private void Awake() {
            MessageReception += ReceiveMessage;
        }

        private void Start() {
            base_link = GameObject.Find("base_link");
        }

        private void FixedUpdate() {
            base_link.transform.position = scale * RosToUnityPositionAxisConversion(currPos); //convert ROS coordinates to Unity coordinates and scale for position vector
            base_link.transform.rotation = RosToUnityQuaternionConversion(currRot); //convert ROS quaternions to Unity quarternions
            base_link.transform.localScale = new Vector3(scale, scale, scale);
        }

        private void ReceiveMessage(object sender, MessageEventArgs e) {
            message = (StandardString)e.Message;
            //Debug.Log(message.data);
            
            string[] tmp = message.data.Split('^'); //seperate position from rotation data
            string pos = tmp[0]; //position data
            string rot = tmp[1]; //rotation data
            pos = pos.Substring(1, pos.Length - 2);
            rot = rot.Substring(1, rot.Length - 2);

            string[] poses = pos.Split(',');

            float pos_x = float.Parse(poses[0]); //x position
            float pos_y = float.Parse(poses[1]); //y position
            float pos_z = float.Parse(poses[2]); //z position

            currPos = new Vector3(pos_x, pos_y, pos_z); //save current position
            string[] rots = rot.Split(',');
            char[] toTrim = { ']' };

            //save rotation as quaternions
            float rot_x = float.Parse(rots[0]);
            float rot_y = float.Parse(rots[1]);
            float rot_z = float.Parse(rots[2]);
            float rot_w = float.Parse(rots[3].TrimEnd(toTrim));

            currRot = new Quaternion(rot_x * Mathf.Deg2Rad, rot_y * Mathf.Deg2Rad, rot_z * Mathf.Deg2Rad, rot_w * Mathf.Deg2Rad);

        }
        //convert ROS position to Unity Position
        Vector3 RosToUnityPositionAxisConversion(Vector3 rosIn) {
            return new Vector3(-rosIn.x, rosIn.z, -rosIn.y);
        }

        //Convert ROS quaternion to Unity Quaternion
        Quaternion RosToUnityQuaternionConversion(Quaternion rosIn) {
            //return new Quaternion(rosIn.x, -rosIn.z, rosIn.y, rosIn.w);
            return new Quaternion(rosIn.y, -rosIn.z, -rosIn.x, rosIn.w);
        }

    }
}
