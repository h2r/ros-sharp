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
            Debug.Log("gjfdkghjkfdhgjkfdhgjkfdhgk");
            StandardString message = (StandardString)e.Message;
            
            string[] payload = message.data.Split(' '); // payload[0] is the name of the gripper, [1] is status

            //if (IdGenerator.Instance.OutOfBounds[payload[0]]
            //    && payload[1] == "SUCCESS") // case where we are back in bounds
            //{
            //    IdGenerator.Instance.SetInBounds(payload[0]);
            //}
            //else if(!IdGenerator.Instance.OutOfBounds[payload[0]]
            //    && payload[1] == "FAIL") // case where we are now out of bounds
            //{
            //    IdGenerator.Instance.SetOutOfBounds(payload[0]);
            //}

        }

        // R:144 G:242 B:209 A:255
    }
}
