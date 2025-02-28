# Empath-DO-UROP
This repository documents the work done on an undegraduate research project entitled Empath-Do, a virtual reality application for phone app developers to test accessibility features via a Meta Quest 3 headset. 

# Features

# Methods tried
Other possible methods that have been brought up but not applied include 
- Using Computer Vision to identify and track hand/phone segmentation
- Memory management and disposal of old frames
- Running project on a more powerful computer

# Requirements
Packages 
1. Meta XR All in One SDK https://assetstore.unity.com/packages/tools/integration/meta-xr-all-in-one-sdk-269657
2. Realsense SDK 2.0 https://github.com/IntelRealSense/librealsense/releases  
Follow the installation steps as per the user guides. For installing Realsense SDK, make sure test scenes provided can be run.  

# Attaching Realsense camera via headset mount   
As Meta does not allow access to the Quest 3's camera feed, an Intel Realsense D435i depth camera is attached to the front of the headset with a 3D printed mount (found in files) and image data streamed to Unity via Unity Link cable.   
