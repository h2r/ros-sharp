using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Collections;

namespace RosSharp.RosBridgeClient {

    public class DisplayTrajectoryReceiver : MessageReceiver {

        public override Type MessageType { get { return (typeof(MoveItDisplayTrajectory)); } }

        public StagingManager StagingManager;
        public GameObject UrdfModel; // baxter
        public JointStateWriter[] JointStateWriters;
        public Dictionary<string, JointStateWriter> JointDict = new Dictionary<string, JointStateWriter>();
        private List<GameObject> TrailPoints;

        //public Boolean visualizationFinished = false;
        public Boolean loop = false;
        public Boolean trail = false;

        public Boolean color = false;
        public Boolean sampling = false;
        private Boolean prev_color;
        private Boolean started = false;

        //private Boolean new_trajectory = false;

        public Color TrailColor = Color.magenta;

        public MoveItDisplayTrajectory message;
        public Queue<MoveItDisplayTrajectory> messageQueue;

        private void Awake() {
            messageQueue = new Queue<MoveItDisplayTrajectory>();
            MessageReception += ReceiveMessage;
        }

        private void Start() {
            foreach (JointStateWriter jsw in JointStateWriters) {
            //for(int i = 0; i < 10; i++) {
                //JointStateWriter jsw = JointStateWriters[i];
                string name = jsw.name.Split(new char[] { ':' })[1];
                //Debug.Log(name);
                name = name.Substring(1, name.Length - 2);
                JointDict.Add(name, jsw);
            }
            TrailPoints = new List<GameObject>();
            prev_color = color;
        }

        private void Update() {
            //if (!started && messageQueue.Count > 0)
            //{
            //    Animate();
            //}

            if (messageQueue.Count != 0 && !started)
            {
                started = true;
                //new_trajectory = false;
                DestroyTrail();
                //visualizationFinished = false;
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
            Debug.Log("CALLED!!!!!!");
            message = (MoveItDisplayTrajectory)e.Message;
            Debug.Log(message.model_id);
            //new_trajectory = true;
            messageQueue.Enqueue(message);
        }

        IEnumerator Animate() {
            started = true;
            int count = 0;
            while (messageQueue.Count != 0)
            {
                if (StagingManager.VisualGids.Count > 0) {
                    StagingManager.ChangeCurrentGroup(IdGenerator.Instance.GIDtoGroup[StagingManager.VisualGids[count]].GroupButton);
                }
                count++;
                MoveItDisplayTrajectory message = messageQueue.Dequeue();
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
                        //Debug.Log(start_names[i]);
                        JointDict[start_names[i]].WriteUpdate();
                    }
                }

                string[] joint_names = message.trajectory[0].joint_trajectory.joint_names;
                TrajectoryJointTrajectoryPoint[] points = message.trajectory[0].joint_trajectory.points;

                if (TrailPoints.Count < points.Length) {
                    DestroyTrail();
                }

                //if (sampling)
                //{
                //    int[] samplePoints = new int[IdGenerator.Instance.NumPoints + 2];
                //    samplePoints[0] = 0;
                //    samplePoints[samplePoints.Length - 1] = points.Length - 1;
                //}

                for (int i = 0; i < points.Length; i += 10)
                {
                    for (int j = 0; j < joint_names.Length; j++)
                    {
                        if (JointDict.ContainsKey(joint_names[j]))
                        {
                            JointDict[joint_names[j]].Write(points[i].positions[j]);
                            JointDict[joint_names[j]].WriteUpdate();
                        }
                    }

                    if (trail)
                    {
                        AddTrailPoint(i);
                    }
                    if (i == 0)
                    {
                        ColorTrailPoint(i);
                    }
                    //if (trail && color)
                    //{
                    //    ColorTrailPoint(i);
                    //}
                    yield return new WaitForSeconds(.5f);
                }
                for (int j = 0; j < joint_names.Length; j++)
                {
                    if (JointDict.ContainsKey(joint_names[j]))
                    {
                        JointDict[joint_names[j]].Write(points[points.Length - 1].positions[j]);
                        JointDict[joint_names[j]].WriteUpdate();
                    }
                }

                if (trail)
                {
                    AddTrailPoint(points.Length - 1);
                }
            }

            //visualizationFinished = true;
            StagingManager.VisualGids.Clear();
            DestroyTrail();
            started = false;
        }

        void AddTrailPoint(int point_index) {
            if (point_index < TrailPoints.Count) {
                TrailPoints[point_index].SetActive(true);
            } else {
                GameObject clone = Instantiate(UrdfModel, UrdfModel.transform.position, UrdfModel.transform.rotation);
                Destroy(clone.gameObject.GetComponentInChildren<Canvas>().gameObject);
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
