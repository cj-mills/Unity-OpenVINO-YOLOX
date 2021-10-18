using UnityEngine;
using System;
using System.Runtime.InteropServices;

public class Utils
{

    // Indicate that the members of the struct are laid out sequentially
    [StructLayout(LayoutKind.Sequential)]
    /// <summary>
    /// Stores the information for a single object
    /// </summary> 
    public struct Object
    {
        // The X coordinate for the top left bounding box corner
        public float x0;
        // The Y coordinate for the top left bounding box cornder
        public float y0;
        // The width of the bounding box
        public float width;
        // The height of the bounding box
        public float height;
        // The object class index for the detected object
        public int label;
        // The model confidence score for the object
        public float prob;

        public Object(float x0, float y0, float width, float height, int label, float prob)
        {
            this.x0 = x0;
            this.y0 = y0;
            this.width = width;
            this.height = height;
            this.label = label;
            this.prob = prob;
        }
    }

    /// <summary>
    /// The color coded, ordered list of object classes the model was trained to detect
    /// </summary>
    public static Tuple<string, Color>[] object_classes = new Tuple<string, Color>[] 
    {
        Tuple.Create("person",         new Color(0.000f, 0.447f, 0.741f)),
        Tuple.Create("bicycle",        new Color(0.850f, 0.325f, 0.098f)),
        Tuple.Create("car",            new Color(0.929f, 0.694f, 0.125f)),
        Tuple.Create("motorcycle",     new Color(0.494f, 0.184f, 0.556f)),
        Tuple.Create("airplane",       new Color(0.466f, 0.674f, 0.188f)),
        Tuple.Create("bus",            new Color(0.301f, 0.745f, 0.933f)),
        Tuple.Create("train",          new Color(0.635f, 0.078f, 0.184f)),
        Tuple.Create("truck",          new Color(0.300f, 0.300f, 0.300f)),
        Tuple.Create("boat",           new Color(0.600f, 0.600f, 0.600f)),
        Tuple.Create("traffic light",  new Color(1.000f, 0.000f, 0.000f)),
        Tuple.Create("fire hydrant",   new Color(1.000f, 0.500f, 0.000f)),
        Tuple.Create("stop sign",      new Color(0.749f, 0.749f, 0.000f)),
        Tuple.Create("parking meter",  new Color(0.000f, 1.000f, 0.000f)),
        Tuple.Create("bench",          new Color(0.000f, 0.000f, 1.000f)),
        Tuple.Create("bird",           new Color(0.667f, 0.000f, 1.000f)),
        Tuple.Create("cat",            new Color(0.333f, 0.333f, 0.000f)),
        Tuple.Create("dog",            new Color(0.333f, 0.667f, 0.000f)),
        Tuple.Create("horse",          new Color(0.333f, 1.000f, 0.000f)),
        Tuple.Create("sheep",          new Color(0.667f, 0.333f, 0.000f)),
        Tuple.Create("cow",            new Color(0.667f, 0.667f, 0.000f)),
        Tuple.Create("elephant",       new Color(0.667f, 1.000f, 0.000f)),
        Tuple.Create("bear",           new Color(1.000f, 0.333f, 0.000f)),
        Tuple.Create("zebra",          new Color(1.000f, 0.667f, 0.000f)),
        Tuple.Create("giraffe",        new Color(1.000f, 1.000f, 0.000f)),
        Tuple.Create("backpack",       new Color(0.000f, 0.333f, 0.500f)),
        Tuple.Create("umbrella",       new Color(0.000f, 0.667f, 0.500f)),
        Tuple.Create("handbag",        new Color(0.000f, 1.000f, 0.500f)),
        Tuple.Create("tie",            new Color(0.333f, 0.000f, 0.500f)),
        Tuple.Create("suitcase",       new Color(0.333f, 0.333f, 0.500f)),
        Tuple.Create("frisbee",        new Color(0.333f, 0.667f, 0.500f)),
        Tuple.Create("skis",           new Color(0.333f, 1.000f, 0.500f)),
        Tuple.Create("snowboard",      new Color(0.667f, 0.000f, 0.500f)),
        Tuple.Create("sports ball",    new Color(0.667f, 0.333f, 0.500f)),
        Tuple.Create("kite",           new Color(0.667f, 0.667f, 0.500f)),
        Tuple.Create("baseball bat",   new Color(0.667f, 1.000f, 0.500f)),
        Tuple.Create("baseball glove", new Color(1.000f, 0.000f, 0.500f)),
        Tuple.Create("skateboard",     new Color(1.000f, 0.333f, 0.500f)),
        Tuple.Create("surfboard",      new Color(1.000f, 0.667f, 0.500f)),
        Tuple.Create("tennis racket",  new Color(1.000f, 1.000f, 0.500f)),
        Tuple.Create("bottle",         new Color(0.000f, 0.333f, 1.000f)),
        Tuple.Create("wine glass",     new Color(0.000f, 0.667f, 1.000f)),
        Tuple.Create("cup",            new Color(0.000f, 1.000f, 1.000f)),
        Tuple.Create("fork",           new Color(0.333f, 0.000f, 1.000f)),
        Tuple.Create("knife",          new Color(0.333f, 0.333f, 1.000f)),
        Tuple.Create("spoon",          new Color(0.333f, 0.667f, 1.000f)),
        Tuple.Create("bowl",           new Color(0.333f, 1.000f, 1.000f)),
        Tuple.Create("banana",         new Color(0.667f, 0.000f, 1.000f)),
        Tuple.Create("apple",          new Color(0.667f, 0.333f, 1.000f)),
        Tuple.Create("sandwich",       new Color(0.667f, 0.667f, 1.000f)),
        Tuple.Create("orange",         new Color(0.667f, 1.000f, 1.000f)),
        Tuple.Create("broccoli",       new Color(1.000f, 0.000f, 1.000f)),
        Tuple.Create("carrot",         new Color(1.000f, 0.333f, 1.000f)),
        Tuple.Create("hot dog",        new Color(1.000f, 0.667f, 1.000f)),
        Tuple.Create("pizza",          new Color(0.333f, 0.000f, 0.000f)),
        Tuple.Create("donut",          new Color(0.500f, 0.000f, 0.000f)),
        Tuple.Create("cake",           new Color(0.667f, 0.000f, 0.000f)),
        Tuple.Create("chair",          new Color(0.833f, 0.000f, 0.000f)),
        Tuple.Create("couch",          new Color(1.000f, 0.000f, 0.000f)),
        Tuple.Create("potted plant",   new Color(0.000f, 0.167f, 0.000f)),
        Tuple.Create("bed",            new Color(0.000f, 0.333f, 0.000f)),
        Tuple.Create("dining table",   new Color(0.000f, 0.500f, 0.000f)),
        Tuple.Create("toilet",         new Color(0.000f, 0.667f, 0.000f)),
        Tuple.Create("tv",             new Color(0.000f, 0.833f, 0.000f)),
        Tuple.Create("laptop",         new Color(0.000f, 1.000f, 0.000f)),
        Tuple.Create("mouse",          new Color(0.000f, 0.000f, 0.167f)),
        Tuple.Create("remote",         new Color(0.000f, 0.000f, 0.333f)),
        Tuple.Create("keyboard",       new Color(0.000f, 0.000f, 0.500f)),
        Tuple.Create("cell phone",     new Color(0.000f, 0.000f, 0.667f)),
        Tuple.Create("microwave",      new Color(0.000f, 0.000f, 0.833f)),
        Tuple.Create("oven",           new Color(0.000f, 0.000f, 1.000f)),
        Tuple.Create("toaster",        new Color(0.000f, 0.000f, 0.000f)),
        Tuple.Create("sink",           new Color(0.143f, 0.143f, 0.143f)),
        Tuple.Create("refrigerator",   new Color(0.286f, 0.286f, 0.286f)),
        Tuple.Create("book",           new Color(0.429f, 0.429f, 0.429f)),
        Tuple.Create("clock",          new Color(0.571f, 0.571f, 0.571f)),
        Tuple.Create("vase",           new Color(0.714f, 0.714f, 0.714f)),
        Tuple.Create("scissors",       new Color(0.857f, 0.857f, 0.857f)),
        Tuple.Create("teddy bear",     new Color(0.000f, 0.447f, 0.741f)),
        Tuple.Create("hair drier",     new Color(0.314f, 0.717f, 0.741f)),
        Tuple.Create("toothbrush",     new Color(0.50f, 0.5f, 0f))
    };

}
