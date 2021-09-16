using UnityEngine;
using TMPro;

public class BoundingBox
{
    // Contains the bounding box
    private GameObject bbox = new GameObject();
    // Contains the label text
    private GameObject text = new GameObject();
    // The canvas on which the bounding box labels will be drawn
    private GameObject canvas = GameObject.Find("Label Canvas");

    // The object information for the bounding box
    private Utils.Object info;

    // The object class color
    private Color color;

    // The adjusted line width for the bounding box
    private int lineWidth = (int)(Screen.width * 1.75e-3);
    // The adjusted font size based on the screen size
    private float fontSize = (float)(Screen.width * 9e-3);

    // The label text
    private TextMeshProUGUI textContent;

    // Draws the bounding box
    private LineRenderer lineRenderer;


    /// <summary>
    /// Initialize the label for the bounding box
    /// </summary>
    /// <param name="label"></param>
    private void InitializeLabel()
    {
        // Set the label text
        textContent.text = $"{text.name}: {(info.prob * 100).ToString("0.##")}%";
        // Set the text color
        textContent.color = color;
        // Set the text alignment
        textContent.alignment = TextAlignmentOptions.MidlineLeft;
        // Set the font size
        textContent.fontSize = fontSize;
        // Resize the text area
        RectTransform rectTransform = text.GetComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(250, 50);
        // Position the label above the top left corner of the bounding box
        Vector3 textPos = Camera.main.WorldToScreenPoint(new Vector3(info.x0, info.y0, -10f));
        float xOffset = rectTransform.rect.width / 2;
        textPos = new Vector3(textPos.x + xOffset, textPos.y + textContent.fontSize, textPos.z);
        text.transform.position = textPos;
    }

    /// <summary>
    /// Toggle the visibility for the bounding box
    /// </summary>
    /// <param name="show"></param>
    public void ToggleBBox(bool show)
    {
        bbox.SetActive(show);
        text.SetActive(show);
    }

    /// <summary>
    /// Initialize the position and dimensions for the bounding box
    /// </summary>
    private void InitializeBBox()
    {
        // Set the material color
        lineRenderer.material.color = color;

        // The bbox will consist of five points
        lineRenderer.positionCount = 5;

        // Set the width from the start point
        lineRenderer.startWidth = lineWidth;
        // Set the width from the end point
        lineRenderer.endWidth = lineWidth;

        // Get object information
        float x0 = info.x0;
        float y0 = info.y0;
        float width = info.width;
        float height = info.height;

        // Offset value to align the bounding box points
        float offset = lineWidth / 2;

        // Top left point
        Vector3 pos0 = new Vector3(x0, y0, 0);
        lineRenderer.SetPosition(0, pos0);
        // Top right point
        Vector3 pos1 = new Vector3(x0 + width, y0, 0);
        lineRenderer.SetPosition(1, pos1);
        // Bottom right point
        Vector3 pos2 = new Vector3(x0 + width, (y0 - height) + offset, 0);
        lineRenderer.SetPosition(2, pos2);
        // Bottom left point
        Vector3 pos3 = new Vector3(x0 + offset, (y0 - height) + offset, 0);
        lineRenderer.SetPosition(3, pos3);
        // Closing Point
        Vector3 pos4 = new Vector3(x0 + offset, y0 + offset, 0);
        lineRenderer.SetPosition(4, pos4);

        // Make sure the bounding box is visible
        ToggleBBox(true);
    }

    /// <summary>
    /// Update the object info for the bounding box
    /// </summary>
    /// <param name="objectInfo"></param>
    public void SetObjectInfo(Utils.Object objectInfo)
    {
        // Set the object info
        info = objectInfo;
        // Get the object class label
        bbox.name = Utils.object_classes[objectInfo.label].Item1;
        text.name = bbox.name;
        // Get the object class color
        color = Utils.object_classes[objectInfo.label].Item2;

        // Initialize the label
        InitializeLabel();
        // Initializ the position and dimensions
        InitializeBBox();
    }

    /// <summary>
    /// Constructor for the bounding box
    /// </summary>
    /// <param name="objectInfo"></param>
    public BoundingBox(Utils.Object objectInfo)
    {
        // Add a text componenet to store the label text
        textContent = text.AddComponent<TextMeshProUGUI>();
        // Assign text object to the label canvas
        text.transform.SetParent(canvas.transform);

        // Add a line renderer to draw the bounding box
        lineRenderer = bbox.AddComponent<LineRenderer>();
        // Make LineRenderer Shader Unlit
        lineRenderer.material = new Material(Shader.Find("Unlit/Color"));

        // Update the object info for the bounding box
        SetObjectInfo(objectInfo);
    }
}
