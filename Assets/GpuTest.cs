using UnityEngine;

public class GpuTest : MonoBehaviour
{
    [SerializeField]
    private CubesContainer _cubesContainer;
    [SerializeField]
    private int _iterations = 10;
    [SerializeField]
    private ComputeShader _shader;
    
    private int _kernelIndex;
    private uint _threadGroupSize;
    private ComputeBuffer _colorBuffer;
    private Vector4[] _colors;

    private int BufferSize => _cubesContainer.Width * _cubesContainer.Height;

    private void Start()
    {
        _colorBuffer = new ComputeBuffer(BufferSize,
            sizeof(float) * 4);
        _colors = new Vector4[BufferSize];
        
        _kernelIndex = _shader.FindKernel("Regenerate");
        _shader.GetKernelThreadGroupSizes(_kernelIndex, out _threadGroupSize, 
            out _, out _);
        
        for (int i = 0; i < _colors.Length; i++)
        {
            var x = i % _cubesContainer.Width;
            var y = i / _cubesContainer.Width;
           Debug.LogError($"{x}: {y} - {i}");
        }
        
    }
        
    private void OnGUI()
    {
        if (GUI.Button(new Rect(350, 50, 200, 100), "GPU"))
        {
            Regenerate();
        }
    }

    private void Regenerate()
    {
        _shader.SetBuffer(_kernelIndex, "Colors", _colorBuffer);
        _shader.SetFloat("Time", Time.realtimeSinceStartup);
        _shader.SetInt("Width", _cubesContainer.Width);
        _shader.SetInt("Height", _cubesContainer.Height);
        
        var threadGroups = (int) ((BufferSize + (_threadGroupSize - 1)) / _threadGroupSize);
        _shader.Dispatch(_kernelIndex, threadGroups, 1, 1);
        
        _colorBuffer.GetData(_colors);
        for (int i = 0; i < _colors.Length; i++)
        {
            var x = i % _cubesContainer.Width;
            var y = i / _cubesContainer.Width;
            _cubesContainer.SetColor(x, y, _colors[i]);
        }
    }
    
    private void OnDestroy()
    {
        _colorBuffer.Dispose();
    }
}