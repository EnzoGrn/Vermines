Shader "Custom/BurnDissolve"
{
    Properties
    {
        _MainTex      ("Sprite Texture",   2D)      = "white" {}
        _NoiseTex     ("Noise Texture",    2D)      = "white" {}
        _BurnAmount   ("Burn Amount",      Range(0,1)) = 0
        _EdgeWidth    ("Edge Width",       Range(0,0.2)) = 0.04
        _BurnColor1   ("Burn Color Inner", Color) = (1, 0.3, 0, 1)
        _BurnColor2   ("Burn Color Outer", Color) = (1, 0.85, 0, 1)
        _EmissiveStr  ("Emissive Strength",Float) = 2.5
    }

    SubShader
    {
        Tags
        {
            "Queue"             = "Transparent"
            "RenderType"        = "Transparent"
            "IgnoreProjector"   = "True"
            "PreviewType"       = "Plane"
            "CanUseSpriteAtlas" = "True"
        }

        Cull Off
        Lighting Off
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex   vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            sampler2D _NoiseTex;
            float4    _MainTex_ST;
            float     _BurnAmount;
            float     _EdgeWidth;
            fixed4    _BurnColor1;
            fixed4    _BurnColor2;
            float     _EmissiveStr;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv     : TEXCOORD0;
                fixed4 color  : COLOR;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv  : TEXCOORD0;
                fixed4 col : COLOR;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv  = TRANSFORM_TEX(v.uv, _MainTex);
                o.col = v.color;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 sprite = tex2D(_MainTex, i.uv) * i.col;

                // Transparent pixels du sprite : on s'arrête là
                clip(sprite.a - 0.01);

                // Lecture du bruit
                float noise = tex2D(_NoiseTex, i.uv).r;

                // Seuil de dissolution
                float threshold = _BurnAmount;

                // Le pixel est totalement dissous
                clip(noise - threshold);

                // Calcul de la zone de bord (0 = juste au-dessus du seuil, 1 = loin)
                float edgeFactor = saturate((noise - threshold) / _EdgeWidth);

                // Couleur de brûlure : dégradé intérieur/extérieur du bord
                fixed4 burnCol = lerp(_BurnColor1, _BurnColor2, edgeFactor);
                burnCol.rgb   *= _EmissiveStr;

                // Mélange bord vs sprite
                fixed4 finalCol = lerp(burnCol, sprite, edgeFactor);

                // Alpha : bord opaque, fondu vers le sprite
                finalCol.a = sprite.a * lerp(1.0, sprite.a, edgeFactor);

                return finalCol;
            }
            ENDCG
        }
    }

    Fallback "Sprites/Default"
}