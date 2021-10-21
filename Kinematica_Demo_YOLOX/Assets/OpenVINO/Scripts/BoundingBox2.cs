using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class BoundingBox2
{
    // Contains the label text
    private GameObject text = new GameObject();
    // The canvas on which the bounding box labels will be drawn
    private GameObject canvas = GameObject.Find("Label Canvas");

    // The object information for the bounding box
    private Utils.Object info;

    // The object class color
    private Color color;

    // The adjusted line width for the bounding box
    private int lineWidth = 6;
    // The adjusted font size based on the screen size
    private float fontSize = (float)(Screen.width * 1.5e-2);

    // The label text
    private TextMeshProUGUI textContent;

    // The UI Images used to draw the bounding box
    private GameObject[] bboxLines = new GameObject[4];


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
        for (int i = 0; i < bboxLines.Length; i++)
        {
            bboxLines[i].SetActive(show);
        }
        
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
        float offset = lineWidth / 2;


        Vector2[] dimensions = new Vector2[]
        {
            new Vector2(width + lineWidth, lineWidth),
            new Vector2(lineWidth, height + lineWidth),
            new Vector2(width + lineWidth, lineWidth),
            new Vector2(lineWidth, height + lineWidth)
        };


        Vector3[] positions = new Vector3[] 
        {
            new Vector3(x0 + (width/2), y0, 0),
            new Vector3(x0 + width, y0 - (height/2), 0),
            new Vector3(x0 + (width / 2), (y0 - height), 0),
            new Vector3(x0, y0 - (height / 2), 0)
        };

        // Set the material color
        for (int i = 0; i < bboxLines.Length; i++)
        {
            Image bboxLine = bboxLines[i].GetComponent<Image>();
            bboxLine.color = color;
            RectTransform transform = bboxLine.rectTransform;
            transform.sizeDelta = dimensions[i];
            transform.position = positions[i];

        }
        
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
        for (int i = 0; i < bboxLines.Length; i++)
        {
            bboxLines[i].name = $"{name}+_line_{i}";
        }
        
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

        for (int i=0; i < bboxLines.Length; i++)
        {
            bboxLines[i] = new GameObject();
            bboxLines[i].AddComponent<Image>();
            bboxLines[i].transform.SetParent(canvas.transform);
        }

        // Update the object info for the bounding box
        SetObjectInfo(objectInfo);
    }
}
