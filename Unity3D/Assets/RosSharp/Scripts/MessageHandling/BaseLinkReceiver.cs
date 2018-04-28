using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RosSharp.RosBridgeClient {
    public class BaseLinkReceiver : MessageReceiver {
        public override Type MessageType { get { return (typeof(StandardString)); } }

        //public Dictionary<string, JointStateWriter> JointDict = new Dictionary<string, JointStateWriter>();

        private StandardString message;

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

        private void ReceiveMessage(object sender, MessageEventArgs e) {
            message = (StandardString)e.Message;
            for (int i = 0; i < message.name.Length; i++) {
                if (JointDict.ContainsKey(message.name[i])) {
                    JointDict[message.name[i]].Write(message.position[i]);
                }
            }
        }
    }
}
