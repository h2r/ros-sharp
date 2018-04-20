/*
© Siemens AG, 2017-2018
Author: Dr. Martin Bischoff (martin.bischoff@siemens.com)

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at
<http://www.apache.org/licenses/LICENSE-2.0>.
Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.
*/

using UnityEngine;
using UnityEditor;

namespace RosSharp.RosBridgeClient {
    [CustomEditor(typeof(MoveItUrdfPatcher))]
    public class MoveItUrdfPatcherEditor : Editor {
        private MoveItUrdfPatcher moveitUrdfPatcher;

        public override void OnInspectorGUI() {
            moveitUrdfPatcher = (MoveItUrdfPatcher)target;

            //DrawDefaultInspector();

            moveitUrdfPatcher.UrdfModel = (GameObject)EditorGUILayout.ObjectField("Urdf Model", moveitUrdfPatcher.UrdfModel, typeof(GameObject), true);
            GUILayout.Space(10);
            moveitUrdfPatcher.EnableRigidbodiesGravity = GUILayout.Toggle(moveitUrdfPatcher.EnableRigidbodiesGravity, "Enable Gravity for Rigidbodies");
            moveitUrdfPatcher.SetRigidbodiesKinematic = GUILayout.Toggle(moveitUrdfPatcher.SetRigidbodiesKinematic, "Set Rigidbodies Kinematic");
            moveitUrdfPatcher.SetMeshCollidersConvex = GUILayout.Toggle(moveitUrdfPatcher.SetMeshCollidersConvex, "Set Mesh Colliders Convex");
            GUILayout.Space(10);
            moveitUrdfPatcher.AddPoseProvider = GUILayout.Toggle(moveitUrdfPatcher.AddPoseProvider, "Publish Pose (Add Pose Provider)");
            moveitUrdfPatcher.AddPoseReceiver = GUILayout.Toggle(moveitUrdfPatcher.AddPoseReceiver, "Subscribe Pose (Add Pose Receiver)");

            GUILayout.Space(10);
            moveitUrdfPatcher.AddJointStateReaders = GUILayout.Toggle(moveitUrdfPatcher.AddJointStateReaders, "Publish Joint States (Add Joint State Readers)");
            if (moveitUrdfPatcher.AddJointStateReaders)
                moveitUrdfPatcher.jointStateProvider = (JointStateProvider)EditorGUILayout.ObjectField("Joint State Provider", moveitUrdfPatcher.jointStateProvider, typeof(JointStateProvider), true);

            moveitUrdfPatcher.AddJointStateWriters = GUILayout.Toggle(moveitUrdfPatcher.AddJointStateWriters, "Subscribe Display Trajectory (Add Joint State Writers)");
            if (moveitUrdfPatcher.AddJointStateWriters)
                moveitUrdfPatcher.displayTrajectoryReceiver = (DisplayTrajectoryReceiver)EditorGUILayout.ObjectField("Display Trajectory Receiver", moveitUrdfPatcher.displayTrajectoryReceiver, typeof(DisplayTrajectoryReceiver), true);

            GUILayout.Space(10);
            if (GUILayout.Button("Apply"))
                moveitUrdfPatcher.Patch();
        }
    }
}