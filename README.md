# Unity OpenVINO YOLOX In-Game Camera
![OpenVINO_YOLOX_Plugin_In-Game Camera](https://raw.githubusercontent.com/cj-mills/Unity-OpenVINO-YOLOX/in-game-camera/images/OpenVINO_YOLOX_Plugin_In-Game_Camera.gif)

This follow up tutorial covers a method to get input from the in-game camera, rather than a webcam or video feed. It also covers a method for using using the OpenVINO plugin directly in the Unity Editor.

**Warning:**
The large .bin files that contain the model weights get corrupted when downloading the repository as a `.zip`.

**Note:** You might get an error like the one below in Unity, if you download the project from GitHub. 

`AssetImporter is referencing an asset from the previous import. This should not happen.`

You can fix this issue by rebuilding the Unit asset. 
1. Open the Kinematica folder in the Assets section. 
2. Double-click on the `Unit` asset.
3. Click `Build` in the pop-up window. 
4. Close the pop-up window once the build is complete.
4. Repeat the above steps for the `Quadruped` asset.
5. Back in the `Assets` section, open the `Biped` scene in the `Scenes` folder.

The project should run normally now. However, there might be some stuttering the first time it is run.





## Tutorial Links

