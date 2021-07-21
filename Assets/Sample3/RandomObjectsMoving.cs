using UnityEngine;


public class RandomObjectsMoving : MonoBehaviour
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
    private GameObject _prefab;

    private ComputeBuffer _buffer;
    private int _kernelIndex;
    private uint _threadGroupSize;

    private Vector3[] _resultPositions;
    private Transform[] _objects;

    private void Start()
    {
        _kernelIndex = _shader.FindKernel("Randomize");
        _shader.GetKernelThreadGroupSizes(_kernelIndex, out _threadGroupSize, out _, out _);

        _objectsCount *= (int) _threadGroupSize;

        _buffer = new ComputeBuffer(_objectsCount, sizeof(float) * 3);
        _resultPositions = new Vector3[_objectsCount];
        _objects = new Transform[_objectsCount];

        for (var i = 0; i < _objectsCount; i++)
        {
            _objects[i] = Instantiate(_prefab, transform).transform;
            _objects[i].gameObject.SetActive(true);
        }
    }

    private void Update()
    {
        _shader.SetFloat("Time", Time.time * _speed);
        _shader.SetFloat("Spread", _spread);
        _shader.SetBuffer(_kernelIndex, "Positions", _buffer);

        var threadGroups = (int) (_objectsCount / _threadGroupSize);
        _shader.Dispatch(_kernelIndex, threadGroups, 1, 1);

        _buffer.GetData(_resultPositions);

        for (var i = 0; i < _objects.Length; i++)
            _objects[i].localPosition = _resultPositions[i];
    }

    private void OnDestroy()
    {
        _buffer.Dispose();
    }
}