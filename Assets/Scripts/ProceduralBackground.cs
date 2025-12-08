using UnityEngine;
[ExecuteAlways]
[RequireComponent(typeof(Renderer))]
public class ProceduralBackground : MonoBehaviour
{
    [Tooltip("Animate")]
    [SerializeField] private Vector2 m_minNoiseAnimateSpeed = Vector2.zero;
    [SerializeField] private Vector2 m_maxNoiseAnimateSpeed = Vector2.zero;

    private Vector2 m_noiseOffset = Vector2.zero;

    private Renderer m_renderer;
    private Material m_materialInstance;
    private static readonly int NoiseOffsetID = Shader.PropertyToID("_NoiseOffset");
    private void OnEnable()
    {
        if (m_renderer == null) m_renderer = GetComponent<Renderer>();
        if (m_materialInstance == null)
        {
            m_materialInstance = Application.isPlaying ? m_renderer.material : m_renderer.sharedMaterial;
        }
        ApplyOffset();
    }
    private void Update()
    {
        if (!Application.isPlaying) return;
        m_noiseOffset += new Vector2(Random.Range(m_minNoiseAnimateSpeed.x, m_maxNoiseAnimateSpeed.x),
            Random.Range(m_minNoiseAnimateSpeed.y, m_maxNoiseAnimateSpeed.y)) * Time.deltaTime;
        ApplyOffset();
    }
    private void ApplyOffset()
    {
        if (m_materialInstance == null) return;
        Vector4 current = m_materialInstance.GetVector(NoiseOffsetID);
        current.x = m_noiseOffset.x;
        current.y = m_noiseOffset.y;
        m_materialInstance.SetVector(NoiseOffsetID, current);
    }
}