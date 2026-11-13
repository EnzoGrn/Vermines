// Ink blot reveal for UI graphics: the graphic appears as if ink dropped on the parchment and soaked outward.
// Driven by the InkReveal component through _Progress (0 = nothing, 1 = fully drawn).
// The blot front is a distance field from _Origin, made irregular by fractal noise; just behind the front the ink
// is darker and a bit more opaque ("wet"), and that wet tint dries out at the end of the reveal.
// Once drawn, the ink keeps a living body: uneven density, paper grain and darker pools along the drawing's edges.
// Noise is computed in the shader, so no texture is needed. Expects a non-atlased sprite (UVs 0..1).
Shader "Vermines/UI/Ink Reveal"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1, 1, 1, 1)

        _Progress ("Progress", Range(0, 1)) = 1
        _Origin ("Origin (UV)", Vector) = (0.5, 0.5, 0, 0)
        _Softness ("Edge Softness", Range(0.005, 0.3)) = 0.06
        _NoiseScale ("Noise Scale", Float) = 5
        _NoiseStrength ("Noise Strength", Range(0, 0.6)) = 0.3
        _WetColor ("Wet Ink Color", Color) = (0.12, 0.08, 0.06, 0.85)
        _WetWidth ("Wet Edge Width", Range(0, 0.4)) = 0.14
        _Warp ("Front Warp", Range(0, 0.6)) = 0.25
        _Seed ("Seed", Float) = 0

        _InkColor ("Ink Color", Color) = (0.16, 0.11, 0.08, 0.9)
        _Density ("Density Variation", Range(0, 1)) = 0.5
        _DensityScale ("Density Scale", Float) = 3
        _Grain ("Grain", Range(0, 1)) = 0.35
        _GrainScale ("Grain Scale", Float) = 120
        _Pooling ("Edge Pooling", Range(0, 1)) = 0.6
        _PoolRadius ("Pooling Radius (UV)", Range(0.002, 0.08)) = 0.02

        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255

        _ColorMask ("Color Mask", Float) = 15

        [Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip ("Use Alpha Clip", Float) = 0
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
            "IgnoreProjector" = "True"
            "RenderType" = "Transparent"
            "PreviewType" = "Plane"
            "CanUseSpriteAtlas" = "True"
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
            Name "Default"

            CGPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma target 3.0

            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

            #pragma multi_compile_local _ UNITY_UI_CLIP_RECT
            #pragma multi_compile_local _ UNITY_UI_ALPHACLIP

            struct appdata_t
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 vertex        : SV_POSITION;
                fixed4 color         : COLOR;
                float2 texcoord      : TEXCOORD0;
                float4 worldPosition : TEXCOORD1;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            fixed4 _Color;
            fixed4 _TextureSampleAdd;
            float4 _ClipRect;
            float _Progress;
            float4 _Origin;
            float _Softness;
            float _NoiseScale;
            float _NoiseStrength;
            fixed4 _WetColor;
            float _WetWidth;
            float _Warp;
            float _Seed;
            fixed4 _InkColor;
            float _Density;
            float _DensityScale;
            float _Grain;
            float _GrainScale;
            float _Pooling;
            float _PoolRadius;
            int _UIVertexColorAlwaysGammaSpace;

            float Hash(float2 p)
            {
                return frac(sin(dot(p, float2(127.1, 311.7))) * 43758.5453);
            }

            float ValueNoise(float2 p)
            {
                float2 i = floor(p);
                float2 f = frac(p);
                float2 u = f * f * (3.0 - 2.0 * f);

                return lerp(lerp(Hash(i), Hash(i + float2(1, 0)), u.x),
                            lerp(Hash(i + float2(0, 1)), Hash(i + float2(1, 1)), u.x), u.y);
            }

            // 4 octaves: large lobes for the blot shape, small ones for the fibres of the paper.
            float Fbm(float2 p)
            {
                float value = 0.0;
                float amplitude = 0.5;

                for (int k = 0; k < 4; k++)
                {
                    value += amplitude * ValueNoise(p);
                    p *= 2.03;
                    amplitude *= 0.5;
                }
                return value;
            }

            v2f Vert(appdata_t v)
            {
                v2f o;

                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                o.worldPosition = v.vertex;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
                if (_UIVertexColorAlwaysGammaSpace && !IsGammaSpace())
                    v.color.rgb = UIGammaToLinear(v.color.rgb);
                o.color = v.color * _Color;
                return o;
            }

            fixed4 Frag(v2f i) : SV_Target
            {
                float2 uv = i.texcoord;
                half4 color = (tex2D(_MainTex, uv) + _TextureSampleAdd) * i.color;

                // --- Ink body: stays after the reveal, so the ink never looks flat ---

                // Large soft blotches where the ink is heavier or thinner.
                float blotch = Fbm(uv * _DensityScale + _Seed * 1.7 + 5.3);
                // Fine granulation of the paper drinking the ink.
                float grain = ValueNoise(uv * _GrainScale + _Seed * 3.1);
                // Ink gathers on the inner side of the drawing's edges: compare the alpha with its neighbourhood.
                float around = (tex2D(_MainTex, uv + float2(_PoolRadius, 0)).a + tex2D(_MainTex, uv - float2(_PoolRadius, 0)).a
                              + tex2D(_MainTex, uv + float2(0, _PoolRadius)).a + tex2D(_MainTex, uv - float2(0, _PoolRadius)).a) * 0.25;
                float pool = saturate((tex2D(_MainTex, uv).a - around) * 4.0);

                float dark = saturate((blotch - 0.5) * 2.0 * _Density + (grain - 0.5) * _Grain + pool * _Pooling);

                color.rgb = lerp(color.rgb, _InkColor.rgb, dark * _InkColor.a);
                color.a *= saturate(1.0 - (0.5 - blotch) * _Density * 0.8 - (1.0 - grain) * _Grain * 0.3 + pool * _Pooling * 0.3);

                // --- Blot front ---

                // Domain warp bends the front into lobes and fingers instead of a circle.
                float2 warp = float2(Fbm(uv * _NoiseScale * 0.5 + _Seed + 13.1), Fbm(uv * _NoiseScale * 0.5 + _Seed + 47.7)) - 0.5;

                // Distance from the drop point, about 0 at the origin and 1 at the far corner.
                float dist = length(uv + warp * _Warp - _Origin.xy) * 1.4142;
                float noise = Fbm(uv * _NoiseScale + _Seed);
                float field = dist + (noise - 0.5) * 2.0 * _NoiseStrength;

                // The front travels across the whole field range, so 0 hides everything and 1 shows everything.
                float front = lerp(-_NoiseStrength - _Softness, 1.0 + _NoiseStrength + _Softness, _Progress);
                float ink = 1.0 - smoothstep(front - _Softness, front, field);

                // Wet band just behind the front, drying out over the last part of the reveal.
                float wet = smoothstep(front - _Softness - _WetWidth, front - _Softness, field) * ink;
                wet *= 1.0 - smoothstep(0.8, 1.0, _Progress);

                color.rgb = lerp(color.rgb, _WetColor.rgb, wet * _WetColor.a);
                color.a = saturate(color.a * (1.0 + wet * 0.5)) * ink;

                #ifdef UNITY_UI_CLIP_RECT
                color.a *= UnityGet2DClipping(i.worldPosition.xy, _ClipRect);
                #endif

                #ifdef UNITY_UI_ALPHACLIP
                clip(color.a - 0.001);
                #endif

                return color;
            }
            ENDCG
        }
    }
}
