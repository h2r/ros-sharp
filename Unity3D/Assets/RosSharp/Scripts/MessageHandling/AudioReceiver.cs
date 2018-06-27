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

namespace RosSharp.RosBridgeClient {
    [RequireComponent(typeof(AudioSource))]
    public class AudioReceiver : MessageReceiver {
        public override Type MessageType { get { return (typeof(SensorAudio)); } }

        // The virtual model of the robot to be the source of the sound
        public GameObject robot;

        private byte[] audioData;
        private float[] scaledAudio;
        private bool isMessageReceived;

        private void Awake() {
            MessageReception += ReceiveMessage;
        }
        private void Start() {

        }
        private void Update() {
            if (isMessageReceived)
                ProcessMessage();
        }
        private void ReceiveMessage(object sender, MessageEventArgs e) {
            audioData = ((SensorAudio)e.Message).data;
            isMessageReceived = true;
        }

        private void ProcessMessage() {
            scaledAudio = ConvertByteToFloat16(audioData);
            AudioClip audioClip = AudioClip.Create("RobotAudio", scaledAudio.Length, 1, 16000, false);
            audioClip.SetData(scaledAudio, 0);
            AudioSource.PlayClipAtPoint(audioClip, robot.transform.position);
            isMessageReceived = false;
        }

        private float[] ConvertByteToFloat16(byte[] array) {
            float[] floatArr = new float[array.Length / 2];
            for (int i = 0; i < floatArr.Length; i++) {
                if (BitConverter.IsLittleEndian) {
                    Array.Reverse(array, i * 2, 2);
                }
                floatArr[i] = (float) (BitConverter.ToInt16(array, i * 2) / 32767f);
            }
            return floatArr;
        }


        private float[] ConvertByteToFloat(byte[] array) {
            float[] floatArr = new float[array.Length / 4];
            for (int i = 0; i < floatArr.Length; i++) {
                if (BitConverter.IsLittleEndian) {
                    Array.Reverse(array, i * 4, 4);
                }
                floatArr[i] = BitConverter.ToSingle(array, i * 4) / 0x80000000;
            }
            return floatArr;
        }

        private float[] Convert16BitByteArrayToAudioClipData(byte[] source) {

            int wavSize = BitConverter.ToInt32(source, 0);
            int x = sizeof(Int16); // block size = 2
            int convertedSize = wavSize / x;

            float[] data = new float[convertedSize];

            Int16 maxValue = Int16.MaxValue;

            int offset = 0;
            int i = 0;
            Debug.Log(convertedSize);
            while (i < convertedSize) {
                offset = i * x;
                Debug.Log(source[offset]);
                data[i] = (float)BitConverter.ToInt16(source, offset) / maxValue;
                i = i+1;
            }

            return data;
        }

    }
}


