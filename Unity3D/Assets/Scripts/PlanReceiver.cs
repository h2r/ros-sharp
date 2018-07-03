using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Collections;

namespace RosSharp.RosBridgeClient
{

    public class PlanReceiver : MessageReceiver
    {

        public override Type MessageType { get { return (typeof(MoveItDisplayTrajectory)); } }

        public GameObject UrdfModel; // baxter
        public int NumPoints;
        public JointStateWriter[] JointStateWriters;
        public Dictionary<string, JointStateWriter> JointDict = new Dictionary<string, JointStateWriter>();
        private List<GameObject> TrailPoints;


        public Boolean loop = false;
        public Boolean trail = false;

        public Boolean sampling = false;
        private Boolean new_trajectory = false;

        public Color TrailColor = Color.magenta;

        public MoveItDisplayTrajectory message;

        private void Awake()
        {
            MessageReception += ReceiveMessage;
        }

        private void Start()
        {
            NumPoints = 1;
            foreach (JointStateWriter jsw in JointStateWriters)
            {
                //for(int i = 0; i < 10; i++) {
                //JointStateWriter jsw = JointStateWriters[i];
                string name = jsw.name.Split(new char[] { ':' })[1];
                //Debug.Log(name);
                name = name.Substring(1, name.Length - 2);
                JointDict.Add(name, jsw);
            }
            TrailPoints = new List<GameObject>();
        }

        private void Update()
        {
            if (Input.GetKeyDown("f") || new_trajectory)
            {
                new_trajectory = false;
                DestroyTrail();
                StopCoroutine("Animate");
                StartCoroutine("Animate");
            }
        }

        private void DestroyTrail()
        {
            foreach (GameObject trailPoint in TrailPoints)
            {
                Destroy(trailPoint);
            }
            TrailPoints.Clear();
        }

        private void ReceiveMessage(object sender, MessageEventArgs e)
        {
            Debug.Log("This should happen");
            message = (MoveItDisplayTrajectory)e.Message;
            new_trajectory = true;
        }

        IEnumerator Animate()
        {
            do
            {

                for (int i = 0; i < TrailPoints.Count; i++)
                {
                    TrailPoints[i].SetActive(false);
                }

                string[] joint_names = message.trajectory_start.joint_state.name;
                float[] start_positions = message.trajectory_start.joint_state.position;

                for (int i = 0; i < joint_names.Length; i++)
                {
                    if (JointDict.ContainsKey(joint_names[i]))
                    {
         
                        JointDict[joint_names[i]].Write(start_positions[i]);
                        Debug.Log(joint_names[i]);
                        JointDict[joint_names[i]].WriteUpdate();
                    }
                }

                //string[] joint_names = message.trajectory[0].joint_trajectory.joint_names;
                TrajectoryJointTrajectoryPoint[] points = message.trajectory[0].joint_trajectory.points;
                Debug.Log(points.Length);
                if (TrailPoints.Count < points.Length)
                {
                    DestroyTrail();
                }


                if (sampling)
                {
                    int[] samplePoints = new int[NumPoints + 2];
                    samplePoints[0] = 0;
                    samplePoints[samplePoints.Length - 1] = points.Length - 1;
                }

                for (int i = 0; i < points.Length; i++)
                {
                    for (int j = 0; j < joint_names.Length; j++)
                    {
                        if (JointDict.ContainsKey(joint_names[j]))
                        {
                            JointDict[joint_names[j]].Write(points[i].positions[j]);
                            JointDict[joint_names[j]].WriteUpdate();
                        }
                    }
                    yield return new WaitForSeconds(.1f);
                }


            } while (loop);
        }

        void AddTrailPoint(int point_index)
        {
            if (point_index < TrailPoints.Count)
            {
                TrailPoints[point_index].SetActive(true);
            }
            else
            {
                GameObject clone = Instantiate(UrdfModel, UrdfModel.transform.position, UrdfModel.transform.rotation);
                clone.transform.localScale = new Vector3(1.01f, 1.01f, 1.01f);
                TrailPoints.Add(clone);
            }
        }

        void ColorTrailPoint(int point_index)
        {
            foreach (MeshRenderer mr in TrailPoints[point_index].GetComponentsInChildren<MeshRenderer>())
            {
                foreach (Material mat in mr.materials)
                {
                    mat.color = TrailColor;
                }
            }
        }
    }
}
