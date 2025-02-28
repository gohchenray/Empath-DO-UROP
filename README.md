# Empath-DO-UROP
This repository documents the work done on an undegraduate research project entitled Empath-Do, a virtual reality application for phone app developers to test accessibility features via a Meta Quest 3 headset. 

Empath-Do is an open-source augmented virtuality simulator that can simulate various impairments. The simulation allows designers to step into the shoes of individuals with sensory impairments and efficiently develop apps with accessibility features on smartphones. Currently, Empath-Do is developed in Unity for the Quest 3 using Meta's all in one SDK and uses an Intel Realsense D435i camera for depth data.  

The concept of Empath-Do is as follows:  
1. Hand tracking and simulated smartphone recreated in virtual environment which reflects real world interaction with smartphone
2. Sensory impairments such as visual (cataracts, glaucoma etc.) or auditory are overlaid in the virtual environment.
3. Developer can test out their smartphone app's accessibility by experiencing the impairment for themselves.

# Technical Breakdown
The project is a Unity app run via a Quest Link cable (USB-C) with a realsense D435i depth camera stream being fed into Unity in real time. The technical breakdown is as follows:  
1. Take realtime depth and colour data from depth camera, which is in the form of a pointcloud.
2. Using the a distance threshold filter, only the pointcloud for the user's hand and smartphone remains in the VR environment.
3. The pointcloud is further segmented using a colour filter that takes individual points HSV values, such that the pointcloud for the smartphone remains. This colour filter tracks human skin tones and culls them from the point cloud. Algorithm for RGB values are obtained from this research paper https://medium.com/swlh/human-skin-color-classification-using-the-threshold-classifier-rgb-ycbcr-hsv-python-code-d34d51febdf8
4. Display of the hand is done using Meta's provided hand tracking which is more robust and supported.
5. A canvas object containing a stream of the screen of the actual smartphone is projected onto the segmented pointcloud smartphone. The result is a recreation of the user's hands and smartphone in VR.
6. Sensorial impairments in the form of filters or events are overlaid onto the scene, simulating the impairment.
   
Currently, progress of the research is stuck at point (4). The following issues are obtained when using the colour filter:
- Low frame rate due to iteration over each point's rgb values which is then converted to HSV
- Inaccurate algorithm for segmenting human skin tones
- Setting colour data of pointcloud to 0 sets its colour to black instead of removing it. Potential need to reference back the corresponding depth pixel and remove it from the depth array.

# Methods tried to segment phone and hand
Some methods that have been tried but receive limited success are as follows:
- usage of capsule colliders instead of colour filters to segment pointcloud - complex capsule collider array needs to be made for the entire user's hand. Checking and iterating through each point to see if they are within bounds using collider.bounds.contains() unsuccessful.
- Checking of normals of points for smartphone - Was not able to obtain normals data from Unity depth camera feed. Only when mesh data is exported can normals be obtained.
- Changing of RGB values to HSV values as research paper indicated most important value is hue, using Color.RGBtoHSV function
  
Other possible methods that can be looked into: 
- Using Computer Vision to identify and track hand/phone segmentation
- Memory management and disposal of old frames
- Running project on a more powerful computer, parallel processing of depth data


# Requirements
Packages 
1. Meta XR All in One SDK https://assetstore.unity.com/packages/tools/integration/meta-xr-all-in-one-sdk-269657
2. Realsense SDK 2.0 https://github.com/IntelRealSense/librealsense/releases  
Follow the installation steps as per the user guides. For installing Realsense SDK, make sure test scenes provided can be run.  

# Attaching Realsense camera via headset mount   
As Meta does not allow access to the Quest 3's camera feed, an Intel Realsense D435i depth camera is attached to the front of the headset with a 3D printed mount (found in files) and image data streamed to Unity via Unity Link cable.   

# Setting up project
1. Download meta and realsense SDKs. Set up meta SDK to work in Unity according to documentation and guides - OVR camera rig and  hand tracking
   - https://www.youtube.com/watch?v=BU9LYKM2TDc
   - https://www.youtube.com/watch?v=rKDuYzS-D2E
2. Set up Realsense SDK in Unity project following one of the provided samples. The project should have these gameobjects:
  - Canvas. Render Mode: Screen Space - Camera
  - EventSystem
  - RsDevice. Attach RsDevice object to CenterEyeAnchor of OVRCameraRig
  - RsProcessingPipe  
Scripts for the GameObjects can be found in the meta realsense SDK files provided, basically copy the project settings of one of the other samples
3. Change the Profile under the RsProcssing Pipe to the Test profile found in this repository under ProcessingPipe
4. Attach the Depth Cutoff, Rs Colour Filter and Rs Point Cloud Processing blocks into the test profile. These profiles can be found in this repository under Scripts > ProcessingBlocks  
The script I use to edit is the Colour filter script as well as testing out modifications to the processing pipe.  
