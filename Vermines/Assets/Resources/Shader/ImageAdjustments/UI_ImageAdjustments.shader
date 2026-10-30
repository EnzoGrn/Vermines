// Photo style adjustments (Lightroom / Photos like) for UI graphics.
// Driven by the ImageAdjustments component, every setting goes from -100 to 100 and 0 leaves the image unchanged.
// Adjustments are done in gamma space so they behave the same in linear and gamma projects.
Shader "Vermines/UI/Image Adjustments"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1, 1, 1, 1)

        _Temperature ("Temperature", Range(-100, 100)) = 0
        _Tint ("Tint (green / magenta)", Range(-100, 100)) = 0
        _Brightness ("Brightness", Range(-100, 100)) = 0
        _Contrast ("Contrast", Range(-100, 100)) = 0
        _Highlights ("Highlights", Range(-100, 100)) = 0
        _Shadows ("Shadows", Range(-100, 100)) = 0
        _Whites ("Whites", Range(-100, 100)) = 0
        _Blacks ("Blacks", Range(-100, 100)) = 0
        _Brilliance ("Brilliance", Range(-100, 100)) = 0
        _Saturation ("Saturation", Range(-100, 100)) = 0

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
            float _Temperature;
            float _Tint;
            float _Brightness;
            float _Contrast;
            float _Highlights;
            float _Shadows;
            float _Whites;
            float _Blacks;
            float _Brilliance;
            float _Saturation;
            int _UIVertexColorAlwaysGammaSpace;

            static const float3 LumaWeights = float3(0.2126, 0.7152, 0.0722);

            // Smooth bump centered on a luminance, used to target a tonal range.
            float ToneWeight(float luma, float center, float width)
            {
                float d = (luma - center) / width;

                return exp(-0.5 * d * d);
            }

            float3 Adjust(float3 color)
            {
                // White balance: temperature moves along blue / orange, tint along green / magenta.
                float temperature = _Temperature * 0.01;
                float tint = _Tint * 0.01;

                color *= float3(1.0 + 0.2 * temperature, 1.0 - 0.2 * tint, 1.0 - 0.2 * temperature);
                color = saturate(color);

                // Brightness bends the midtones and keeps pure black and pure white.
                color = pow(color, exp2(-_Brightness * 0.01));

                // Contrast around middle grey.
                color = saturate((color - 0.5) * (1.0 + _Contrast * 0.01) + 0.5);

                // Tonal ranges, applied on the luminance so hues are kept.
                float luma = dot(color, LumaWeights);
                float delta = 0.0;

                delta += _Blacks * 0.01 * 0.20 * ToneWeight(luma, 0.05, 0.12);
                delta += _Shadows * 0.01 * 0.25 * ToneWeight(luma, 0.28, 0.15);
                delta += _Highlights * 0.01 * 0.25 * ToneWeight(luma, 0.72, 0.15);
                delta += _Whites * 0.01 * 0.20 * ToneWeight(luma, 0.95, 0.12);

                // Brilliance lifts the shadows and pulls the highlights back, revealing details.
                delta += _Brilliance * 0.01 * 0.20 * (ToneWeight(luma, 0.3, 0.2) - ToneWeight(luma, 0.8, 0.15));
                color = saturate(color + delta);

                // Saturation, -100 gives black and white.
                luma = dot(color, LumaWeights);
                color = saturate(lerp(luma.xxx, color, 1.0 + _Saturation * 0.01));

                return color;
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
                half4 color = (tex2D(_MainTex, i.texcoord) + _TextureSampleAdd) * i.color;

                if (IsGammaSpace())
                    color.rgb = Adjust(color.rgb);
                else
                    color.rgb = GammaToLinearSpace(Adjust(LinearToGammaSpace(max(color.rgb, 0.0))));

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
