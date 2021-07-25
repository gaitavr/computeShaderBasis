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
    private uint _threadGroupSizeX;

    private string _savePath;
    
    private void Start()
    {
        _savePath = Application.persistentDataPath + $"/samples/";
        
        _kernelIndex = _shader.FindKernel("DownSample");
        _shader.GetKernelThreadGroupSizes(_kernelIndex, out _threadGroupSizeX, 
            out _, out _);
    }
        
    private void OnGUI()
    {
        if (GUI.Button(new Rect(250, 50, 200, 100), "DownSample"))
        {
            DownSample();
            UnityEditor.EditorUtility.RevealInFinder(_savePath);
        }
    }

    private void DownSample()
    {
        var width = _sourceTexture.width;
        var height = _sourceTexture.height;
        var factor = 2;
        
        for (int i = 0; i < _samples; i++)
        {
            width /= factor;
            height /= factor;
            factor *= 2;

            var workTexture = new RenderTexture(width, height, 32);
            workTexture.Create();
            
            _shader.SetTexture(_kernelIndex, "Texture", workTexture);
            var yGroupCount = _sourceTexture.height / _sourceTexture.width * _threadGroupSizeX;
            _shader.Dispatch(_kernelIndex, (int)_threadGroupSizeX, (int)yGroupCount, 1);
            
            workTexture.Release();
            
            var textureToSave = new Texture2D(workTexture.width,workTexture.height, TextureFormat.RGB24, false);
            RenderTexture.active = workTexture;
            textureToSave.ReadPixels( new Rect(0, 0, workTexture.width,workTexture.height), 0, 0);
            RenderTexture.active = null;
     
            var bytes = textureToSave.EncodeToPNG();
            Destroy(textureToSave);
            File.WriteAllBytes($"{_savePath}Sample{i}.png", bytes);
        }
    }
}