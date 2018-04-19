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

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RosSharp.RosBridgeClient
{
    public class JointStateReceiver : MessageReceiver
    {
        public override Type MessageType { get { return (typeof(SensorJointStates)); } }

        public JointStateWriter[] JointStateWriters;
        public Dictionary<string, JointStateWriter> JointDict = new Dictionary<string, JointStateWriter>();

        private SensorJointStates message;

        private void Awake()
        {
            MessageReception += ReceiveMessage;
        }

        private void Start() {
            foreach(JointStateWriter jsw in JointStateWriters) {
                string name = jsw.name.Split(new char[] { ':' })[1];
                name = name.Substring(1, name.Length - 2);
                JointDict.Add(name, jsw);
            }
        }

        private void ReceiveMessage(object sender, MessageEventArgs e)
        {
            message = (SensorJointStates)e.Message;
            for (int i = 0; i < message.name.Length; i++) {
                if (JointDict.ContainsKey(message.name[i])) {
                    JointDict[message.name[i]].Write(message.position[i]);
                }
            }
        }
    }
}

