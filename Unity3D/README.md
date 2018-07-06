# [Unity3D](https://github.com/siemens/ros-sharp/tree/master/Unity3D) #
[Unity](https://unity3d.com/) project providing Unity-specific extensions to
* [RosBridgeClient](https://github.com/siemens/ros-sharp/tree/master/RosBridgeClient)
* [UrdfImporter](https://github.com/siemens/ros-sharp/tree/master/UrdfImporter)

__Please see the [Wiki](https://github.com/siemens/ros-sharp/wiki) for further info.__

---

© Siemens AG, 2017-2018

Author: Dr. Martin Bischoff (martin.bischoff@siemens.com)
------------------------------------------------------------------------------------------

Welcome! This Unity Project uses ROS# to create a Virtual Reality interface to control a 
Kinova Robotics MOVO.

Instructions for streaming Audio

On MOVO computer
If you haven't already, install ffmpeg and then run the following command
ffmpeg -f alsa -acodec pcm_s32le -ac 4 -ar 16000 -i hw:2,0 -f rtp rtp://234.5.5.5:1234

On Unity computer
Download and install VLC Media Player, then open the MovoSound.sdp file (which is in the 
topmost directory of this branch) with VLC Media Player.
