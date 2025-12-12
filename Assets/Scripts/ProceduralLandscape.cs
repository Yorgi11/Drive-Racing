using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// - Flat start, then smooth Perlin-based hills
/// - Fractal noise (multiple octaves) for more interesting shapes
/// - Mesh fill under the road down to a flat baseline
/// - EdgeCollider2D matches the road surface
/// </summary>
[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(EdgeCollider2D))]
public class ProceduralLandscape : MonoBehaviour
{
    [Header("Bezier Path To Follow")]
    [SerializeField] private Bezier2D m_bezierPath;

    [Header("Road Length & Resolution")]
    [SerializeField] private int m_segmentCount = 200;              // number of segments (points = count + 1)
    [SerializeField] private float m_segmentWidth = 1f;             // horizontal spacing between points

    [Header("Height Settings")]
    [SerializeField] private float m_baseHeight = 0f;               // where the road roughly sits vertically
    [SerializeField] private float m_amplitude = 5f;                // how tall the hills are (peak above base)

    [Header("Flat Start")]
    [SerializeField] private int m_flatStartSegments = 15;          // how many segments are perfectly flat
    [SerializeField] private int m_flatToNoiseBlendSegments = 10;   // how many segments to blend from flat into hills

    [Header("Noise (Fractal / Multi-Octave)")]
    [SerializeField] private float m_baseFrequency = 0.05f;         // main frequency (lower = wider hills)
    [SerializeField] private int m_octaves = 3;                     // number of noise octaves
    [SerializeField] private float m_lacunarity = 2.0f;             // frequency multiplier per octave
    [SerializeField] private float m_persistence = 0.5f;            // amplitude multiplier per octave

    [Tooltip("Random seed for reproducible roads")]
    [SerializeField] private int m_seed = 12345;
    [SerializeField] private bool m_useRandomSeed = false;

    [Header("Bottom Fill")]
    [SerializeField] private float m_bottomDepth = 20f;

    private MeshFilter m_meshFilter;
    private EdgeCollider2D m_edgeCollider;
    List<Vector2> m_surfacePoints = new();
    public List<Vector2> SurfacePoints { get { return m_surfacePoints; } }
    private void Awake()
    {
        m_meshFilter = GetComponent<MeshFilter>();
        m_edgeCollider = GetComponent<EdgeCollider2D>();
        Generate();
    }
    [ContextMenu("Regenerate Road")]
    public void Generate()
    {
        if (m_useRandomSeed) m_seed = Random.Range(int.MinValue, int.MaxValue);
        m_surfacePoints = GenerateSurfacePoints();
        BuildMesh(m_surfacePoints);
        ApplyCollider(m_surfacePoints);
    }
    private List<Vector2> GenerateSurfacePoints()
    {
        var points = new List<Vector2>(m_segmentCount + 1);
        System.Random rng = new(m_seed);
        float noiseOffsetX = rng.Next(-100000, 100000);
        float noiseOffsetY = rng.Next(-100000, 100000);
        for (int i = 0; i <= m_segmentCount; i++)
        {
            float tCurve = (float)i / m_segmentCount;
            Vector3 basePoint;
            if (m_bezierPath != null) basePoint = m_bezierPath.GetPointOnCurves(tCurve);
            else basePoint = new Vector3(i * m_segmentWidth, m_baseHeight, 0f);

            float x = basePoint.x;
            float baselineY = basePoint.y;

            float noiseValue = FractalPerlin(
                (x + noiseOffsetX) * m_baseFrequency,
                noiseOffsetY,
                m_octaves,
                m_lacunarity,
                m_persistence
            );
            float noisyY = baselineY + noiseValue * m_amplitude;

            float y;
            if (i < m_flatStartSegments) y = baselineY;
            else if (i < m_flatStartSegments + m_flatToNoiseBlendSegments)
            {
                int blendIndex = i - m_flatStartSegments;
                float tBlend = Mathf.Clamp01(blendIndex / (float)m_flatToNoiseBlendSegments);
                y = Mathf.Lerp(baselineY, noisyY, tBlend);
            }
            else y = noisyY;
            points.Add(new Vector2(x, y));
        }
        return points;
    }
    /// <summary>
    /// Multi-octave fractal Perlin noise.
    /// Returns a value roughly in [-1, 1].
    /// </summary>
    private float FractalPerlin(float x, float y, int octaves, float lacunarity, float persistence)
    {
        float total = 0f;
        float amplitude = 1f;
        float frequency = 1f;
        float maxPossible = 0f;
        for (int i = 0; i < octaves; i++)
        {
            float nx = x * frequency;
            float ny = y * frequency;

            float perlin = Mathf.PerlinNoise(nx, ny); // [0,1]
            perlin = perlin * 2f - 1f;                // -> [-1,1]

            total += perlin * amplitude;
            maxPossible += amplitude;

            amplitude *= persistence;
            frequency *= lacunarity;
        }
        if (maxPossible > 0f) total /= maxPossible;
        return total;
    }
    private void BuildMesh(List<Vector2> surfacePoints)
    {
        int n = surfacePoints.Count;
        if (n < 2)
        {
            Debug.LogWarning("Not enough points to build mesh.");
            return;
        }
        float bottomY = m_baseHeight - m_bottomDepth;
        // 2 rows of vertices
        // top (surface), bottom
        var vertices = new Vector3[n * 2];
        var uvs = new Vector2[n * 2];
        for (int i = 0; i < n; i++)
        {
            Vector2 p = surfacePoints[i];

            vertices[i] = new(p.x, p.y, 0f);            // top
            vertices[i + n] = new(p.x, bottomY, 0f);    // bottom

            float t = (float)i / (n - 1);
            uvs[i] = new(t, 1f);
            uvs[i + n] = new(t, 0f);
        }
        // Triangle strip between top and bottom rows
        int quadCount = n - 1;
        int[] triangles = new int[quadCount * 6];
        int ti = 0;
        for (int i = 0; i < quadCount; i++)
        {
            int top0 = i;
            int top1 = i + 1;
            int bot0 = i + n;
            int bot1 = i + 1 + n;

            // Triangle 1
            triangles[ti++] = bot0;
            triangles[ti++] = top0;
            triangles[ti++] = top1;

            // Triangle 2
            triangles[ti++] = bot0;
            triangles[ti++] = top1;
            triangles[ti++] = bot1;
        }
        Mesh mesh = new()
        {
            name = "HillRoadMesh",
            vertices = vertices,
            triangles = triangles,
            uv = uvs
        };
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        m_meshFilter.sharedMesh = mesh;
    }
    private void ApplyCollider(List<Vector2> surfacePoints)
    {
        m_edgeCollider.points = surfacePoints.ToArray();
    }
}