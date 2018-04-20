using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Collections;

namespace RosSharp.RosBridgeClient {

    public class DisplayTrajectoryReceiver : MessageReceiver {

        public override Type MessageType { get { return (typeof(MoveItDisplayTrajectory)); } }

        public JointStateWriter[] JointStateWriters;
        public Dictionary<string, JointStateWriter> JointDict = new Dictionary<string, JointStateWriter>();

        public Boolean loop = false;
        public Boolean trail = false;
        public Boolean color = false;
        public Color trail_color = Color.magenta;

        public MoveItDisplayTrajectory message;

        private void Awake() {
            MessageReception += ReceiveMessage;
        }

        private void Start() {
            foreach (JointStateWriter jsw in JointStateWriters) {
                string name = jsw.name.Split(new char[] { ':' })[1];
                name = name.Substring(1, name.Length - 2);
                JointDict.Add(name, jsw);
            }
        }

        private void Update() {
            if (Input.GetKeyDown("f")) {
                StopCoroutine("Animate");
                StartCoroutine("Animate");
            }
        }

        private void ReceiveMessage(object sender, MessageEventArgs e) {
            message = (MoveItDisplayTrajectory)e.Message;
        }

        IEnumerator Animate() {
            do {
                foreach (GameObject clone in GameObject.FindGameObjectsWithTag("clone")) {
                    Destroy(clone);
                }

                string[] start_names = message.trajectory_start.joint_state.name;
                float[] start_positions = message.trajectory_start.joint_state.position;

                for (int i = 0; i < start_names.Length; i++) {
                    if (JointDict.ContainsKey(start_names[i])) {
                        JointDict[start_names[i]].Write(start_positions[i]);
                        JointDict[start_names[i]].WriteUpdate();
                    }
                }

                string[] joint_names = message.trajectory[0].joint_trajectory.joint_names;
                TrajectoryJointTrajectoryPoint[] points = message.trajectory[0].joint_trajectory.points;
                foreach (TrajectoryJointTrajectoryPoint point in points) {
                    for (int i = 0; i < joint_names.Length; i++) {
                        if (JointDict.ContainsKey(joint_names[i])) {
                            JointDict[joint_names[i]].Write(point.positions[i]);
                            JointDict[joint_names[i]].WriteUpdate();
                            if (trail) {
                                GameObject original = JointDict[joint_names[i]].gameObject;
                                GameObject clone = Instantiate(original, original.transform.position, original.transform.rotation);
                                clone.tag = "clone";
                                if (color) {
                                    foreach (MeshRenderer mr in clone.GetComponentsInChildren<MeshRenderer>()) {
                                        foreach (Material mat in mr.materials) {
                                            mat.color = trail_color;
                                        }
                                    }
                                }
                            }
                        }
                    }
                    yield return new WaitForSeconds(.1f);
                }
            } while (loop);
        }
    }

}
