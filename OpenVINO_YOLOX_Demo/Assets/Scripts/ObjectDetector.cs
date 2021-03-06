using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;
using UnityEngine.Rendering;
using System;
using System.Runtime.InteropServices;
using UnityEngine.UI;

public class ObjectDetector : MonoBehaviour
{
    [Tooltip("The screen for viewing preprocessed images")]
    public Transform videoScreen;

    [Tooltip("Performs the preprocessing and postprocessing steps")]
    public ComputeShader imageProcessingShader;

    [Tooltip("Switch between the available compute devices for OpenVINO")]
    public TMPro.TMP_Dropdown deviceDropdown;

    [Tooltip("Switch between the available OpenVINO models")]
    public TMPro.TMP_Dropdown modelDropdown;

    [Tooltip("Switch between the available video files")]
    public TMPro.TMP_Dropdown videoDropdown;

    [Tooltip("Turn stylization on and off")]
    public Toggle inference;

    [Tooltip("Use webcam feed as input")]
    public Toggle useWebcam;

    [Tooltip("Turn AsyncGPUReadback on and off")]
    public Toggle useAsync;

    [Tooltip("Text area to display console output")]
    public Text consoleText;

    [Tooltip("List of available video files")]
    public VideoClip[] videoClips;


    // Name of the DLL file
    const string dll = "OpenVINO_YOLOX_DLL";

    [DllImport(dll)]
    private static extern IntPtr GetAvailableDevices();

    [DllImport(dll)]
    private static extern IntPtr InitOpenVINO(string model, int width, int height, int device);

    [DllImport(dll)]
    private static extern void PerformInference(IntPtr inputData);

    [DllImport(dll)]
    private static extern void PopulateObjectsArray(IntPtr objects);

    [DllImport(dll)]
    private static extern int GetObjectCount();

    [DllImport(dll)]
    private static extern void SetNMSThreshold(float threshold);

    [DllImport(dll)]
    private static extern void SetConfidenceThreshold(float threshold);


    // The requested webcam dimensions
    private Vector2Int webcamDims = new Vector2Int(1280, 720);
    // The dimensions of the current video source
    private Vector2Int videoDims;
    // The targrt resolution for input images
    private Vector2Int targetDims = new Vector2Int(640, 640);
    // The unpadded dimensions of the image being fed to the model
    private Vector2Int imageDims = new Vector2Int(0, 0);

    // Live video input from a webcam
    private WebCamTexture webcamTexture;
    
    // The source video texture
    private RenderTexture videoTexture;
    // The texture used to create input tensor
    private RenderTexture rTex;

    // Contains the input texture that will be sent to the OpenVINO inference engine
    private Texture2D inputTex;

    // Keeps track of whether to execute the OpenVINO model
    private bool performInference = true;

    // The requested webcam frame rate
    private int webcamFPS = 60;

    // Used to scale the input image dimensions while maintaining aspect ratio
    private float aspectRatioScale;

    // Current compute device for OpenVINO
    private string currentDevice;

    // Stores the raw pixel data for inputTex
    private byte[] inputData;
    // Stores information about detected obejcts
    private Utils.Object[] objectInfoArray;

    // Stores the bounding boxes for detected objects
    private List<BoundingBox> boundingBoxes = new List<BoundingBox>();
    // Parsed list of compute devices for OpenVINO
    private List<string> deviceList = new List<string>();
    // File paths for the OpenVINO IR models
    private List<string> openVINOPaths = new List<string>();
    // Names of the OpenVINO IR model
    private List<string> openvinoModels = new List<string>();
    // Names of the available video files 
    private List<string> videoNames = new List<string>();

    // A reference to the canvas for the user interface
    private GameObject canvas;
    // A reference to the Graphy on-screen metrics
    private GameObject graphy;
    // References to input fields for the target image dimensions
    private TMPro.TMP_InputField width;
    private TMPro.TMP_InputField height;


