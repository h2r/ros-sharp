using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RosSharp.RosBridgeClient {

    [RequireComponent(typeof(UrdfPatcher))]
    public class UnityIdleAnimator : MonoBehaviour {

        public GameObject UrdfModel;

        Dictionary<Transform, JointStateHandler.JointTypes> jointTypeDictionary;
        private List<JoyAxisJointTransformWriter> JoyAxisJointTransformWriters = new List<JoyAxisJointTransformWriter>();


        // Use this for initialization
        void Start() {
            GetSingleDimensionalJoints();
            AddJoyAxisJointTransformWriters();
        }

        void SetRandomAngles() {
            foreach (JoyAxisJointTransformWriter j in JoyAxisJointTransformWriters) {
                float goal = Random.Range(j.GetLimit().x, j.GetLimit().y);
                StartCoroutine(MoveToAngle(j, goal));
            }
        }

        // Update is called once per frame
        void Update() {
            if(Input.GetKeyDown("a") || Input.GetKeyDown("joystick button 0") || Input.GetKeyDown("joystick button 2")) {
                StopAllCoroutines();
                SetRandomAngles();
            }
        }

        IEnumerator MoveToAngle(JoyAxisJointTransformWriter j, float goal) {
            while (Mathf.Abs(j.GetState() - goal) > 0.1) {
                j.Write(Mathf.LerpAngle(j.GetState(), goal, 0.005f));
                yield return null;
            }
        }

        private void GetSingleDimensionalJoints() {
            jointTypeDictionary = new Dictionary<Transform, JointStateHandler.JointTypes>();

            JointStateHandler.JointTypes jointType;

            foreach (Transform child in UrdfModel.GetComponentsInChildren<Transform>())
                if (HasSingleDimensionalJoint(child, out jointType))
                    jointTypeDictionary.Add(child, jointType);

        }

        private bool HasSingleDimensionalJoint(Transform child, out JointStateReader.JointTypes jointType) {
            jointType = JointStateHandler.JointTypes.continuous;

            if (child.name.Contains("continuous Joint"))
                jointType = JointStateHandler.JointTypes.continuous;
            else if (child.name.Contains("revolute Joint"))
                jointType = JointStateHandler.JointTypes.revolute;
            else if (child.name.Contains("prismatic Joint"))
                jointType = JointStateHandler.JointTypes.prismatic;
            else
                return false;

            return true;
        }

        public void AddJoyAxisJointTransformWriters() {
            foreach (KeyValuePair<Transform, JointStateHandler.JointTypes> jointTypeEntry in jointTypeDictionary) {
                JoyAxisJointTransformWriters.Add(jointTypeEntry.Key.gameObject.AddComponent<JoyAxisJointTransformWriter>());
                jointTypeEntry.Key.gameObject.GetComponent<JoyAxisJointTransformWriter>().DoApplyUnityJointLimits = true;
            }
        }
    }
}