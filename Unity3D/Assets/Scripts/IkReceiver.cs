using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Collections;

namespace RosSharp.RosBridgeClient
{
    public class IkReceiver : MessageReceiver
    {

        public override Type MessageType { get { return (typeof(StandardString)); } }

        private void Awake()
        {
            MessageReception += ReceiveMessage;
        }

        // Use this for initialization
        void Start()
        {
            
        }

        // Update is called once per frame
        void Update()
        {

        }

        private void ReceiveMessage(object sender, MessageEventArgs e)
        {
            StandardString message = (StandardString)e.Message;
            if(message.data == "f")
            {

            }

        }

        // R:144 G:242 B:209 A:255
    }
}
