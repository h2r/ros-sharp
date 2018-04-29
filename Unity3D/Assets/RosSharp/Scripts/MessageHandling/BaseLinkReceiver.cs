using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RosSharp.RosBridgeClient {
    public class BaseLinkReceiver : MessageReceiver {
        public override Type MessageType { get { return (typeof(StandardString)); } }
        private StandardString message;
        private BasePosRotWriter baseUpdater;

        private void Awake() {
            MessageReception += ReceiveMessage;
        }

        private void Start() {
            baseUpdater = new BasePosRotWriter();
        }

        private void ReceiveMessage(object sender, MessageEventArgs e) {
            message = (StandardString)e.Message;
            //Debug.Log(message.data);
            baseUpdater.Write(message.data);
        }
    }
}
