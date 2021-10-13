# Unity OpenVINO YOLOX

![OpenVINO_YOLOX_Plugin_Demo](https://raw.githubusercontent.com/cj-mills/Unity-OpenVINO-YOLOX/main/images/OpenVINO_YOLOX_Plugin_Demo.gif)

This tutorial series covers how to run the [YOLOX](https://github.com/Megvii-BaseDetection/YOLOX) object detection model in the Unity game engine with the OpenVINO™ Toolkit.


## Demo Video
* [OpenVINO YOLOX Unity Plugin Demo](https://www.youtube.com/watch?v=opClIrHumzI)



## Download Models

1. Download the [`models`](https://drive.google.com/file/d/1N4GuHcKyBpDzJQ1r0LulzD3KRE3GRnAe/view?usp=sharing) folder from Google Drive. ([link](https://drive.google.com/file/d/1N4GuHcKyBpDzJQ1r0LulzD3KRE3GRnAe/view?usp=sharing))

2. Extract the `models` folder from the `.tar` file.

3. Copy and paste the `models` folder into the [`OpenVINO_YOLOX_Demo\Build`](https://github.com/cj-mills/Unity-OpenVINO-YOLOX/tree/main/OpenVINO_YOLOX_Demo/Build) folder.



## Run the Demo

1. Open the `OpenVINO_YOLOX_Demo\Build` folder.

2. Run the `OpenVINO_YOLOX_Demo.exe` file

   **Note:** The `yolo_m` model seems to provide the best balance between accuracy and performance.

3. You can press the space bar to hide the user interface.



## Tutorial Links


[Part 1](https://christianjmills.com/OpenVINO-Object-Detection-for-Unity-Tutorial-1/): This post covers the prerequisite software, pretrained object detection models, and test videos used in the tutorial.

[Part 2](https://christianjmills.com/OpenVINO-Object-Detection-for-Unity-Tutorial-2/): This post walks through the steps needed to create a Dynamic link library (DLL) in Visual Studio to perform inference with the pretrained deep learning model.

[Part 3](https://christianjmills.com/OpenVINO-Object-Detection-for-Unity-Tutorial-3/): This post demonstrates how to create a Unity project to access the DLL as a plugin.
