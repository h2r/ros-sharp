using UnityEngine;

namespace RosSharp.RosBridgeClient {
    public class BasePosRotWriter : MonoBehaviour {

        private string newState; // deg or m
        private bool isNewStateReceived;
        public GameObject base_link;
        public float scale = 1f;

        private void Start() {
            base_link = GameObject.Find("base_link");
        }

        private void Update() {
            if (isNewStateReceived) {
                string message = newState; //get newest robot state data (from transform)
                string[] tmp = message.Split('^'); //seperate position from rotation data
                string pos = tmp[0]; //position data
                string rot = tmp[1]; //rotation data
                pos = pos.Substring(1, pos.Length - 2);
                rot = rot.Substring(1, rot.Length - 2);

                string[] poses = pos.Split(',');
                
                float pos_x = float.Parse(poses[0]); //x position
                float pos_y = float.Parse(poses[1]); //y position
                float pos_z = float.Parse(poses[2]); //z position

                Vector3 curPos = new Vector3(pos_x, pos_y, pos_z); //save current position
                string[] rots = rot.Split(',');
                char[] toTrim = { ']' };

                //save rotation as quaternions
                float rot_x = float.Parse(rots[0]);
                float rot_y = float.Parse(rots[1]);
                float rot_z = float.Parse(rots[2]);
                float rot_w = float.Parse(rots[3].TrimEnd(toTrim));

                Quaternion curRot = new Quaternion(rot_x, rot_y, rot_z, rot_w);
                base_link.transform.position = scale * RosToUnityPositionAxisConversion(curPos); //convert ROS coordinates to Unity coordinates and scale for position vector
                base_link.transform.rotation = RosToUnityQuaternionConversion(curRot); //convert ROS quaternions to Unity quarternions
                base_link.transform.localScale = new Vector3(scale, scale, scale);

                        
                    }
            isNewStateReceived = false;
        }
                

        //convert ROS position to Unity Position
        Vector3 RosToUnityPositionAxisConversion(Vector3 rosIn) {
            return new Vector3(-rosIn.x, rosIn.z, -rosIn.y);
        }

        //Convert ROS quaternion to Unity Quaternion
        Quaternion RosToUnityQuaternionConversion(Quaternion rosIn) {
            return new Quaternion(rosIn.x, -rosIn.z, rosIn.y, rosIn.w);
        }

        public void Write(string state) {
            newState = state;
            isNewStateReceived = true;
            Start();
            Update();
        }
    }
}