// Voile du tutoriel : couleur sombre partout, sauf dans 1 à 4 rectangles arrondis
// (ou cercles) aux bords adoucis. Les positions viennent de TutorialSpotlightOverlay,
// exprimées dans l'espace local de son RectTransform et transmises par les UV.
Shader "Vermines/Tutorial/Spotlight"
{
    Properties
    {
        _Color ("Couleur du voile", Color) = (0.02, 0.01, 0.03, 1)
        _Dim ("Opacité", Range(0, 1)) = 0.78
        _Softness ("Adoucissement des bords", Float) = 10

        // Propriétés standard UI (masques, stencil)
        [HideInInspector] _StencilComp ("Stencil Comparison", Float) = 8
        [HideInInspector] _Stencil ("Stencil ID", Float) = 0
        [HideInInspector] _StencilOp ("Stencil Operation", Float) = 0
        [HideInInspector] _StencilWriteMask ("Stencil Write Mask", Float) = 255
        [HideInInspector] _StencilReadMask ("Stencil Read Mask", Float) = 255
        [HideInInspector] _ColorMask ("Color Mask", Float) = 15
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
            "IgnoreProjector" = "True"
            "RenderType" = "Transparent"
            "PreviewType" = "Plane"
            "CanUseSpriteAtlas" = "False"
        }

        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask [_ColorMask]

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0
            #include "UnityCG.cginc"

            #define MAX_HOLES 4

            struct appdata
            {
                float4 vertex : POSITION;
                float4 color : COLOR;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                fixed4 color : COLOR;
                float2 localPos : TEXCOORD0;
            };

            fixed4 _Color;
            float _Dim;
            float _Softness;
            float4 _Holes[MAX_HOLES];     // xy = centre, zw = demi-taille
            float4 _HoleRadii[MAX_HOLES]; // x = rayon des coins
            int _HoleCount;

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.color = v.color;
                o.localPos = v.uv;
                return o;
            }

            // Distance signée à un rectangle arrondi (négative à l'intérieur).
            float sdRoundedBox(float2 p, float2 halfSize, float r)
            {
                float2 q = abs(p) - halfSize + r;
                return length(max(q, 0.0)) + min(max(q.x, q.y), 0.0) - r;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float d = 1e6;
                for (int k = 0; k < MAX_HOLES; k++)
                {
                    if (k < _HoleCount)
                    {
                        float4 h = _Holes[k];
                        d = min(d, sdRoundedBox(i.localPos - h.xy, h.zw, _HoleRadii[k].x));
                    }
                }
                // 0 dans le trou, 1 au-delà de la zone d'adoucissement.
                float mask = smoothstep(-_Softness * 0.5, _Softness * 0.5, d);
                fixed4 c = _Color * i.color;
                c.a *= _Dim * mask;
                return c;
            }
            ENDCG
        }
    }
}
