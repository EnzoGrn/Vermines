// Bakes the coverage of a TextMeshPro mesh into a render texture then blurs it (separable gaussian).
// Used by the TMPOuterGlow component, never assigned by hand.
Shader "Hidden/Vermines/TMP Outer Glow Bake"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }

    CGINCLUDE
    #include "UnityCG.cginc"

    #define MAX_BLUR_RADIUS 48

    sampler2D _MainTex;
    float4 _MainTex_TexelSize;
    float4 _BakeRect;
    float _BlurSigma;

    float4 Blur(float2 uv, float2 direction)
    {
        float sigma = max(_BlurSigma, 0.01);
        int radius = min((int)ceil(sigma * 3.0), MAX_BLUR_RADIUS);
        float2 offset = direction * _MainTex_TexelSize.xy;
        float sum = 0.0;
        float total = 0.0;

        [loop]
        for (int i = -radius; i <= radius; i++) {
            float weight = exp(-(i * i) / (2.0 * sigma * sigma));

            sum += tex2Dlod(_MainTex, float4(uv + offset * i, 0.0, 0.0)).r * weight;
            total += weight;
        }
        return (sum / total).xxxx;
    }
    ENDCG

    SubShader
    {
        Cull Off
        ZWrite Off
        ZTest Always

        // Pass 0: coverage of the text glyphs (union of every glyph quad).
        Pass
        {
            Name "Mask"
            Blend One One
            BlendOp Max

            CGPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma target 3.0

            struct appdata_t
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float4 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex   : SV_POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            v2f Vert(appdata_t v)
            {
                v2f o;

                // Map the baked area (text local space) straight to clip space.
                float2 position = (v.vertex.xy - _BakeRect.xy) / _BakeRect.zw;

                o.vertex = float4(position * 2.0 - 1.0, 0.5, 1.0);
                #if UNITY_UV_STARTS_AT_TOP
                o.vertex.y = -o.vertex.y;
                #endif
                o.color = v.color;
                o.texcoord = v.texcoord.xy;
                return o;
            }

            float4 Frag(v2f i) : SV_Target
            {
                float sdf = tex2D(_MainTex, i.texcoord).a;
                float width = max(fwidth(sdf), 0.0001);
                float coverage = saturate((sdf - 0.5) / width + 0.5) * i.color.a;

                return coverage.xxxx;
            }
            ENDCG
        }

        // Pass 1: horizontal blur.
        Pass
        {
            Name "BlurHorizontal"

            CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment Frag
            #pragma target 3.0

            float4 Frag(v2f_img i) : SV_Target
            {
                return Blur(i.uv, float2(1.0, 0.0));
            }
            ENDCG
        }

        // Pass 2: vertical blur.
        Pass
        {
            Name "BlurVertical"

            CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment Frag
            #pragma target 3.0

            float4 Frag(v2f_img i) : SV_Target
            {
                return Blur(i.uv, float2(0.0, 1.0));
            }
            ENDCG
        }
    }
}
