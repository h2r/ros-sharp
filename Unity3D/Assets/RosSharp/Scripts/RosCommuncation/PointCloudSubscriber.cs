//using UnityEngine;

//namespace RosSharp.RosBridgeClient {
//    [RequireComponent(typeof(RosConnector))]
//    public class PointCloudSubscriber : MonoBehaviour {
//        public string colorTopic;
//        public string depthTopic;

//        private Message currentColorMessage;

//        public float TimeStep;
//        private int timeStep { get { return (int)(TimeStep * 1000); } } // the rate(in ms in between messages) at which to throttle the topics

//        public DepthAndColorGeometryView depthAndColorHandler;

//        private RosSocket rosSocket;

//        private void Start() {
//            rosSocket = GetComponent<RosConnector>().RosSocket;
//            rosSocket.Subscribe(colorTopic, MessageTypes.RosMessageType(typeof(SensorCompressedImage)), ReceiveColor, timeStep);
//            rosSocket.Subscribe(depthTopic, MessageTypes.RosMessageType(typeof(SensorImage)), ReceiveDepth, timeStep);
//        }

//        private void ReceiveColor(Message colorMessage) {
//            currentColorMessage = colorMessage;
//        }

//        private void ReceiveDepth(Message depthMessage) {
            

//        }
//    }
//}