    /// <summary>
    /// Updates onscreen console text
    /// </summary>
    /// <param name="logString"></param>
    /// <param name="stackTrace"></param>
    /// <param name="type"></param>
    public void Log(string logString, string stackTrace, LogType type)
    {
        consoleText.text = consoleText.text + "\n " + logString;
    }

    // Called when the object becomes enabled and active
    void OnEnable() 
    {
        Application.logMessageReceived += Log;
    }

    /// <summary>
    /// Prepares the videoScreen GameObject to display the chosen video source.
    /// </summary>
    /// <param name="width"></param>
    /// <param name="height"></param>
    /// <param name="mirrorScreen"></param>
    private void InitializeVideoScreen(int width, int height)
    {
        // Set the render mode for the video player
        videoScreen.GetComponent<VideoPlayer>().renderMode = VideoRenderMode.RenderTexture;

        // Use new videoTexture for Video Player
        videoScreen.GetComponent<VideoPlayer>().targetTexture = videoTexture;

        // Apply the new videoTexture to the VideoScreen Gameobject
        videoScreen.gameObject.GetComponent<MeshRenderer>().material.shader = Shader.Find("Unlit/Texture");
        videoScreen.gameObject.GetComponent<MeshRenderer>().material.SetTexture("_MainTex", videoTexture);
        // Adjust the VideoScreen dimensions for the new videoTexture
        videoScreen.localScale = new Vector3(width, height, videoScreen.localScale.z);
        // Adjust the VideoScreen position for the new videoTexture
        videoScreen.position = new Vector3(width / 2, height / 2, 1);
    }

    /// <summary>
    /// Try to initialize and start a webcam
    /// </summary>
    private void InitializeWebcam()
    {

        // Create a new WebCamTexture
        webcamTexture = new WebCamTexture(webcamDims.x, webcamDims.y, webcamFPS);

        // Start the Camera
        webcamTexture.Play();

        if (webcamTexture.width == 16)
        {
            webcamTexture.Stop();
            Debug.Log("\nUnable to initialize a webcam. Disabling option.\n");
            useWebcam.isOn = false;
            useWebcam.enabled = false;
        }
        else
        {
            // Limit application framerate to the target webcam framerate
            Application.targetFrameRate = webcamFPS;

            // Deactivate the Video Player
            videoScreen.GetComponent<VideoPlayer>().enabled = false;

            // Update the videoDims.y
            videoDims.y = webcamTexture.height;
            // Update the videoDims.x
            videoDims.x = webcamTexture.width;
        }

    }

    /// <summary>
    /// Resizes and positions the in-game Camera to accommodate the video dimensions
    /// </summary>
    private void InitializeCamera()
    {
        // Get a reference to the Main Camera GameObject
        GameObject mainCamera = GameObject.Find("Main Camera");
        // Adjust the camera position to account for updates to the VideoScreen
        mainCamera.transform.position = new Vector3(videoDims.x / 2, videoDims.y / 2, -10f);
        // Render objects with no perspective (i.e. 2D)
        mainCamera.GetComponent<Camera>().orthographic = true;
        // Adjust the camera size to account for updates to the VideoScreen
        int orthographicSize;
        if (((float)Screen.width / Screen.height) < ((float)videoDims.x / videoDims.y)){
            float scale = ((float)Screen.width / Screen.height) /
            ((float)videoDims.x / videoDims.y);
            orthographicSize = (int)((videoDims.y / 2) / scale);
        }
        else
        {
            orthographicSize = (int)(videoDims.y / 2);
        }
        
        Debug.Log($"Orthogrphic Size: {orthographicSize}");
        mainCamera.GetComponent<Camera>().orthographicSize = orthographicSize;
    }

