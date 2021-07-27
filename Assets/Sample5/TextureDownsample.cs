using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class TextureDownsample : MonoBehaviour
{
    [SerializeField] 
    private Texture2D _sourceTexture;

    [SerializeField, Range(1, 20)] 
    private int _samples = 5;
    [SerializeField]
    private ComputeShader _shader;
    
    private int _kernelIndex;
    private uint _threadGroupSize;

    private string _savePath;
    
    private void Start()
    {
        _savePath = Application.persistentDataPath + $"/samples/";
        if (Directory.Exists(_savePath) == false)
            Directory.CreateDirectory(_savePath);
        
        _kernelIndex = _shader.FindKernel("DownSample");
        _shader.GetKernelThreadGroupSizes(_kernelIndex, out _threadGroupSize, 
            out _, out _);
    }
        
    private void OnGUI()
    {
        if (GUI.Button(new Rect(250, 50, 200, 100), "DownSample"))
        {
            foreach (var file in Directory.EnumerateFiles(_savePath))
            {
                File.Delete(file);
            }
            DownSample();
            UnityEditor.EditorUtility.RevealInFinder(_savePath);
        }
    }

    private void DownSample()
    {
        var factor = 2;
        
        for (int i = 0; i < _samples; i++)
        {
            var width = _sourceTexture.width / factor;
            var height = _sourceTexture.height / factor;
            
            if(width < _threadGroupSize || height < _threadGroupSize)
                break;
            
            var workTexture = new RenderTexture(width, height, 32)
            {
                enableRandomWrite = true
            };
            workTexture.Create();
            
            var computeBuffer = new ComputeBuffer(4, sizeof(int));
            computeBuffer.SetData(new List<int>()
            {
                _sourceTexture.width, _sourceTexture.height,
                workTexture.width, workTexture.height
            });
            
            _shader.SetTexture(_kernelIndex, "SourceTexture", _sourceTexture);
            _shader.SetTexture(_kernelIndex, "DestinationTexture", workTexture);
            _shader.SetBuffer(_kernelIndex, "Resolutions", computeBuffer);
            _shader.Dispatch(_kernelIndex, workTexture.width/(int)_threadGroupSize, 
                workTexture.height/(int)_threadGroupSize, 1);
            
            var textureToSave = new Texture2D(workTexture.width,workTexture.height, TextureFormat.RGB24, false);
            RenderTexture.active = workTexture;
            textureToSave.ReadPixels( new Rect(0, 0, workTexture.width,workTexture.height), 0, 0);
            RenderTexture.active = null;
     
            var bytes = textureToSave.EncodeToJPG();
            File.WriteAllBytes($"{_savePath}Sample{factor}.jpg", bytes);
            
            Destroy(textureToSave);
            workTexture.Release();
            computeBuffer.Dispose();
            
            factor *= 2;
        }
    }
}