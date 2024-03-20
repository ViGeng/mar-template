using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Sentis;
using Unity.Sentis.Layers;

public class MNIST : MonoBehaviour
{
    public Texture2D inputTexture;
    public ModelAsset modelAsset;
    Model runtimeModel;
    IWorker worker;
    public float[] results;
    
    // Start is called before the first frame update
    void Start()
    {
        SetupModel();

        SetupEngine();

        // Create input data as a tensor
        using Tensor inputTensor = TextureConverter.ToTensor(inputTexture, width: 28, height: 28, channels: 1);
        
        // Run the model with the input data
        worker.Execute(inputTensor);

        // Get the result
        using TensorFloat outputTensor = worker.PeekOutput() as TensorFloat;

        // Move the tensor data to the CPU before reading it
        outputTensor.MakeReadable();

        results = outputTensor.ToReadOnlyArray();
        
    }

    private void SetupEngine()
    {
        // Create an engine
        worker = WorkerFactory.CreateWorker(BackendType.GPUCompute, runtimeModel);
    }

    private void SetupModel()
    {
        // Create the runtime model
        runtimeModel = ModelLoader.Load(modelAsset);

        // Add softmax layer to end of model instead of non-softmaxed output
        string softmaxOutputName = "Softmax_Output";
        runtimeModel.AddLayer(new Softmax(softmaxOutputName, runtimeModel.outputs[0]));
        runtimeModel.outputs[0] = softmaxOutputName;
    }
    
    void OnDisable()
    {
        // Tell the GPU we're finished with the memory the engine used
        worker.Dispose();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
