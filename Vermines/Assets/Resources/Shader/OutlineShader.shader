Shader "Custom/OutlineTransparent"
{
    Properties
    {
        _OutlineColor ("Outline Color", Color) = (0,1,0,1)
        _OutlineWidth ("Outline Width", Range(0.001, 0.1)) = 0.01
    }
    SubShader
    {
        // Force rendering after the main object to ensure the outline is visible
        Tags { "Queue"="Overlay" "RenderType"="Transparent" }

        Pass
        {
            Name "Outline"
            Cull Front          // Hide front faces so the outline appears around the object
            ZWrite Off          // Disable depth writing to prevent occlusion issues
            ZTest LEqual        // Keep depth testing active
            Blend SrcAlpha OneMinusSrcAlpha // Enable transparency blending

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
            };

            float _OutlineWidth;
            float4 _OutlineColor;

            v2f vert (appdata_t v)
            {
                v2f o;
                float3 norm = normalize(v.normal);
                v.vertex.xyz += norm * _OutlineWidth;
                o.pos = UnityObjectToClipPos(v.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                return _OutlineColor; // Use the alpha of the outline color
            }
            ENDCG
        }
    }
}
