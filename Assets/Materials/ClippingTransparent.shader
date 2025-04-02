Shader "Custom/AdvancedClippingTransparent"
{
    Properties
    {
        _Color ("Base Color", Color) = (1, 1, 1, 1)
        _MainTex ("Main Texture", 2D) = "white" {}
        _Glossiness ("Smoothness", Range(0, 1)) = 0.5
        _Metallic ("Metallic", Range(0, 1)) = 0.0
        _AlphaThreshold ("Alpha Threshold", Range(0, 1)) = 0.5
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        LOD 300

        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        ZTest LEqual
        Cull Back

        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows alpha

        struct Input
        {
            float2 uv_MainTex;
            float3 worldPos;
        };

        sampler2D _MainTex;
        half _Glossiness;
        half _Metallic;
        fixed4 _Color;
        
        // Global parametreler
        float4 _Bound;  // Kesme boyutu
        float4 _ClippingPosition;  // Kesme pozisyonu
        half _AlphaThreshold;  // Alpha eşiği

        void surf(Input IN, inout SurfaceOutputStandard o)
        {
            // Doku rengini al ve materyal rengi ile çarp
            fixed4 texColor = tex2D(_MainTex, IN.uv_MainTex) * _Color;

            // Clipping kontrolü
            float3 adjustedMinBounds = _ClippingPosition.xyz - (_Bound.xyz * 0.5);
            float3 adjustedMaxBounds = _ClippingPosition.xyz + (_Bound.xyz * 0.5);

            if (IN.worldPos.x < adjustedMinBounds.x || IN.worldPos.x > adjustedMaxBounds.x ||
                IN.worldPos.y < adjustedMinBounds.y || IN.worldPos.y > adjustedMaxBounds.y ||
                IN.worldPos.z < adjustedMinBounds.z || IN.worldPos.z > adjustedMaxBounds.z)
            {
                discard;  // Kesim kontrolü
            }

            // Alpha testi
            if (texColor.a < _AlphaThreshold)
            {
                discard;
            }

            // Albedo ve diğer değerleri ayarla
            o.Albedo = texColor.rgb; 
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
            o.Alpha = texColor.a;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
