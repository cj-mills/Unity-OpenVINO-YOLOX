// dllmain.cpp : Defines the entry point for the DLL application.
#include "pch.h"

using namespace InferenceEngine;

// Create a macro to quickly mark a function for export
#define DLLExport __declspec (dllexport)

// Wrap code to prevent name-mangling issues
extern "C" {

    // Stores information about a single object prediction
    struct Object
    {
        float x0;
        float y0;
        float width;
        float height;
        int label;
        float prob;
    };

    // Store grid offset and stride values to decode a section of the model output
    struct GridAndStride
    {
        int grid0;
        int grid1;
        int stride;
    };

    // The width of the source input image
    int img_w;
    // The height of the source input image
    int img_h;
    // The input width for the model
    int input_w = 640;
    // The input height for the model
    int input_h = 640;
    // Stores the final number of objects detected after decoding the model outptut
    int count = 0;

    // The scale value used to adjust the model output to the original unpadded image
    float scale;
    // The minimum confidence score to consider an object proposal
    float bbox_conf_thresh = 0.3;
    // The maximum intersection over union value before an object proposal will be ignored
    float nms_thresh = 0.45;

    // List of available compute devices
    std::vector<std::string> available_devices;
    // Stores the grid and stride values
    std::vector<GridAndStride> grid_strides;
    // Stores the object proposals with confidence scores above bbox_conf_thresh
    std::vector<Object> proposals;
    // Stores the indices for the object proposals selected using non-maximum suppression
    std::vector<int> picked;

    // Inference engine instance
    Core ie;
    // Contains all the information about the Neural Network topology and related constant values for the model
    CNNNetwork network;
    // Provides an interface for an executable network on the compute device
    ExecutableNetwork executable_network;
    // Provides an interface for an asynchronous inference request
    InferRequest infer_request;

    // A poiner to the input tensor for the model
    MemoryBlob::Ptr minput;
    // A poiner to the output tensor for the model
    MemoryBlob::CPtr moutput;


    // Returns an unparsed list of available compute devices
    DLLExport int FindAvailableDevices(std::string* device_list) {


        available_devices.clear();

        for (auto&& device : ie.GetAvailableDevices()) {
            if (device.find("GNA") != std::string::npos) continue;

            available_devices.push_back(device);
        }

        // Reverse the order of the list
        std::reverse(available_devices.begin(), available_devices.end());

        // Configure the cache directory for GPU compute devices
        ie.SetConfig({ {CONFIG_KEY(CACHE_DIR), "cache"} }, "GPU");

        return available_devices.size();
    }

    DLLExport std::string* GetDeviceName(int index) {
        return &available_devices[index];
    }

    // Get the final number of objects detected in the current model outptut
    DLLExport int GetObjectCount() {
        return count;
    }

    // Set the minimum confidence score
    DLLExport void SetConfidenceThreshold(float threshold) {
        bbox_conf_thresh = threshold;
    }

    // Set the maximum intersection over union value
    DLLExport void SetNMSThreshold(float threshold) {
        nms_thresh = threshold;
    }

    // Generate the grid and stride values
    void GenerateGridsAndStride() {

        // The stride values used to generate the gride_strides vector
        const int strides[] = { 8, 16, 32 };

        // Iterate through each stride value
        for (auto stride : strides)
        {
            // Calculate the grid dimensions
            int grid_height = input_h / stride;
            int grid_width = input_w / stride;

            // Store each combination of grid coordinates
            for (int g1 = 0; g1 < grid_height; g1++)
            {
                for (int g0 = 0; g0 < grid_width; g0++)
                {
                    grid_strides.push_back(GridAndStride{ g0, g1, stride });
                }
            }
        }
    }

    // Manually set the input resolution for the model
    void SetInputDims(int width, int height) {

        img_w = width;
        img_h = height;

        // Calculate the padded model input dimensions
        input_w = (int)(32 * std::roundf(img_w / 32));
        input_h = (int)(32 * std::roundf(img_h / 32));

        // Calculate the value used to adjust the model output to the original unpadded image
        scale = std::min(input_w / (img_w * 1.0), input_h / (img_h * 1.0));

        // Generate grid_strides for the padded model input dimensions
        grid_strides.clear();
        GenerateGridsAndStride();

        // Collect the map of input names and shapes from IR
        auto input_shapes = network.getInputShapes();

        // Set new input shapes
        std::string input_name_1;
        InferenceEngine::SizeVector input_shape;
        // Create a tuple for accessing the input dimensions
        std::tie(input_name_1, input_shape) = *input_shapes.begin();
        // Set batch size to the first input dimension
        input_shape[0] = 1;
        // Update the height for the input dimensions
        input_shape[2] = input_h;
        // Update the width for the input dimensions
        input_shape[3] = input_w;
        input_shapes[input_name_1] = input_shape;

        // Perform shape inference with the new input dimensions
        network.reshape(input_shapes);
    }


    // Create an executable network for the target compute device
    std::string* UploadModelToDevice(int deviceNum) {

        // Create executable network
        executable_network = ie.LoadNetwork(network, available_devices[deviceNum]);
        // Create an inference request object
        infer_request = executable_network.CreateInferRequest();

        // Get the name of the input layer
        std::string input_name = network.getInputsInfo().begin()->first;
        // Get a poiner to the input tensor for the model
        minput = as<MemoryBlob>(infer_request.GetBlob(input_name));

        // Get the name of the output layer
        std::string output_name = network.getOutputsInfo().begin()->first;
        // Get a poiner to the ouptut tensor for the model
        moutput = as<MemoryBlob>(infer_request.GetBlob(output_name));

        // Return the name of the current compute device
        return GetDeviceName(deviceNum);
    }

    // Set up OpenVINO inference engine
    DLLExport std::string* InitOpenVINO(char* modelPath, int width, int height, int deviceNum) {

        // Read network file
        network = ie.ReadNetwork(modelPath);

        SetInputDims(width, height);

        return UploadModelToDevice(deviceNum);
    }

    // Resize and pad the source input image to the dimensions expected by the model
    cv::Mat StaticResize(cv::Mat& img) {
        // Calculate the unpadded input dimensions
        float r = std::min(input_w / (img.cols * 1.0), input_h / (img.rows * 1.0));
        int unpad_w = r * img.cols;
        int unpad_h = r * img.rows;

        // Scale the input image to the unpadded input dimensions
        cv::Mat re(unpad_h, unpad_w, CV_8UC3);
        cv::resize(img, re, re.size());

        // Create a new Mat with the padded input dimensions
        cv::Mat out(input_h, input_w, CV_8UC3);
        // Copy the unpadded image data to the padded Mat
        re.copyTo(out(cv::Rect(0, 0, re.cols, re.rows)));
        return out;
    }

    // Create object proposals for all model predictions with high enough confidence scores
    void GenerateYoloxProposals(const float* feat_ptr) {

        const int num_anchors = grid_strides.size();

        // Obtain the length of a single bounding box proposal
        const int proposal_length = moutput->getTensorDesc().getDims()[2];

        // Obtain the number of classes the model was trained to detect
        const int num_classes = proposal_length - 5;

        for (int anchor_idx = 0; anchor_idx < num_anchors; anchor_idx++)
        {
            // Get the current grid and stride values
            const int grid0 = grid_strides[anchor_idx].grid0;
            const int grid1 = grid_strides[anchor_idx].grid1;
            const int stride = grid_strides[anchor_idx].stride;

            // Get the starting index for the current proposal
            const int basic_pos = anchor_idx * proposal_length;

            // Get the coordinates for the center of the predicted bounding box
            float x_center = (feat_ptr[basic_pos + 0] + grid0) * stride;
            float y_center = (feat_ptr[basic_pos + 1] + grid1) * stride;

            // Get the dimensions for the predicte bounding box
            float w = exp(feat_ptr[basic_pos + 2]) * stride;
            float h = exp(feat_ptr[basic_pos + 3]) * stride;

            // Calculate the coordinates for the upper left corner of the bounding box
            float x0 = x_center - w * 0.5f;
            float y0 = y_center - h * 0.5f;

            // Get the confidence score that an object is present
            float box_objectness = feat_ptr[basic_pos + 4];

            // Initialize object struct with bounding box information
            Object obj = { x0 , y0, w, h, 0, 0 };

            // Find the object class with the highest confidence score
            for (int class_idx = 0; class_idx < num_classes; class_idx++)
            {
                // Get the confidence score for the current object class
                float box_cls_score = feat_ptr[basic_pos + 5 + class_idx];
                // Calculate the final confidence score for the object proposal
                float box_prob = box_objectness * box_cls_score;

                // Check for the highest confidence score
                if (box_prob > obj.prob) {
                    obj.label = class_idx;
                    obj.prob = box_prob;
                }
            }

            // Only add object proposals with high enough confidence scores
            if (obj.prob > bbox_conf_thresh) proposals.push_back(obj);
        }
    }

    // Filter through a sorted list of object proposals using Non-maximum suppression
    void NmsSortedBboxes() {

        const int n = proposals.size();

        // Iterate through the object proposals
        for (int i = 0; i < n; i++)
        {
            const Object& a = proposals[i];

            // Create OpenCV rectangle for the Object bounding box
            cv::Rect_<float> aRect = cv::Rect_<float>(a.x0, a.y0, a.width, a.height);
            // Get the bounding box area
            const float aRect_area = aRect.area();

            bool keep = true;

            // Check if the current object proposal overlaps any selected objects too much
            for (int j = 0; j < (int)picked.size(); j++)
            {
                const Object& b = proposals[picked[j]];

                // Create OpenCV rectangle for the Object bounding box
                cv::Rect_<float> bRect = cv::Rect_<float>(b.x0, b.y0, b.width, b.height);

                // Calculate the area where the two object bounding boxes overlap
                float inter_area = (aRect & bRect).area();
                // Calculate the union area of both bounding boxes
                float union_area = aRect_area + bRect.area() - inter_area;
                // Ignore object proposals that overlap selected objects too much
                if (inter_area / union_area > nms_thresh) keep = false;
            }

            // Keep object proposals that do not overlap selected objects too much
            if (keep) picked.push_back(i);
        }
    }

    // The comparison function for sorting the object proposals
    bool CompareProposals(const Object& a, const Object& b) {

        return a.prob > b.prob;
    }

    // Process the model outptut to determine detected objects
    void DecodeOutputs(const float* prob) {

        // Remove the proposals for the previous model output
        proposals.clear();
        // Generate new proposals for the current model output
        GenerateYoloxProposals(prob);

        // Sort the generated proposals based on their confidence scores
        std::sort(proposals.begin(), proposals.end(), CompareProposals);

        // Remove the picked proposals for the previous model outptut
        picked.clear();
        // Pick detected objects to keep using Non-maximum Suppression
        NmsSortedBboxes();
        // Update the number of objects detected
        count = picked.size();
    }

    // Perform inference with the provided texture data
    DLLExport void PerformInference(uchar* inputData) {

        // Store the pixel data for the source input image
        cv::Mat texture = cv::Mat(img_h, img_w, CV_8UC4);

        // Assign the inputData to the OpenCV Mat
        texture.data = inputData;
        // Remove the alpha channel
        cv::cvtColor(texture, texture, cv::COLOR_RGBA2RGB);
        // Resize and pad the input image
        cv::Mat pr_img = StaticResize(texture);

        // The number of color channels 
        int num_channels = pr_img.channels();
        // Get the number of pixels in the input image
        int H = minput->getTensorDesc().getDims()[2];
        int W = minput->getTensorDesc().getDims()[3];
        int nPixels = W * H;

        // locked memory holder should be alive all time while access to its buffer happens
        LockedMemory<void> ilmHolder = minput->wmap();

        // Filling input tensor with image data
        float* input_data = ilmHolder.as<float*>();

        // The mean of the ImageNet dataset used to train the model
        const float mean[] = { 0.485, 0.456, 0.406 };
        // The standard deviation of the ImageNet dataset used to train the model
        const float standard_dev[] = { 0.229, 0.224, 0.225 };

        // Iterate over each pixel in image
        for (int p = 0; p < nPixels; p++) {
            // Iterate over each color channel for each pixel in image
            for (int ch = 0; ch < num_channels; ++ch) {
                input_data[ch * nPixels + p] = (pr_img.data[p * num_channels + ch] / 255.0f - mean[ch]) / standard_dev[ch];
            }
        }

        // Perform inference
        infer_request.Infer();

        // locked memory holder should be alive all time while access to its buffer happens
        LockedMemory<const void> moutputHolder = moutput->rmap();
        const float* net_pred = moutputHolder.as<const PrecisionTrait<Precision::FP32>::value_type*>();

        // Process the model output
        DecodeOutputs(net_pred);
    }

    // Fill the provided array with the detected objects
    DLLExport void PopulateObjectsArray(Object* objects) {

        for (int i = 0; i < count; i++)
        {
            Object object = proposals[picked[i]];

            // Adjust offset to original unpadded dimensions
            float x0 = (object.x0) / scale;
            float y0 = (object.y0) / scale;

            // Clamp the image coordinates to the original image dimensions
            x0 = std::max(std::min(x0, (float)(img_w - 1)), 0.f);
            y0 = std::max(std::min(y0, (float)(img_h - 1)), 0.f);

            // Save the final object information
            object.x0 = x0;
            object.y0 = y0;

            objects[i] = object;
        }
    }

    DLLExport void FreeResources() {

        available_devices.clear();
        grid_strides.clear();
        proposals.clear();
        picked.clear();
    }
}