// Photoshop "Outer Glow" (Softer technique) for TextMeshPro UGUI texts.
// Drawn by the hidden TMPOuterGlowGraphic created by the TMPOuterGlow component, behind the text.
// _MainTex holds the blurred text coverage, every Photoshop setting is applied here.
Shader "Vermines/UI/TMP Outer Glow"
{
    Properties
    {
        [PerRendererData] _MainTex ("Blurred Text Mask", 2D) = "black" {}
        _Color ("Tint", Color) = (1, 1, 1, 1)

        [NoScaleOffset] _GlowGradient ("Gradient (right = against the text)", 2D) = "white" {}
        [NoScaleOffset] _GlowContour ("Contour", 2D) = "white" {}
        _GlowOpacity ("Opacity", Range(0, 1)) = 0.64
        _GlowNoise ("Noise", Range(0, 1)) = 0
        _GlowSpread ("Spread", Range(0, 1)) = 0
        _GlowRange ("Range", Range(0.01, 1)) = 0.61
        _GlowJitter ("Jitter", Range(0, 1)) = 0.51
        _GlowIntensity ("Intensity", Range(0, 8)) = 2
        _GradientSmoothness ("Gradient Smoothness", Range(0, 1)) = 1

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
            #pragma target 2.0

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
            sampler2D _GlowGradient;
            sampler2D _GlowContour;
            fixed4 _Color;
            fixed4 _TextureSampleAdd;
            float4 _ClipRect;
            float _GlowOpacity;
            float _GlowNoise;
            float _GlowSpread;
            float _GlowRange;
            float _GlowJitter;
            float _GlowIntensity;
            float _GradientSmoothness;
            int _UIVertexColorAlwaysGammaSpace;

            // Stable per pixel random value in [0, 1).
            float Hash(float2 p)
            {
                p = frac(p * float2(123.34, 456.21));
                p += dot(p, p + 45.32);

                return frac(p.x * p.y);
            }

            v2f Vert(appdata_t v)
            {
                v2f o;

                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                o.worldPosition = v.vertex;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.texcoord = v.texcoord;
                if (_UIVertexColorAlwaysGammaSpace && !IsGammaSpace())
                    v.color.rgb = UIGammaToLinear(v.color.rgb);
                o.color = v.color * _Color;
                return o;
            }

            fixed4 Frag(v2f i) : SV_Target
            {
                float mask = tex2D(_MainTex, i.texcoord).r;

                // Spread chokes the matte, Range keeps only the part of the falloff fed to the contour.
                float glow = saturate(mask * _GlowIntensity / max(1.0 - _GlowSpread, 0.001));
                glow = saturate(glow / _GlowRange);

                float contour = tex2D(_GlowContour, float2(glow, 0.5)).r;

                // As in Photoshop, the right stop of the gradient is against the text and the left stop is the outer edge.
                // Jitter is not applied per pixel: Photoshop shows no visible grain with a two stop gradient.
                float position = contour;
                position = lerp(position, position * position * (3.0 - 2.0 * position), _GradientSmoothness);

                float4 ramp = tex2D(_GlowGradient, float2(position, 0.5));
                float noise = Hash(i.vertex.yx + 31.7);

                fixed4 color;
                color.rgb = ramp.rgb * i.color.rgb;
                color.a = ramp.a * contour * _GlowOpacity * (1.0 - _GlowNoise * noise) * i.color.a;

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