    /// <summary>
    /// Calculate the dimensions for the input image
    /// </summary>
    /// <param name="newVideo"></param>
    private void InitializeTextures(bool newVideo = false)
    {
        if (newVideo)
        {
            // Calculate scale for new  aspect ratio
            int min = Mathf.Min(videoTexture.width, videoTexture.height);
            int max = Mathf.Max(videoTexture.width, videoTexture.height);
            aspectRatioScale = (float)min / max;

            // Adjust the smallest input dimension to maintain the new aspect ratio
            if (max == videoTexture.height)
            {
                imageDims.x = (int)(targetDims.y * aspectRatioScale);
                imageDims.y = targetDims.y;
            }
            else
            {
                imageDims.y = (int)(targetDims.x * aspectRatioScale);
                imageDims.x = targetDims.x;
            }
        }
        else
        {
            // Adjust the input dimensions to maintain the current aspect ratio
            if (imageDims.x != targetDims.x)
            {
                imageDims.x = targetDims.x;
                aspectRatioScale = (float)videoTexture.height / videoTexture.width;
                imageDims.y = (int)(targetDims.x * aspectRatioScale);
                targetDims.y = imageDims.y;

            }
            if (imageDims.y != targetDims.y)
            {
                imageDims.y = targetDims.y;
                aspectRatioScale = (float)videoTexture.width / videoTexture.height;
                imageDims.x = (int)(targetDims.y * aspectRatioScale);
                targetDims.x = imageDims.x;

            }
        }

        // Initialize the RenderTexture that will store the processed input image
        rTex = RenderTexture.GetTemporary(imageDims.x, imageDims.y, 24, RenderTextureFormat.ARGB32);
        // Update inputTex with the new dimensions
        inputTex = new Texture2D(imageDims.x, imageDims.y, TextureFormat.RGBA32, false);

        // Update the values for the width and height input fields
        Debug.Log($"Setting Input Dims to W: {imageDims.x} x H: {imageDims.y}");
        width.text = $"{imageDims.x}";
        height.text = $"{imageDims.y}";
    }

    /// <summary>
    /// Initialize the options for the dropdown menus
    /// </summary>
    private void InitializeDropdowns()
    {
        // Remove default dropdown options
        deviceDropdown.ClearOptions();
        // Add OpenVINO compute devices to dropdown
        deviceDropdown.AddOptions(deviceList);
        // Set the value for the dropdown to the current compute device
        deviceDropdown.SetValueWithoutNotify(deviceList.IndexOf(currentDevice));

        // Remove default dropdown options
        videoDropdown.ClearOptions();
        // Add OpenVINO models to menu
        videoDropdown.AddOptions(videoNames);
        // Select the first option in the dropdown
        videoDropdown.SetValueWithoutNotify(0);

        // Remove default dropdown options
        modelDropdown.ClearOptions();
        // Add OpenVINO models to menu
        modelDropdown.AddOptions(openvinoModels);
        // Select the first option in the dropdown
        modelDropdown.SetValueWithoutNotify(0);
    }

    /// <summary>
    /// Called when a model option is selected from the dropdown
    /// </summary>
    public void InitializeOpenVINO()
    {
        // Only initialize OpenVINO when performing inference
        if (performInference == false) return;

        Debug.Log("Initializing OpenVINO");
        Debug.Log($"Selected Model: {openvinoModels[modelDropdown.value]}");
        Debug.Log($"Selected Model Path: {openVINOPaths[modelDropdown.value]}");
        Debug.Log($"Setting Input Dims to W: {imageDims.x} x H: {imageDims.y}");
        Debug.Log("Uploading IR Model to Compute Device");

        // Set up the neural network for the OpenVINO inference engine
        currentDevice = Marshal.PtrToStringAnsi(InitOpenVINO(
            openVINOPaths[modelDropdown.value],
            inputTex.width,
            inputTex.height,
            deviceDropdown.value));

        Debug.Log($"OpenVINO using: {currentDevice}");
    }

