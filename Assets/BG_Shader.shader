Shader "Unlit/BG_Shader"
{
    Properties
    {
        _PrimaryColor   ("Primary Color",   Color) = (0, 1, 1, 1)
        _SecondaryColor ("Secondary Color", Color) = (0, 0, 1, 1)
        _TertiaryColor  ("Tertiary Color",  Color) = (1, 0, 1, 1)

        _NoiseScale     ("Noise Scale",     Float) = 1.0
        _Frequency      ("Frequency",       Float) = 1.0

        _SecondOctaveStrength ("2nd Octave Strength", Range(0,1)) = 0.5
        _SecondOctaveFreq     ("2nd Octave Frequency", Float) = 2.0

        _NoiseOffset    ("Noise Offset",    Vector) = (0, 0, 0, 0)
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline" = "UniversalRenderPipeline"
            "RenderType"     = "Opaque"
            "Queue"          = "Background"
        }

        Pass
        {
            Name "Unlit"
            // ?? CHANGE THIS:
            // Tags { "LightMode" = "UniversalForward" }
            // ? TO THIS:
            Tags { "LightMode" = "SRPDefaultUnlit" }

            Cull Off
            ZWrite Off
            ZTest Always
            Blend One Zero

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv         : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv          : TEXCOORD0;
            };

            float4 _PrimaryColor;
            float4 _SecondaryColor;
            float4 _TertiaryColor;

            float  _NoiseScale;
            float  _Frequency;

            float  _SecondOctaveStrength;
            float  _SecondOctaveFreq;

            float4 _NoiseOffset;

            float2 hash2(float2 p)
            {
                p = float2(
                    dot(p, float2(127.1, 311.7)),
                    dot(p, float2(269.5, 183.3))
                );
                return frac(sin(p) * 43758.5453);
            }

            float noise2D(float2 p)
            {
                float2 i = floor(p);
                float2 f = frac(p);

                float2 u = f * f * (3.0 - 2.0 * f);

                float2 a = hash2(i + float2(0.0, 0.0));
                float2 b = hash2(i + float2(1.0, 0.0));
                float2 c = hash2(i + float2(0.0, 1.0));
                float2 d = hash2(i + float2(1.0, 1.0));

                float v1 = lerp(a.x, b.x, u.x);
                float v2 = lerp(c.x, d.x, u.x);
                float v  = lerp(v1, v2, u.y);

                return v; // [0,1]
            }

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = IN.uv;
                return OUT;
            }

            float4 frag(Varyings IN) : SV_Target
            {
                float2 uv = IN.uv;

                float2 basePos = uv * _NoiseScale * _Frequency + _NoiseOffset.xy;

                float n1 = noise2D(basePos);
                float n2 = noise2D(basePos * float2(1.37, 1.91));
                float n3 = noise2D(basePos * float2(2.11, 2.53));

                if (_SecondOctaveStrength > 0.0)
                {
                    float2 basePos2 = basePos * _SecondOctaveFreq;

                    float o1 = noise2D(basePos2);
                    float o2 = noise2D(basePos2 * float2(1.37, 1.91));
                    float o3 = noise2D(basePos2 * float2(2.11, 2.53));

                    n1 = lerp(n1, o1, _SecondOctaveStrength);
                    n2 = lerp(n2, o2, _SecondOctaveStrength);
                    n3 = lerp(n3, o3, _SecondOctaveStrength);
                }

                float w1 = n1;
                float w2 = n2;
                float w3 = n3;

                float sum = w1 + w2 + w3 + 0.0001;
                w1 /= sum;
                w2 /= sum;
                w3 /= sum;

                float4 col =
                    _PrimaryColor   * w1 +
                    _SecondaryColor * w2 +
                    _TertiaryColor  * w3;

                col.a = 1.0;
                return col;
            }

            ENDHLSL
        }
    }
    FallBack Off
}