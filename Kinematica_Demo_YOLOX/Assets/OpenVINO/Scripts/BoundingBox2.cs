using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class BoundingBox2
{
    // Contains the bounding box
    private GameObject bboxLine1 = new GameObject();
    private GameObject bboxLine2 = new GameObject();
    private GameObject bboxLine3 = new GameObject();
    private GameObject bboxLine4 = new GameObject();
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
    private int lineThickness = 6;
    // The adjusted font size based on the screen size
    private float fontSize = (float)(Screen.width * 1.5e-2);

    // The label text
    private TextMeshProUGUI textContent;

    // The bounding box lines
    private Image bboxImage1;
    private Image bboxImage2;
    private Image bboxImage3;
    private Image bboxImage4;


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
        Vector3 textPos = new Vector3(info.x0, info.y0, 0f);
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
        bboxLine1.SetActive(show);
        bboxLine2.SetActive(show);
        bboxLine3.SetActive(show);
        bboxLine4.SetActive(show);
        text.SetActive(show);
    }

    /// <summary>
    /// Initialize the position and dimensions for the bounding box
    /// </summary>
    private void InitializeBBox()
    {
        // Get object information
        float x0 = info.x0;
        float y0 = info.y0;
        float width = info.width;
        float height = info.height;

        // Offset value to align the bounding box points
        float offset = lineThickness / 2;

        // Set the material color
        bboxImage1.color = color;
        bboxImage2.color = color;
        bboxImage3.color = color;
        bboxImage4.color = color;

        // Top Line
        RectTransform rectTransform = bboxImage1.GetComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(width + lineThickness, lineThickness);
        Vector3 pos0 = new Vector3(x0 + (width/2), y0, 0);
        rectTransform.position = pos0;

        // Right Line
        rectTransform = bboxImage2.GetComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(lineThickness, height + lineThickness);
        Vector3 pos1 = new Vector3(x0 + width, y0 - (height/2), 0);
        rectTransform.position = pos1;

        // Bottom Line
        rectTransform = bboxImage3.GetComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(width + lineThickness, lineThickness);
        Vector3 pos2 = new Vector3(x0 + (width / 2), (y0 - height), 0);
        rectTransform.position = pos2;

        // Left Line
        rectTransform = bboxImage4.GetComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(lineThickness, height + lineThickness);
        Vector3 pos3 = new Vector3(x0, y0 - (height / 2), 0);
        rectTransform.position = pos3;

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
        string name = Utils.object_classes[objectInfo.label].Item1;
        bboxLine1.name = name + "_line_1";
        bboxLine2.name = name + "_line_2";
        bboxLine3.name = name + "_line_3";
        bboxLine4.name = name + "_line_4";
        text.name = name;
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
    public BoundingBox2(Utils.Object objectInfo)
    {
        // Add a text componenet to store the label text
        textContent = text.AddComponent<TextMeshProUGUI>();
        // Assign text object to the label canvas
        text.transform.SetParent(canvas.transform);

        bboxLine1.transform.SetParent(canvas.transform);
        bboxLine2.transform.SetParent(canvas.transform);
        bboxLine3.transform.SetParent(canvas.transform);
        bboxLine4.transform.SetParent(canvas.transform);

        bboxImage1 = bboxLine1.AddComponent<Image>();
        bboxImage2 = bboxLine2.AddComponent<Image>();
        bboxImage3 = bboxLine3.AddComponent<Image>();
        bboxImage4 = bboxLine4.AddComponent<Image>();
        
        // Update the object info for the bounding box
        SetObjectInfo(objectInfo);
    }
}
