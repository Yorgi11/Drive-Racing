using UnityEngine;
public class LevelGenerator : MonoBehaviour
{
    [SerializeField] private bool m_generateButton;
    [SerializeField] private ProceduralLandscape m_pl;
    [SerializeField] private LandscapeTextureGenerator m_ltg;
    private void OnValidate()
    {
        if (m_generateButton)
        {
            m_pl.Init();
            m_pl.Generate();
            m_ltg.Init();
            m_ltg.GenerateTexture();
        }
    }
}