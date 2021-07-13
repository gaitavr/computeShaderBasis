using UnityEngine;

public class CubesContainer : MonoBehaviour
{
    [SerializeField]
    private Renderer _prefab;

    [SerializeField]
    private int _resolutionDivider = 5;
    
    private Vector2Int _resolution;

    public int Width => _resolution.x;
    public int Height => _resolution.y;
    
    private Renderer[,] _cubes;

    private static readonly int Color = Shader.PropertyToID("_Color");
    
    private void Awake()
    {
        _resolution = new Vector2Int(Screen.width / _resolutionDivider,
            Screen.height / _resolutionDivider);
        _cubes = new Renderer[_resolution.x, _resolution.y];
        for (int x = 0; x < _resolution.x; x++)
        {
            for (int y = 0; y < _resolution.y; y++)
            {
                var prefab = Instantiate(_prefab, transform);
                prefab.gameObject.SetActive(true);
                prefab.transform.position = new Vector3(x, y);
                _cubes[x, y] = prefab;
            }
        }
    }

    public void SetColor(int x, int y, Color color)
    {
        _cubes[x, y].material.SetColor(Color, color);
    }
}