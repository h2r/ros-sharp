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
            
            string[] payload = message.data.Split(' ');
            
   
            //if (MoveItGoalPublisher
            //    .LastManipulatedGripper.GetComponent<TargetModelBehavior>().OutOfBounds && payload[1] == "SUCCESS") // case where we are back in bounds
            //{
            //    GetComponent<RosConnector>().GetComponent<MoveItGoalPublisher>()
            //    .LastManipulatedGripper.GetComponent<Renderer>().material.color = Color.blue;
            //    MoveItGoalPublisher
            //    .LastManipulatedGripper.GetComponent<TargetModelBehavior>().OutOfBounds = false;
            //} else if (!MoveItGoalPublisher
            //    .LastManipulatedGripper.GetComponent<TargetModelBehavior>().OutOfBounds && payload[1] == "FAIL") // case where we are now out of bounds
            //{
            //    Debug.Log("Should be here");
            //    GetComponent<RosConnector>().GetComponent<MoveItGoalPublisher>()
            //    .LastManipulatedGripper.GetComponent<Renderer>().material.color = Color.red;
            //    MoveItGoalPublisher
            //    .LastManipulatedGripper.GetComponent<TargetModelBehavior>().OutOfBounds = true;
            //} 

        }

        // R:144 G:242 B:209 A:255
    }
}
