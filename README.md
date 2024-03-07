# Mobile Augmented Reality Template


## Roadmap

- [ ] save video feed to disk
- [ ] webRTC or other streaming to server

## Scene Overview
| Scene                                                    | Function                                                                                                                         | Gif Preview                                                 |
|----------------------------------------------------------|----------------------------------------------------------------------------------------------------------------------------------|-------------------------------------------------------------|
| [CameraBackground](Assets/Scenes/CameraBackground.unity) | Realtime camera feed on the screen                                                                                               |                                                             |
| [PhoneDemo](Assets/Scenes/PhoneDemo.unity)               | Realtime depth estimation for camera [ref](https://github.com/Unity-Technologies/sentis-samples/tree/main/DepthEstimationSample) | ![image info](./Documentation/main.gif)                     |
| [ObjectDetection](Assets/Scenes/ObjectDetection.unity)   | Detection of video feed.[ref](https://huggingface.co/unity/sentis-YOLOv8n)                                                       | ![object_detection.gif](Documentation/object_detection.gif) |
| [FaceDetection](Assets/Scenes/FaceDetection.unity)       | Detection of faces in video/WebCamera feed. [ref](https://huggingface.co/unity/sentis-blaze-face/tree/main)                      | ![face_detection.gif](Documentation/face_detection.gif)     |

