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

namespace RosSharp.RosBridgeClient
{
    [RequireComponent(typeof(Joint)),RequireComponent(typeof(JointStateWriter))]
    public class JoyAxisJointTransformWriter : JoyAxisWriter
    {
        public bool DoApplyUnityJointLimits;

        private Joint joint;
        private JointStateWriter jointStateWriter;
        
        public float state;
        private Vector2 limit;

        public Vector2 GetLimit() {
            return limit;
        }

        public float GetState() {
            return state;
        }

        private void Start()
        {
            joint = GetComponent<Joint>();
            jointStateWriter = GetComponent<JointStateWriter>();

            SetLimit();
        }
        private void SetLimit()
        {
            if (jointStateWriter.JointType == JointStateWriter.JointTypes.continuous
                || jointStateWriter.JointType == JointStateWriter.JointTypes.revolute)
            {
                HingeJoint hingeJoint = (HingeJoint)joint;
                limit = new Vector2(hingeJoint.limits.min, hingeJoint.limits.max);
            }
            else if (jointStateWriter.JointType == JointStateWriter.JointTypes.prismatic)
            {
                ConfigurableJoint configurableJoint = (ConfigurableJoint)joint;
                limit = new Vector2(-configurableJoint.linearLimit.limit, 0f);
            }
        }

        private void ApplyLimits()
        {
            state = (state >= limit.x) ? state : limit.x;
            state = (state <= limit.y) ? state : limit.y;
            
        }

        public override void Write(float value)
        {
            state = value;
            if (DoApplyUnityJointLimits)
                ApplyLimits();

            if (jointStateWriter.JointType != JointStateWriter.JointTypes.prismatic) {
                jointStateWriter.Write(state * Mathf.Deg2Rad);
            } else {
                jointStateWriter.Write(state);
            }
        }
    }
}