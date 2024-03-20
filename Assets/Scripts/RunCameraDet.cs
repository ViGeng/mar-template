using System.Collections.Generic;
using UnityEngine;
using Unity.Sentis;
using UnityEngine.Video;
using UnityEngine.UI;
using Lays = Unity.Sentis.Layers;

/*
 *                   obj det Inference
 *                   ====================
 *                   
 * Basic inference script for yolov8
 * 
 * Put this script on the Main Camera
 * Put yolov8n.sentis in the Assets/StreamingAssets folder
 * Create a RawImage in the scene
 * Put a link to that image in previewUI
 * Put a video in Assets/StreamingAssets folder and put the name of it int videoName
 * Or put a test image in inputImage
 * Set inputType to appropriate input
 */


public class RunCameraDet : MonoBehaviour
{
    //Drag a link to a raw image here:
    public RawImage previewUI = null;

    // Put your bounding box sprite image here
    public Sprite faceTexture;

    // 6 optional sprite images (left eye, right eye, nose, mouth, left ear, right ear)
    public Sprite[] markerTextures;

    public string videoName = "face.mp4";

    // Link the classes.txt here:
    public TextAsset labelsAsset;
    public Font font;
    private string[] labels;
    List<GameObject> boxPool = new List<GameObject>();
    // 
    public Texture2D inputImage = null;

    public InputType inputType = InputType.Video;

    Vector2Int resolution = new Vector2Int(640, 640);
    WebCamTexture webcam;
    VideoPlayer video;

    const BackendType backend = BackendType.GPUCompute;

    RenderTexture targetTexture;
    public enum InputType { Image, Video, Webcam };
   
    
    //Some adjustable parameters for the model
    [SerializeField, Range(0, 1)] float iouThreshold = 0.5f;
    [SerializeField, Range(0, 1)] float scoreThreshold = 0.5f;
    int maxOutputBoxes = 64;

    IWorker worker;

    Ops ops;
    ITensorAllocator allocator;

    Model model;

    //webcam device name:
    private string deviceName;

    bool closing = false;

    public struct BoundingBox
    {
        public float centerX;
        public float centerY;
        public float width;
        public float height;
        public string label;
    }

    void Start()
    {
        allocator = new TensorCachingAllocator();

        //(Note: if using a webcam on mobile get permissions here first)

        targetTexture = new RenderTexture(resolution.x, resolution.y, 0);

        SetupInput();
        
        SetupDetectionModel();

        SetupEngine();
    }
    
    void SetupDetectionModel()
    {
        Debug.Log("Loading model...");
        
        //Load model
        model = ModelLoader.Load(Application.streamingAssetsPath + "/yolov8n.sentis");

        //The classes are also stored here in JSON format:
        Debug.Log($"Class names: \n{model.Metadata["names"]}");

        //We need to add some layers to choose the best boxes with the NMSLayer
        
        //Set constants
        model.AddConstant(new Lays.Constant("0", new int[] { 0 }));
        model.AddConstant(new Lays.Constant("1", new int[] { 1 }));
        model.AddConstant(new Lays.Constant("4", new int[] { 4 }));


        model.AddConstant(new Lays.Constant("classes_plus_4", new int[] { 80 + 4 }));
        model.AddConstant(new Lays.Constant("maxOutputBoxes", new int[] { maxOutputBoxes }));
        model.AddConstant(new Lays.Constant("iouThreshold", new float[] { iouThreshold }));
        model.AddConstant(new Lays.Constant("scoreThreshold", new float[] { scoreThreshold }));
       
        //Add layers
        model.AddLayer(new Lays.Slice("boxCoords0", "output0", "0", "4", "1")); 
        model.AddLayer(new Lays.Transpose("boxCoords", "boxCoords0", new int[] { 0, 2, 1 }));
        model.AddLayer(new Lays.Slice("scores0", "output0", "4", "classes_plus_4", "1")); 
        model.AddLayer(new Lays.ReduceMax("scores", new[] { "scores0", "1" }));
        model.AddLayer(new Lays.ArgMax("classIDs", "scores0", 1));

        model.AddLayer(new Lays.NonMaxSuppression("NMS", "boxCoords", "scores",
            "maxOutputBoxes", "iouThreshold", "scoreThreshold",
            centerPointBox: Lays.CenterPointBox.Center
        ));

        model.outputs.Clear();
        model.AddOutput("boxCoords");
        model.AddOutput("classIDs");
        model.AddOutput("NMS");
    }