    /// <summary>
    /// Perform the initialization steps required when the model input is updated
    /// </summary>
    private void InitializationSteps()
    {
        if (useWebcam.isOn)
        {
            // Initialize webcam
            InitializeWebcam();
        }
        else
        {
            Debug.Log($"Selected Video: {videoDropdown.value}");

            // Set Initial video clip
            videoScreen.GetComponent<VideoPlayer>().clip = videoClips[videoDropdown.value];
            // Update the videoDims.y
            videoDims.y = (int)videoScreen.GetComponent<VideoPlayer>().height;
            // Update the videoDims.x
            videoDims.x = (int)videoScreen.GetComponent<VideoPlayer>().width;
        }

        // Create a new videoTexture using the current video dimensions
        videoTexture = RenderTexture.GetTemporary(videoDims.x, videoDims.y, 24, RenderTextureFormat.ARGB32);

        // Initialize the videoScreen
        InitializeVideoScreen(videoDims.x, videoDims.y);
        // Adjust the camera based on the source video dimensions
        InitializeCamera();
        // Initialize the textures that store the model input
        InitializeTextures(true);
        // Set up the neural network for the OpenVINO inference engine
        InitializeOpenVINO();
    }

    /// <summary>
    /// Get the list of available OpenVINO models
    /// </summary>
    private void GetOpenVINOModels()
    {
        // Get the subdirectories containing the available models
        string[] modelDirs = System.IO.Directory.GetDirectories("models");

        // Get the model files in each subdirectory
        List<string> openVINOFiles = new List<string>();
        foreach (string dir in modelDirs)
        {
            openVINOFiles.AddRange(System.IO.Directory.GetFiles(dir));
        }

        // Get the paths for the .xml files for each model
        Debug.Log("Available OpenVINO Models:");
        foreach (string file in openVINOFiles)
        {
            if (file.EndsWith(".xml"))
            {
                openVINOPaths.Add(file);
                string modelName = file.Split('\\')[1];
                openvinoModels.Add(modelName.Substring(0, modelName.Length));

                Debug.Log($"Model Name: {modelName}");
                Debug.Log($"File Path: {file}");
            }
        }
        Debug.Log("");
    }


    // Start is called before the first frame update
    void Start()
    {
        // Get references to GameObjects in hierarchy
        canvas = GameObject.Find("Canvas");
        graphy = GameObject.Find("[Graphy]");
        width = GameObject.Find("Width").GetComponent<TMPro.TMP_InputField>();
        height = GameObject.Find("Height").GetComponent<TMPro.TMP_InputField>();

        // Check if either the CPU of GPU is made by Intel
        string processorType = SystemInfo.processorType.ToString();
        string graphicsDeviceName = SystemInfo.graphicsDeviceName.ToString();
        if (processorType.Contains("Intel") || graphicsDeviceName.Contains("Intel"))
        {
            // Get the list of available models
            GetOpenVINOModels();

            // Get an unparsed list of available 
            string openvinoDevices = Marshal.PtrToStringAnsi(GetAvailableDevices());

            Debug.Log($"Available Devices:");
            // Parse list of available compute devices
            foreach (string device in openvinoDevices.Split(','))
            {
                // Add device name to list
                deviceList.Add(device);
                Debug.Log(device);
            }
        }
        else
        {
            inference.isOn = performInference = inference.enabled = false;
            Debug.Log("No Intel hardware detected");
        }

        // Get the names of the video clips
        foreach (VideoClip clip in videoClips) videoNames.Add(clip.name);

        // Initialize the dropdown menus
        InitializeDropdowns();
        // Perform the requred 
        InitializationSteps();
    }

    /// <summary>
    /// Perform a flip operation of the GPU
    /// </summary>
    /// <param name="image">The image to be flipped</param>
    /// <param name="tempTex">Stores the flipped image</param>
    /// <param name="functionName">The name of the function to execute in the compute shader</param>
    private void FlipImage(RenderTexture image, string functionName)
    {
        // Specify the number of threads on the GPU
        int numthreads = 4;
        // Get the index for the PreprocessResNet function in the ComputeShader
        int kernelHandle = imageProcessingShader.FindKernel(functionName);

        /// Allocate a temporary RenderTexture
        RenderTexture result = RenderTexture.GetTemporary(image.width, image.height, 24, image.format);
        // Enable random write access
        result.enableRandomWrite = true;
        // Create the RenderTexture
        result.Create();

        // Set the value for the Result variable in the ComputeShader
        imageProcessingShader.SetTexture(kernelHandle, "Result", result);
        // Set the value for the InputImage variable in the ComputeShader
        imageProcessingShader.SetTexture(kernelHandle, "InputImage", image);
        // Set the value for the height variable in the ComputeShader
        imageProcessingShader.SetInt("height", image.height);
        // Set the value for the width variable in the ComputeShader
        imageProcessingShader.SetInt("width", image.width);

        // Execute the ComputeShader
        imageProcessingShader.Dispatch(kernelHandle, image.width / numthreads, image.height / numthreads, 1);

        // Copy the flipped image to tempTex
        Graphics.Blit(result, image);

        // Release the temporary RenderTexture
        RenderTexture.ReleaseTemporary(result);
    }

