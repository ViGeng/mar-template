# Mobile Augmented Reality Template

## Quick Links

* [Sentis Doc](https://docs.unity3d.com/Packages/com.unity.sentis@1.2/manual/create-an-input-tensor.html)
* [Unity Huggingface](https://huggingface.co/unity)
* [Unity Render Streaming](https://docs.unity3d.com/Packages/com.unity.renderstreaming@3.1/manual/index.html)

some refs:
* [rtp streaming](https://blog.csdn.net/qq_44983147/article/details/124276256)

## Roadmap

- [x] A demo scene locally detecting objects using camera feed (base on FaceDetection)
  - [x] support video/webcamera feed
  - [x] save video feed to disk
  - [ ] beautify the UI (bounding box, fps, etc)
- [x] webRTC streaming to server
    - [x] unity streaming client (broadcast)
    - [ ] edge server receive stream and processing
    - [ ] streaming result back to client
- [x] WebCamera/Video face/object detection
- [ ] speech to text scene demo
- [ ] add docs for each scene

## Requirements

* Unity Packages
    ```
    com.unity.renderstreaming
    com.unity.sentis
    ```
Unity=2023.2.0b17
## Scene Overview

| Scene                                                    | Function                                                                                                                         | Preview                                                     |
|----------------------------------------------------------|:---------------------------------------------------------------------------------------------------------------------------------|-------------------------------------------------------------|
| [CameraBackground](Assets/Scenes/CameraBackground.unity) | Realtime camera feed on the screen                                                                                               |                                                             |
| [PhoneDemo](Assets/Scenes/PhoneDemo.unity)               | Realtime depth estimation for camera [ref](https://github.com/Unity-Technologies/sentis-samples/tree/main/DepthEstimationSample) | ![image info](./Documentation/main.gif)                     |
| [ObjectDetection](Assets/Scenes/ObjectDetection.unity)   | Detection of video feed.[ref](https://huggingface.co/unity/sentis-YOLOv8n)                                                       | ![object_detection.gif](Documentation/object_detection.gif) |
| [FaceDetection](Assets/Scenes/FaceDetection.unity)       | Detection of faces in video/WebCamera feed. [ref](https://huggingface.co/unity/sentis-blaze-face/tree/main)                      | ![face_detection.gif](Documentation/face_detection.gif)     |
| [WebRTC-Streaming](Assets/Scenes/streaming-test.unity)   | Streaming videos to other client/browser                                                                                         | ![img.png](Documentation/streaming.png)                     |
