Shader "Custom/LoadingBarShader"
{
    Properties
    {
        _Color ("Main Color", Color) = (.5,.5,.5,1)
        _MainTex ("Base (RGB)", 2D) = "white" { }
        _Progress ("Progress", Range(0, 1)) = 0.0
    }
    SubShader
    {
        Tags { "Queue"="Overlay" }
        Pass {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex : POSITION;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float4 pos : POSITION;
                float2 uv : TEXCOORD0;
            };

            float _Progress;

            v2f vert(appdata_t v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.texcoord;
                return o;
            }

            half4 frag(v2f i) : SV_Target
            {
                float2 uv = i.uv;
                // Left to right fill progress
                if (uv.x > _Progress) {
                    discard;
                }
                return half4(1, 0, 0, 1);  // Kýrmýzý renk
            }
            ENDCG
        }
    }
    Fallback "Diffuse"
}
