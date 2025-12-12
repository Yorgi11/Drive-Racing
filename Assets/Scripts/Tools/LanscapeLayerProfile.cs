using System;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "LandscapeLayerProfile", menuName = "Landscape/Layer Profile")]
public class LandscapeLayerProfile : ScriptableObject
{
    [System.Serializable]
    public class Layer
    {
        public Color color = Color.white;
        public Texture2D texture;
        public float thickness = 1f;

        public float textureTilingX = 1f;
        public float textureTilingY = 1f;
        [Range(0f, 1f)]
        public float textureBlend = 1f;

        [Header("Color Blending")]
        [Tooltip("0 = hard cut at top of this layer; 1 = entire top portion blends from previous color into this layer.")]
        [Range(0f, 1f)]
        public float blendIntoPrevious = 0.1f;

        [Tooltip("0 = hard cut at bottom of this layer; 1 = entire bottom portion blends from this color into next color.")]
        [Range(0f, 1f)]
        public float blendIntoNext = 0.1f;
    }
    public List<Layer> layers = new List<Layer>();
}