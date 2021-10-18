# Unity OpenVINO YOLOX In-Game Camera
![OpenVINO_YOLOX_Plugin_In-Game Camera](https://raw.githubusercontent.com/cj-mills/Unity-OpenVINO-YOLOX/in-game-camera/images/OpenVINO_YOLOX_Plugin_In-Game_Camera.gif)

This follow up tutorial covers a method to get input from the in-game camera, rather than a webcam or video feed. It also covers a method for using using the OpenVINO plugin directly in the Unity Editor.

## Download Models

1. Download the [`models`](https://drive.google.com/file/d/1N4GuHcKyBpDzJQ1r0LulzD3KRE3GRnAe/view?usp=sharing) folder from Google Drive. ([link](https://drive.google.com/file/d/1N4GuHcKyBpDzJQ1r0LulzD3KRE3GRnAe/view?usp=sharing))

2. Extract the `models` folder from the `.tar` file.

3. Copy and paste the `models` folder into the [`OpenVINO_YOLOX_Demo\Build`](https://github.com/cj-mills/Unity-OpenVINO-YOLOX/tree/main/OpenVINO_YOLOX_Demo/Build) folder.


**Note:**
GitHub breaks the large .bin files that contain the model weights, so follow the instructions in the README to download the models folder from GitHub and drop the extracted folder into the StreamingAssets folder in the Unity project. You will also likely need to rebuild the Unit.asset like for the Style Transfer version.



## Run the Demo

1. Open the `OpenVINO_YOLOX_Demo\Build` folder.

2. Run the `OpenVINO_YOLOX_Demo.exe` file

   **Note:** The `yolo_m` model seems to provide the best balance between accuracy and performance.

3. You can press the space bar to hide the user interface.



## Tutorial Links