    /// <summary>
    /// Called once AsyncGPUReadback has been completed
    /// </summary>
    /// <param name="request"></param>
    void OnCompleteReadback(AsyncGPUReadbackRequest request)
    {
        if (request.hasError)
        {
            Debug.Log("GPU readback error detected.");
            return;
        }

        // Fill Texture2D with raw data from the AsyncGPUReadbackRequest
        inputTex.LoadRawTextureData(request.GetData<uint>());
        // Apply changes to Textur2D
        inputTex.Apply();
    }

    /// <summary>
    /// Pin memory for the input data and send it to OpenVINO for inference
    /// </summary>
    /// <param name="inputData"></param>
    public unsafe void UploadTexture(byte[] inputData)
    {
        //Pin Memory
        fixed (byte* p = inputData)
        {
            // Perform inference
            PerformInference((IntPtr)p);
        }

        // Get the number of detected objects
        int numObjects = GetObjectCount();
        // Initialize the array
        objectInfoArray = new Utils.Object[numObjects];

        // Pin memory
        fixed (Utils.Object* o = objectInfoArray)
        {
            // Get the detected objects
            PopulateObjectsArray((IntPtr)o);
        }
    }

    /// <summary>
    /// Update the list of bounding boxes based on the latest output from the model
    /// </summary>
    private void UpdateBoundingBoxes()
    {
        // Process new detected objects
        for (int i = 0; i < objectInfoArray.Length; i++)
        {
            // The smallest dimension of the videoTexture
            int minDimension = Mathf.Min(videoTexture.width, videoTexture.height);

            // The value used to scale the bbox locations up to the source resolution
            float scale = (float)minDimension / Mathf.Min(imageDims.x, imageDims.y);

            // Flip the bbox coordinates vertically
            objectInfoArray[i].y0 = rTex.height - objectInfoArray[i].y0;

            objectInfoArray[i].x0 *= scale;
            objectInfoArray[i].y0 *= scale;
            objectInfoArray[i].width *= scale;
            objectInfoArray[i].height *= scale;

            // Update bounding box list with new object info
            try
            {
                boundingBoxes[i].SetObjectInfo(objectInfoArray[i]);
            }
            catch
            {
                // Add a new bounding box object when needed
                boundingBoxes.Add(new BoundingBox(objectInfoArray[i]));
            }
        }

        // Turn off extra bounding boxes
        for (int i = 0; i < boundingBoxes.Count; i++)
        {
            if (i > objectInfoArray.Length - 1)
            {
                boundingBoxes[i].ToggleBBox(false);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        // Toggle the user interface
        if (Input.GetKeyDown("space"))
        {
            canvas.SetActive(!canvas.activeInHierarchy);
            graphy.SetActive(!graphy.activeInHierarchy);
        }

        // Copy webcamTexture to videoTexture if using webcam
        if (useWebcam.isOn) Graphics.Blit(webcamTexture, videoTexture);

        // Toggle whether to perform inference
        if (performInference == false) return;

        // Copy the videoTexture to the rTex RenderTexture
        Graphics.Blit(videoTexture, rTex);

        // Flip image before sending to DLL
        FlipImage(rTex, "FlipXAxis");

        // Download pixel data from GPU to CPU
        if (useAsync.isOn)
        {
            AsyncGPUReadback.Request(rTex, 0, TextureFormat.RGBA32, OnCompleteReadback);
        }
        else
        {
            RenderTexture.active = rTex;
            inputTex.ReadPixels(new Rect(0, 0, rTex.width, rTex.height), 0, 0);
            inputTex.Apply();
        }

        // Send reference to inputData to DLL
        UploadTexture(inputTex.GetRawTextureData());

        // Update bounding boxes with new object info
        UpdateBoundingBoxes();
    }

    /// <summary>
    /// Called when the input dimensions are updated in the GUI
    /// </summary>
    public void UpdateInputDims()
    {
        // Pares the new width value
        int newWidth;
        int.TryParse(width.text, out newWidth);
        // Parse the new height value
        int newHeight;
        int.TryParse(height.text, out newHeight);
        // Update target dims
        targetDims = new Vector2Int(newWidth, newHeight);
        // Initialize the textures that store the model input
        InitializeTextures();
        // Set up the neural network for the OpenVINO inference engine
        InitializeOpenVINO();
    }

    /// <summary>
    /// Called when the value for the Inference toggle is updated
    /// </summary>
    public void UpdateInferenceValue()
    {
        // Only update the performInference value if the canvas is active
        //if (canvas.activeInHierarchy) performInference = inference.isOn;
        performInference = inference.isOn;

        if (performInference)
        {
            InitializeOpenVINO();
        }
        else
        {
            // Hide all bounding boxes when not performing inference
            for (int i = 0; i < boundingBoxes.Count; i++)
            {
                boundingBoxes[i].ToggleBBox(false);
            }
        }
    }

    /// <summary>
    /// Called when the NMS threshold value is updated in the GUI
    /// </summary>
    /// <param name="inputField"></param>
    public void UpdateNMSThreshold(TMPro.TMP_InputField inputField)
    {
        // Parse the input field value
        float threshold;
        float.TryParse(inputField.text, out threshold);
        // Clamp threshold value between 0 and 1
        threshold = Mathf.Min(threshold, 1f);
        threshold = Mathf.Max(0f, threshold);
        // Update the threshold value
        inputField.text = $"{threshold}";
        SetNMSThreshold(threshold);
    }

    /// <summary>
    /// Called when the confidence threshold is updated in the GUI
    /// </summary>
    /// <param name="inputField"></param>
    public void UpdateConfThreshold(TMPro.TMP_InputField inputField)
    {
        // Parse the input field value
        float threshold;
        float.TryParse(inputField.text, out threshold);
        // Clamp threshold value between 0 and 1
        threshold = Mathf.Min(threshold, 1f);
        threshold = Mathf.Max(0f, threshold);
        // Update the threshold value
        inputField.text = $"{threshold}";
        SetConfidenceThreshold(threshold);
    }

    /// <summary>
    /// Called when a model option is selected from the dropdown
    /// </summary>
    public void UpdateVideo()
    {
        if (videoScreen.GetComponent<VideoPlayer>().enabled == false) return;

        Debug.Log($"Selected Video: {videoDropdown.value}");
        InitializationSteps();
    }

    /// <summary>
    /// Called when the value for the Use Webcam toggle is updated
    /// </summary>
    public void UseWebcam()
    {
        if (useWebcam.isOn)
        {
            WebCamDevice[] devices = WebCamTexture.devices;
            for (int i = 0; i < devices.Length; i++)
            {
                Debug.Log(devices[i].name);
            }

            if (WebCamTexture.devices.Length == 0)
            {
                Debug.Log("No webcam device detected.");
                useWebcam.SetIsOnWithoutNotify(false);
            }
        }
        else
        {
            // Stop the webcam
            webcamTexture.Stop();
            // Activate the Video Player
            videoScreen.GetComponent<VideoPlayer>().enabled = true;
        }

        InitializationSteps();
    }

    // Called when the MonoBehaviour will be destroyed
    private void OnDestroy()
    {
        Application.logMessageReceived -= Log;
    }

    /// <summary>
    /// Called when the Quit button is clicked.
    /// </summary>
    public void Quit()
    {
        // Causes the application to exit
        Application.Quit();
    }
}
