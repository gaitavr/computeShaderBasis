using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Fractal : MonoBehaviour
{
    [SerializeField]
    private ComputeShader _shader;

    [SerializeField]
    private RawImage _debugImage;

    private int _kernelIndex;
    private uint _threadX;
    private RenderTexture _resultTexture;

    private void Awake()
    {
        _resultTexture = new RenderTexture(1920, 1080, 32)
        {
            enableRandomWrite = true
        };
        _resultTexture.Create();

        _debugImage.texture = _resultTexture;

        _kernelIndex = _shader.FindKernel("Fractal");
        _shader.GetKernelThreadGroupSizes(_kernelIndex, out _threadX, out _, 
            out _);
        _shader.SetTexture(_kernelIndex, "Result", _resultTexture);
    }

    private void Update()
    {
        _shader.SetFloat("Time", Time.timeSinceLevelLoad);
        _shader.SetVector("Resolution", new Vector2(Screen.width, Screen.height));
        
        var threadGroups = (int)(Screen.width / _threadX);
        _shader.Dispatch(_kernelIndex, threadGroups, threadGroups, 
            1);
    }
}
