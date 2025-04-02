Shader "Custom/SingleBoundClippingOpaque"
{
    Properties
    {
        _Color ("Base Color", Color) = (1, 1, 1, 1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Glossiness ("Smoothness", Range(0, 1)) = 0.5
        _Metallic ("Metallic", Range(0, 1)) = 0.0
    }

    SubShader
    {
        Tags { "Queue"="Geometry" "RenderType"="Opaque" }
        LOD 200

        ZWrite On
        ZTest LEqual
        Cull Back

        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows

        struct Input
        {
            float2 uv_MainTex;
            float3 worldPos;
        };

        sampler2D _MainTex;
        half _Glossiness;
        half _Metallic;
        fixed4 _Color;
        
        // Global parametreleri kullanıyoruz
        float4 _Bound;
        float4 _ClippingPosition;

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            // Clipping işlemi
            float3 worldPos = IN.worldPos;
            float3 adjustedMinBounds = _ClippingPosition.xyz - (_Bound.xyz * 0.5);
            float3 adjustedMaxBounds = _ClippingPosition.xyz + (_Bound.xyz * 0.5);

            if (worldPos.x < adjustedMinBounds.x || worldPos.x > adjustedMaxBounds.x ||
                worldPos.y < adjustedMinBounds.y || worldPos.y > adjustedMaxBounds.y ||
                worldPos.z < adjustedMinBounds.z || worldPos.z > adjustedMaxBounds.z)
            {
                discard; // Clipping işlemi
            }

            // Renk ve doku işlemleri
            fixed4 texColor = tex2D(_MainTex, IN.uv_MainTex);
            o.Albedo = _Color.rgb;
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
            o.Alpha = texColor.a;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
