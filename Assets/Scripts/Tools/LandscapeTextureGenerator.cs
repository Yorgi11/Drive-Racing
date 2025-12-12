using UnityEngine;
[ExecuteAlways]
[RequireComponent(typeof(MeshRenderer))]
public class LandscapeTextureGenerator : MonoBehaviour
{
    [Header("Layer Profile")]
    [SerializeField] private LandscapeLayerProfile m_profile;

    [Header("Texture Output")]
    [SerializeField] private int m_width = 512;
    [SerializeField] private int m_height = 512;
    [Tooltip("Name used for the generated texture asset (optional if you want to export).")]
    [SerializeField] private string m_textureName = "LandscapeLayeredTex";

    private MeshRenderer m_renderer;
    private void Awake()
    {
        m_renderer = GetComponent<MeshRenderer>();
    }
    private void Start()
    {
        GenerateTexture();
    }
    [ContextMenu("Generate Layered Texture")]
    public void GenerateTexture()
    {
        if (m_profile == null || m_profile.layers == null || m_profile.layers.Count == 0)
        {
            Debug.LogWarning("LandscapeTextureGenerator: No layer profile assigned or empty.");
            return;
        }
        if (m_renderer == null) m_renderer = GetComponent<MeshRenderer>();
        float totalThickness = 0f;
        foreach (var layer in m_profile.layers)
        {
            if (layer.thickness > 0f) totalThickness += layer.thickness;
        }
        if (totalThickness <= 0f)
        {
            Debug.LogWarning("LandscapeTextureGenerator: Total thickness <= 0.");
            return;
        }
        int layerCount = m_profile.layers.Count;
        float[] layerStart = new float[layerCount];
        float[] layerEnd = new float[layerCount];

        float accum = 0f;
        for (int i = 0; i < layerCount; i++)
        {
            var l = m_profile.layers[i];
            float normalizedThickness = (l.thickness <= 0f) ? 0f : l.thickness / totalThickness;
            layerStart[i] = accum;
            layerEnd[i] = accum + normalizedThickness;
            accum += normalizedThickness;
        }
        Texture2D tex = new(m_width, m_height, TextureFormat.RGBA32, false)
        {
            wrapMode = TextureWrapMode.Repeat,
            filterMode = FilterMode.Bilinear
        };
        for (int y = 0; y < m_height; y++)
        {
            float depth01 = (float)y / (m_height - 1);

            for (int x = 0; x < m_width; x++)
            {
                Color finalColor = Color.magenta; // debug color if layer not found
                bool foundLayer = false;

                for (int i = 0; i < layerCount; i++)
                {
                    float s = layerStart[i];
                    float e = layerEnd[i];
                    if (depth01 < s || depth01 > e) continue;

                    var layer = m_profile.layers[i];
                    Color baseColor = layer.color;
                    if (layer.texture != null)
                    {
                        float u = ((float)x / m_width) * layer.textureTilingX;
                        float vLocal = Mathf.InverseLerp(s, e, depth01); // 0 at top of this layer, 1 at bottom
                        float v = vLocal * layer.textureTilingY;
                        Color texCol = layer.texture.GetPixelBilinear(u, v);
                        baseColor = Color.Lerp(baseColor, texCol, layer.textureBlend);
                    }
                    float layerHeight = e - s;
                    float localT = layerHeight > 0f ? (depth01 - s) / layerHeight : 0f;

                    Color prevColor = baseColor;
                    if (i > 0) prevColor = m_profile.layers[i - 1].color;

                    Color nextColor = baseColor;
                    if (i < layerCount - 1) nextColor = m_profile.layers[i + 1].color;
                    Color c = baseColor;

                    if (layer.blendIntoPrevious > 0f && localT <= layer.blendIntoPrevious)
                    {
                        float tBlend = Mathf.InverseLerp(0f, layer.blendIntoPrevious, localT);
                        c = Color.Lerp(prevColor, baseColor, tBlend);
                    }
                    else if (layer.blendIntoNext > 0f && localT >= 1f - layer.blendIntoNext)
                    {
                        float tBlend = Mathf.InverseLerp(1f - layer.blendIntoNext, 1f, localT);
                        c = Color.Lerp(baseColor, nextColor, tBlend);
                    }
                    finalColor = c;
                    foundLayer = true;
                    break;
                }
                if (!foundLayer)
                {
                    var lastLayer = m_profile.layers[layerCount - 1];
                    finalColor = lastLayer.color;
                }
                tex.SetPixel(x, m_height - 1 - y, finalColor);
            }
        }
        tex.Apply();
        if (m_renderer != null && m_renderer.sharedMaterial != null) m_renderer.sharedMaterial.mainTexture = tex;

#if UNITY_EDITOR
        /*
        string path = "Assets/" + m_textureName + ".png";
        var bytes = tex.EncodeToPNG();
        System.IO.File.WriteAllBytes(path, bytes);
        UnityEditor.AssetDatabase.Refresh();
        Debug.Log("Saved layered texture to " + path);
        */
#endif
        Debug.Log("LandscapeTextureGenerator: Texture generated and applied.");
    }
}