Shader "Custom/ClippingOpaque"
{
    Properties
    {
        _Color ("Base Color", Color) = (1, 1, 1, 1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Glossiness ("Smoothness", Range(0, 1)) = 0.5
        _Metallic ("Metallic", Range(0, 1)) = 0.0
        _UseOverrideColor ("Use Override Color", Float) = 0
        _OverrideColor ("Override Color", Color) = (1,1,1,1)
        
        // Yeni özellikler
        [Toggle]_UseSelectionColor ("Use Selection Color", Float) = 0
        _SelectionColor ("Selection Color", Color) = (1,0,0,1)
        _SelectionIntensity ("Selection Intensity", Range(0, 1)) = 0.5
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
        #pragma multi_compile_instancing
        #pragma shader_feature _USESELECTIONCOLOR_ON

        struct Input
        {
            float2 uv_MainTex;
            float3 worldPos;
        };

        sampler2D _MainTex;
        half _Glossiness;
        half _Metallic;
        fixed4 _Color;
        
        // Color override properties
        UNITY_INSTANCING_BUFFER_START(Props)
            UNITY_DEFINE_INSTANCED_PROP(float, _UseOverrideColor)
            UNITY_DEFINE_INSTANCED_PROP(fixed4, _OverrideColor)
        UNITY_INSTANCING_BUFFER_END(Props)
        
        // Global clipping parameters
        float4 _Bound;
        float4 _ClippingPosition;
        
        // Selection properties
        float _UseSelectionColor;
        fixed4 _SelectionColor;
        float _SelectionIntensity;

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
                discard;
            }

            // Get instance data
            float useOverride = UNITY_ACCESS_INSTANCED_PROP(Props, _UseOverrideColor);
            fixed4 overrideColor = UNITY_ACCESS_INSTANCED_PROP(Props, _OverrideColor);
            
            // Renk ve doku işlemleri
            fixed4 texColor = tex2D(_MainTex, IN.uv_MainTex);
            fixed3 baseColor = useOverride > 0.5 ? overrideColor.rgb : _Color.rgb * texColor.rgb;
            
            // Seçim rengi uygulama
            #ifdef _USESELECTIONCOLOR_ON
                if (_UseSelectionColor > 0.5)
                {
                    baseColor = lerp(baseColor, _SelectionColor.rgb, _SelectionIntensity);
                }
            #endif
            
            o.Albedo = baseColor;
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
            o.Alpha = texColor.a;
        }
        ENDCG
    }
    FallBack "Diffuse"
}