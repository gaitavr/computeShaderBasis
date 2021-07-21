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

    private ComputeBuffer _positions;
    private ComputeBuffer _triangles;
    private ComputeBuffer _vertices;
    private int _kernelIndex;
    private uint _threadGroupSize;
    private Bounds _bounds;

    private void Start()
    {
        _kernelIndex = _shader.FindKernel("Randomize");
        _shader.GetKernelThreadGroupSizes(_kernelIndex, out _threadGroupSize, out _, out _);

        _objectsCount *= (int) _threadGroupSize;

        _positions = new ComputeBuffer(_objectsCount, sizeof(float) * 3);

        var triangles = _mesh.triangles;
        _triangles = new ComputeBuffer(_objectsCount, sizeof(int));
        _triangles.SetData(triangles);

        var vertices = _mesh.vertices;
        _vertices = new ComputeBuffer(_objectsCount, sizeof(float) * 3);
        _vertices.SetData(vertices);

        _shader.SetBuffer(_kernelIndex, "Positions", _positions);
        
        _material.SetBuffer("Positions", _positions);
        _material.SetBuffer("Triangles", _triangles);
        _material.SetBuffer("Vertices", _vertices);

        _bounds = new Bounds(Vector3.zero, Vector3.one * _speed);
    }

    private void Update()
    {
        _shader.SetFloat("Time", Time.time * _speed);
        _shader.SetFloat("Spread", _spread);
        
        var threadGroups = (int) (_objectsCount / _threadGroupSize);
        _shader.Dispatch(_kernelIndex, threadGroups, 1, 1);

        Graphics.DrawProcedural(_material, _bounds, 
            MeshTopology.Triangles, _triangles.count, _objectsCount);
    }

    private void OnDestroy()
    {
        _positions.Dispose();
    }
}