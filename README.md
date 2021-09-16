# Unity-OpenVINO-YOLOX
Testing a native plugin for the [YOLOX](https://github.com/Megvii-BaseDetection/YOLOX) object detection model for Unity that leverages the OpenVINO™ Toolkit.

![OpenVINO_YOLOX_Plugin_Demo](https://raw.githubusercontent.com/cj-mills/Unity-OpenVINO-YOLOX/main/images/OpenVINO_YOLOX_Plugin_Demo.gif)


## Demo Video
* [OpenVINO YOLOX Unity Plugin Demo](https://www.youtube.com/watch?v=opClIrHumzI)



## Download Models

1. Download the [`models`](https://drive.google.com/drive/folders/19Pzuo_Hsr0eQDs8COxKjMrosoVyw2WWW?usp=sharing) folder from Google Drive. ([link](https://drive.google.com/drive/folders/19Pzuo_Hsr0eQDs8COxKjMrosoVyw2WWW?usp=sharing))

   ![google-drive-download-models-folder](https://raw.githubusercontent.com/cj-mills/Unity-OpenVINO-YOLOX/main/images/google-drive-download-models-folder.png)

2. Extract the `models` folder from the `.zip` file.

3. Copy and paste the `models` folder into the [`OpenVINO_YOLOX_Demo\Build`](https://github.com/cj-mills/Unity-OpenVINO-YOLOX/tree/main/OpenVINO_YOLOX_Demo/Build) folder.



## Run the Demo

1. Open the `OpenVINO_YOLOX_Demo\Build` folder.

2. Run the `OpenVINO_YOLOX_Demo.exe` file

   **Note:** The `yolo_m` model seems to provide the best balance between accuracy and performance.

3. You can press the space bar to hide the user interface.
