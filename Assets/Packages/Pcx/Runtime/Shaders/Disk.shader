Shader "Point Cloud/Disk"
{
    Properties
    {
        _Tint("Tint", Color) = (0.5, 0.5, 0.5, 1)
        _PointSize("Point Size", Float) = 0.05
        [Toggle] _Distance("Apply Distance", Float) = 1
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        Pass
        {
            CGPROGRAM

            #pragma vertex Vertex
            #pragma fragment Fragment
            #pragma multi_compile_fog
            #pragma multi_compile _ UNITY_COLORSPACE_GAMMA
            #pragma multi_compile _ _DISTANCE_ON
            #pragma multi_compile _ _COMPUTE_BUFFER

            #include "UnityCG.cginc"
            #include "Common.cginc"

            struct Attributes
            {
                float4 position : POSITION;
                half3 color : COLOR;
            };

            struct Varyings
            {
                float4 position : SV_Position;
                half3 color : COLOR;
                half psize : PSIZE;
                float3 worldPos : TEXCOORD1;
                UNITY_FOG_COORDS(0)
            };

            half4 _Tint;
            float4x4 _Transform;
            half _PointSize;
            
            // Global bound ve clipping position
            float4 _Bound;
            float4 _ClippingPosition;

        #if _COMPUTE_BUFFER
            StructuredBuffer<float4> _PointBuffer;
        #endif

        #if _COMPUTE_BUFFER
            Varyings Vertex(uint vid : SV_VertexID)
        #else
            Varyings Vertex(Attributes input)
        #endif
            {
            #if _COMPUTE_BUFFER
                float4 pt = _PointBuffer[vid];
                float4 pos = mul(_Transform, float4(pt.xyz, 1));
                half3 col = PcxDecodeColor(asuint(pt.w));
            #else
                float4 pos = input.position;
                half3 col = input.color;
            #endif

            #ifdef UNITY_COLORSPACE_GAMMA
                col *= _Tint.rgb * 2;
            #else
                col *= LinearToGammaSpace(_Tint.rgb) * 2;
                col = GammaToLinearSpace(col);
            #endif

                Varyings o;
                o.position = UnityObjectToClipPos(pos);
                o.color = col;
                o.worldPos = mul(unity_ObjectToWorld, pos).xyz;
            #ifdef _DISTANCE_ON
                o.psize = _PointSize / o.position.w * _ScreenParams.y;
            #else
                o.psize = _PointSize;
            #endif
                UNITY_TRANSFER_FOG(o, o.position);
                return o;
            }

            half4 Fragment(Varyings input) : SV_Target
            {
                // Clipping kontrolü global değişkenlerle yapılır
                float3 adjustedMinBounds = _ClippingPosition.xyz - (_Bound.xyz * 0.5);
                float3 adjustedMaxBounds = _ClippingPosition.xyz + (_Bound.xyz * 0.5);

                if (input.worldPos.x < adjustedMinBounds.x || input.worldPos.x > adjustedMaxBounds.x ||
                    input.worldPos.y < adjustedMinBounds.y || input.worldPos.y > adjustedMaxBounds.y ||
                    input.worldPos.z < adjustedMinBounds.z || input.worldPos.z > adjustedMaxBounds.z)
                {
                    discard;
                }

                half4 c = half4(input.color, _Tint.a);
                UNITY_APPLY_FOG(input.fogCoord, c);
                return c;
            }

            ENDCG
        }
    }
    CustomEditor "Pcx.PointMaterialInspector"
}