    void SetupInput()
    {
        //Parse neural net labels
        labels = labelsAsset.text.Split('\n');
        
        switch (inputType)
        {
            case InputType.Webcam:
                {
                    deviceName = WebCamTexture.devices[0].name;
                    webcam = new WebCamTexture(deviceName, resolution.x, resolution.y);
                    webcam.requestedFPS = 30;
                    webcam.Play();
                    break;
                }
            case InputType.Video:
                {
                    video = gameObject.AddComponent<VideoPlayer>();//new VideoPlayer();
                    video.renderMode = VideoRenderMode.APIOnly;
                    video.source = VideoSource.Url;
                    video.url = Application.streamingAssetsPath + "/"+videoName;
                    video.isLooping = true;
                    video.Play();
                    break;
                }
            default:
                {
                    Graphics.Blit(inputImage, targetTexture);
                }
                break;
        }
    }

    void Update()
    {
        if (inputType == InputType.Webcam)
        {
            // Format video input
            if (!webcam.didUpdateThisFrame) return;

            var aspect1 = (float)webcam.width / webcam.height;
            var aspect2 = (float)resolution.x / resolution.y;
            var gap = aspect2 / aspect1;

            var vflip = webcam.videoVerticallyMirrored;
            var scale = new Vector2(gap, vflip ? -1 : 1);
            var offset = new Vector2((1 - gap) / 2, vflip ? 1 : 0);

            Graphics.Blit(webcam, targetTexture, scale, offset);
        }
        if (inputType == InputType.Video)
        {
            var aspect1 = (float)video.width / video.height;
            var aspect2 = (float)resolution.x / resolution.y;
            var gap = aspect2 / aspect1;

            var vflip = false;
            var scale = new Vector2(gap, vflip ? -1 : 1);
            var offset = new Vector2((1 - gap) / 2, vflip ? 1 : 0);
            Graphics.Blit(video.texture, targetTexture, scale, offset);
        }
        if (inputType == InputType.Image)
        {
            Graphics.Blit(inputImage, targetTexture);
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            closing = true;
            Application.Quit();
        }

        if (Input.GetKeyDown(KeyCode.P))
        {
            previewUI.enabled = !previewUI.enabled;
        }
    }


    void LateUpdate()
    {
        if (!closing)
        {
            RunInference(targetTexture);
        }
    }

    //Calculate the centers of the grid squares for two 16x16 grids and six 8x8 grids
    //The positions of the faces are given relative to these "anchor points"
    float[] GetGridBoxCoords()
    {
        var offsets = new float[896 * 4];
        int n = 0;
        AddGrid(offsets, 16, 2, 8, ref n);
        AddGrid(offsets, 8, 6, 16, ref n);
        return offsets;
    }
    void AddGrid(float[] offsets, int rows, int repeats, int cellWidth, ref int n)
    {
        for (int j = 0; j < repeats * rows * rows; j++)
        {
            offsets[n++] = cellWidth * ((j / repeats) % rows - (rows - 1) * 0.5f);
            offsets[n++] = cellWidth * ((j / repeats / rows) - (rows - 1) * 0.5f);
            n += 2;
        }
    }

    public void SetupEngine()
    {
        Debug.Log("Creating engine...");
        worker = WorkerFactory.CreateWorker(backend, model);
        ops = WorkerFactory.CreateOps(backend, allocator);
    }

