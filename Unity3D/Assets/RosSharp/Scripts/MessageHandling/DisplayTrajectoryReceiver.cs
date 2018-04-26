using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Collections;

namespace RosSharp.RosBridgeClient {

    public class DisplayTrajectoryReceiver : MessageReceiver {

        public override Type MessageType { get { return (typeof(MoveItDisplayTrajectory)); } }

        public GameObject UrdfModel;

        public JointStateWriter[] JointStateWriters;
        public Dictionary<string, JointStateWriter> JointDict = new Dictionary<string, JointStateWriter>();
        private List<GameObject> TrailPoints;
        

        public Boolean loop = false;
        public Boolean trail = false;


        public Boolean color = false;
        private Boolean prev_color;

        private Boolean new_trajectory = false;

        public Color TrailColor = Color.magenta;

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
            TrailPoints = new List<GameObject>();
            prev_color = color;
        }

        private void Update() {
            
            if (Input.GetKeyDown("f") || new_trajectory) {
                new_trajectory = false;
                DestroyTrail();
                StopCoroutine("Animate");
                StartCoroutine("Animate");
            }
        }

        private void DestroyTrail() {
            foreach(GameObject trailPoint in TrailPoints) {
                Destroy(trailPoint);
            }
            TrailPoints.Clear();
        }

        private void ReceiveMessage(object sender, MessageEventArgs e) {
            message = (MoveItDisplayTrajectory)e.Message;
            new_trajectory = true;
        }

        IEnumerator Animate() {
            do {
                if (prev_color != color) {
                    prev_color = color;
                    DestroyTrail();
                }

                for (int i = 0; i < TrailPoints.Count; i++) {
                    TrailPoints[i].SetActive(false);
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

                if (TrailPoints.Count < points.Length) {
                    DestroyTrail();
                }

                for (int i = 0; i < points.Length; i++) {
                    for (int j = 0; j < joint_names.Length; j++) {
                        if (JointDict.ContainsKey(joint_names[j])) {
                            JointDict[joint_names[j]].Write(points[i].positions[j]);
                            JointDict[joint_names[j]].WriteUpdate();
                        }
                    }
                    if (trail) {
                        AddTrailPoint(i);
                    }
                    if (trail && color) {
                        ColorTrailPoint(i);
                    }
                    yield return new WaitForSeconds(.1f);
                }
            } while (loop);
        }

        void AddTrailPoint(int point_index) {
            if (point_index < TrailPoints.Count) {
                TrailPoints[point_index].SetActive(true);
            } else {
                GameObject clone = Instantiate(UrdfModel, UrdfModel.transform.position, UrdfModel.transform.rotation);
                clone.transform.localScale = new Vector3(1.01f, 1.01f, 1.01f);
                TrailPoints.Add(clone);
            }
        }


        void ColorTrailPoint(int point_index) {
            foreach (MeshRenderer mr in TrailPoints[point_index].GetComponentsInChildren<MeshRenderer>()) {
                foreach (Material mat in mr.materials) {
                    mat.color = TrailColor;
                }
            }
        }
        }

}
