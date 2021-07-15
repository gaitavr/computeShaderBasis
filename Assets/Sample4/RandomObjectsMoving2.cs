using System.Linq;
using UnityEngine;

public class RandomObjectsMoving2 : MonoBehaviour
{
    [SerializeField]
    private int _objectsCount = 20;
    [SerializeField, Range(1, 50)]
    private float _spread = 30;
    [SerializeField, Range(0.1f, 2f)]
    private float _speed = 0.5f;
    [SerializeField]
    private ComputeShader _shader;

    [SerializeField]
    private Mesh _mesh;
    [SerializeField]
    private Material _material;
    [SerializeField]
    private float _scale = 1;

    //internal data
    private ComputeBuffer _resultBuffer;
    private ComputeBuffer _triangles;
    private ComputeBuffer _positions;
    private int _kernelIndex;
    private uint _threadGroupSize;
    private Bounds _bounds;

    private void Start()
    {
        _kernelIndex = _shader.FindKernel("Randomize");
        _shader.GetKernelThreadGroupSizes(_kernelIndex, out _threadGroupSize, out _, out _);

        _objectsCount *= (int)_threadGroupSize;
        
        _resultBuffer = new ComputeBuffer(_objectsCount, sizeof(float) * 3);
        
        var triangles = _mesh.triangles;
        _triangles = new ComputeBuffer(triangles.Length, sizeof(int));
        _triangles.SetData(triangles);
        
        var positions = _mesh.vertices.Select(p => p * _scale).ToArray();
        _positions = new ComputeBuffer(positions.Length, sizeof(float) * 3);
        _positions.SetData(positions);
        
        _shader.SetBuffer(_kernelIndex, "Positions", _resultBuffer);

        _material.SetBuffer("SphereLocations", _resultBuffer);
        _material.SetBuffer("Triangles", _triangles);
        _material.SetBuffer("Positions", _positions);

        //bounds for frustum culling (20 is a magic number (radius) from the compute shader)
        _bounds = new Bounds(Vector3.zero, Vector3.one * 20);
    }

    private void Update()
    {
        _shader.SetFloat("Time", Time.time * _speed);
        _shader.SetFloat("Spread", _spread);
        
        var threadGroups = (int) (_objectsCount / _threadGroupSize);
        _shader.Dispatch(_kernelIndex, threadGroups, 1, 1);

        //draw result
        Graphics.DrawProcedural(_material, _bounds, MeshTopology.Triangles, _triangles.count, _objectsCount);
    }

    private void OnDestroy()
    {
        _resultBuffer.Dispose();
        _triangles.Dispose();
        _positions.Dispose();
    }
}