    void ExecuteML(Texture source)
    {
        var transform = new TextureTransform();
        transform.SetDimensions(resolution.x, resolution.y, 3);
        // transform.SetTensorLayout(0, 3, 1, 2);
        using var image0 = TextureConverter.ToTensor(source, transform);
        
        // Pre-process the image to make input in range (-1..1)
        using var image = ops.Mad(image0, 2f, -1f);
        Debug.Log("Input shape: " + image.shape.ToString());
        
        worker.Execute(image);

        var boxCoords = worker.PeekOutput("boxCoords") as TensorFloat;
        var NMS = worker.PeekOutput("NMS") as TensorInt;
        var classIDs = worker.PeekOutput("classIDs") as TensorInt;

        using var boxIDs = ops.Slice(NMS, new int[] { 2 }, new int[] { 3 }, new int[] { 1 }, new int[] { 1 });
        using var boxIDsFlat = boxIDs.ShallowReshape(new TensorShape(boxIDs.shape.length)) as TensorInt;
        using var output = ops.Gather(boxCoords, boxIDsFlat, 1);
        using var labelIDs = ops.Gather(classIDs, boxIDsFlat, 2);
        
        output.MakeReadable();
        labelIDs.MakeReadable();
        Debug.Log("Output shape: " + output.shape.ToString());
        Debug.Log("Label shape: " + labelIDs.shape.ToString());
        Debug.Log("labels:" + labels.ToString());

        float displayWidth = previewUI.rectTransform.rect.width;
        float displayHeight = previewUI.rectTransform.rect.height;

        float scaleX = displayWidth / resolution.x;
        float scaleY = displayHeight / resolution.y;
        
        ClearAnnotations();
        
        //Draw the bounding boxes
        for (int n = 0; n < output.shape[1]; n++)
        {
            var box = new BoundingBox
            {
                centerX = output[0, n, 0] * scaleX - displayWidth / 2,
                centerY = output[0, n, 1] * scaleY - displayHeight / 2,
                width = output[0, n, 2] * scaleX,
                height = output[0, n, 3] * scaleY,
                label = labels[labelIDs[0, 0, n]],
            };
            Debug.Log("BBox: " + "NO." + n + " " +  box.centerX + " " + box.centerY + " " + box.width + " " + box.height + " " + box.label);
            DrawBox(box, n);
        }
        
    }

    void RunInference(Texture input)
    {
        // Face detection
        ExecuteML(input);
        previewUI.texture = input;
    }

    public void DrawBox(BoundingBox box , int id)
    {
        //Create the bounding box graphic or get from pool
        GameObject panel;
        if (id < boxPool.Count)
        {
            panel = boxPool[id];
            panel.SetActive(true);
        }
        else
        {
            panel = CreateNewBox(Color.yellow);
        }
        //Set box position
        panel.transform.localPosition = new Vector3(box.centerX, -box.centerY);

        //Set box size
        RectTransform rt = panel.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(box.width, box.height);
        
        //Set label text
        var label = panel.GetComponentInChildren<Text>();
        label.text = box.label;
    }

    public GameObject CreateNewBox(Color color)
    {
        //Create the box and set image

        var panel = new GameObject("ObjectBox");
        panel.AddComponent<CanvasRenderer>();
        Image img = panel.AddComponent<Image>();
        img.color = color;
        img.sprite = markerTextures[0];
        img.type = Image.Type.Sliced;
        panel.transform.SetParent(previewUI.transform, false);

        //Create the label

        var text = new GameObject("ObjectLabel");
        text.AddComponent<CanvasRenderer>();
        text.transform.SetParent(panel.transform, false);
        Text txt = text.AddComponent<Text>();
        txt.font = font;
        txt.color = color;
        txt.fontSize = 40;
        txt.horizontalOverflow = HorizontalWrapMode.Overflow;

        RectTransform rt2 = text.GetComponent<RectTransform>();
        rt2.offsetMin = new Vector2(20, rt2.offsetMin.y);
        rt2.offsetMax = new Vector2(0, rt2.offsetMax.y);
        rt2.offsetMin = new Vector2(rt2.offsetMin.x, 0);
        rt2.offsetMax = new Vector2(rt2.offsetMax.x, 30);
        rt2.anchorMin = new Vector2(0, 0);
        rt2.anchorMax = new Vector2(1, 1);

        boxPool.Add(panel);
        return panel;
    }

    public void ClearAnnotations()
    {
        foreach(var box in boxPool)
        {
            box.SetActive(false);
        }
    }

    void CleanUp()
    {
        closing = true;
        ops?.Dispose();
        allocator?.Dispose();
        if (webcam) Destroy(webcam);
        if (video) Destroy(video);
        RenderTexture.active = null;
        targetTexture.Release();
        worker?.Dispose();
        worker = null;
    }

    void OnDestroy()
    {
        CleanUp();
    }

}

