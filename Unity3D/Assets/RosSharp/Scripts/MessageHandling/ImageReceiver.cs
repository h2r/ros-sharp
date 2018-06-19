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
using UnityEngine;

namespace RosSharp.RosBridgeClient
{
    [RequireComponent(typeof(MeshRenderer))]
    public class ImageReceiver : MessageReceiver
    {
        public override Type MessageType { get { return (typeof(SensorCompressedImage)); } }

        private byte[] imageData;
        private bool isMessageReceived;

        private MeshRenderer meshRenderer;
        private Texture2D texture2D;

        public float scale = 1.0f;

        private void Awake()
        {
            MessageReception += ReceiveMessage;
        }
        private void Start()
        {
            texture2D = new Texture2D(2,2);
            GetComponent<Renderer>().material.mainTexture = texture2D;

            //meshRenderer = GetComponent<MeshRenderer>();
            //meshRenderer.material = new Material(Shader.Find("Standard"));
        }
        private void Update()
        {
            if (isMessageReceived)
                ProcessMessage();
            gameObject.transform.localScale = new Vector3(16f * scale, scale, 9f *scale);
        }
        private void ReceiveMessage(object sender, MessageEventArgs e)
        {
            imageData = ((SensorCompressedImage)e.Message).data;
            isMessageReceived = true;
        }

        private void ProcessMessage()
        {
            texture2D.LoadImage(imageData);
            isMessageReceived = false;
        }
    }
}

