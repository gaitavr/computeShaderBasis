using UnityEngine;

public class CpuTest : MonoBehaviour
{
    [SerializeField]
    private CubesContainer _cubesContainer;
    [SerializeField]
    private int _iterations = 10;
    
    private void OnGUI()
    {
        if (GUI.Button(new Rect(50, 50, 200, 100), "CPU"))
        {
            Regenerate();
        }
    }
    
    private void Regenerate()
    {
        var width = _cubesContainer.Width;
        var height = _cubesContainer.Height;
        for (int i = 0; i < _iterations; i++)
        {
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    var uv = new Vector2((float)x / width, (float)y / height);
                    float displacement = Pattern(uv);
                    
                    var color = new Color(displacement * 1.2f, 0.2f, displacement * 5.0f, 
                        1.0f);
                    color.a = Mathf.Min(color.r * 0.25f, 1.0f);
                    _cubesContainer.SetColor(x, y, color);
                }
            }
        }
    }

    private float Pattern(Vector2 uv)
    {
        var fbmQ = Fbm(uv);
        var q = new Vector2(fbmQ, fbmQ);
        var fbmR = Fbm(uv + 14.0f * q + new Vector2(1.7f, 9.2f));
        var r = new Vector2(fbmR, fbmR);
        r.x += Time.timeSinceLevelLoad * 0.15f;
        r.y += Time.timeSinceLevelLoad * 0.15f;
        return Fbm(uv + r);
    }
    
    private static readonly Vector4 _rotation = new Vector4(0.8f, -0.6f, 0.6f, 0.8f);
    
    private float Fbm(Vector2 p)
    {
        float f = 0.0f;
        const int octaves = 4;
        float m = 0.5f;
        
        for (int i = 0; i < octaves; i++)
        {
            f += m * Noise(p);
            p = _rotation * p * 2;
            m *= 0.5f;
        }
        f += m * Noise(p);
        
        return f / 0.769f;
    }

    private float Noise(Vector2 p) 
    {
        Vector2 ip = new Vector2(Mathf.Floor(p.x), Mathf.Floor(p.y));
        Vector2 u = new Vector2(Fract(p.x), Fract(p.y));
        u = u * u * (new Vector2(3.0f, 3.0f) - 2.0f * u);

        float res = Mathf.Lerp(
            Mathf.Lerp(Rand(ip), Rand(ip + new Vector2(1.0f, 0.0f)), u.x),
            Mathf.Lerp(Rand(ip+ new Vector2(0.0f, 1.0f)), 
                Rand(ip + new Vector2(1.0f, 1.0f)), u.x), u.y);
        return res * res;
    }
    
    private float Rand(Vector2 n) 
    {
        return Fract(Mathf.Sin(Vector2.Dot(n, new Vector2(12.9898f, 4.1414f))) 
                     * 43758.5453f);
    }

    private float Fract(float value)
    {
        return value % 1;
    }
}
