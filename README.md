# Mobile Augmented Reality Template

This is a template for a mobile AR experience. It is forked from [Depth Estimation Sample](https://github.com/Unity-Technologies/sentis-samples/tree/main/DepthEstimationSample).
 
Realtime per pixel image to depth estimation.


![image info](./Documentation/main.gif)


![image info](./Documentation/gameplay.png)

## Runtime Inference

To solve this problem we leverage a small Neural Network.

It takes as input the camera stream.

After running inference we get for every pixel of the input image a depth value.

We use this to feedback into the gameplay loop

![image info](./Documentation/runtime-inference.png)

## Optimization

To keep this within budget we split the model inference over 2 frame.

By running a lower frequency as the refresh rate, we avoid the phone to overheat

![image info](./Documentation/runtime-optimization.png)


## Video Tutorial

[![IMAGE ALT TEXT HERE](../Documentation/video-image.png)](https://youtu.be/xpp2f6ITvvE)
