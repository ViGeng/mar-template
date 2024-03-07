using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class BackgroundWebcam : MonoBehaviour
{
    private bool camAvailable;
    private WebCamTexture backCam;
    private Texture defaultBackground;
    
    public RawImage background;
    public AspectRatioFitter fit;
    
    void Start()
    {
        WebCamDevice[] devices = WebCamTexture.devices;
        if (devices.Length == 0)
        {
            Debug.Log("No camera detected");
            camAvailable = false;
            return;
        }
        camAvailable = true;
        Debug.Log("Camera detected");
        Debug.Log(devices[0].name);
        
        // with a camera available, we can set up the webcam texture
        backCam = new WebCamTexture(devices[0].name, Screen.width, Screen.height);
        backCam.Play();
        background.texture = backCam;
    }
    
    private void Update()
    {
        if (!camAvailable)
            return;
        
        float ratio = (float)backCam.width / (float)backCam.height;
        fit.aspectRatio = ratio;
        
        float scaleY = backCam.videoVerticallyMirrored ? -1f : 1f;
        background.rectTransform.localScale = new Vector3(1f, scaleY, 1f);
        
        int orient = -backCam.videoRotationAngle;
        background.rectTransform.localEulerAngles = new Vector3(0, 0, orient);
    }
}