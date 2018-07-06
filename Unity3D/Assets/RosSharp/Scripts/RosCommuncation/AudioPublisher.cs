using RosSharp.RosBridgeClient;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


// The class for this script extends the Publisher class of ROS# which was authored by Dr. Martin Bischoff
public class AudioPublisher : Publisher
{

    private SensorAudio message;
    private AudioClip audioRecording;
    private String mic;

    // Used for initialization
    protected override void Start()
    {
        rosSocket = GetComponent<RosConnector>().RosSocket;
        publicationId = rosSocket.Advertise(Topic, "audio_common_msgs/AudioData");
        message = new SensorAudio();
        InvokeRepeating("SendAudio", 1.5f, 1.5f);
    }

    void SendAudio()
    {
        mic = Microphone.devices[0];
        audioRecording = Microphone.Start(mic, false,1,16000);
        message.data = WavUtility.FromAudioClip(audioRecording);

        if (message.data != null)
        {
            Debug.Log(message.data[2]);
            rosSocket.Publish(publicationId, message);
        }
    }

}