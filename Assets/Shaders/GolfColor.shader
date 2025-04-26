Shader "Unlit/GolfColor"
{
    Properties
    {
        _BrightColor ("Bright Color", Color) = (1.0, 1.0, 1.0)
        _ShadowColor ("Shadow Color", Color) = (1.0, 1.0, 1.0)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
                float3 normal : NORMAL;
            };

            fixed3 _BrightColor;
            fixed3 _ShadowColor;
            float3 _SunDirection;

            v2f vert (appdata v)
            {
                v2f o;
                o.normal = mul(unity_ObjectToWorld, float4(v.normal, 0.0)).xyz;
                o.vertex = UnityObjectToClipPos(v.vertex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float3 normal = normalize(i.normal);
                float sunDot = dot(normal, _SunDirection);
                float sunT = 0.5 - (sunDot * 0.5);
                fixed3 color = lerp(_ShadowColor, _BrightColor, sunT);

                fixed4 finalColor = fixed4(color, 1.0);
                UNITY_APPLY_FOG(i.fogCoord, finalColor);
                return finalColor;

            }
            ENDCG
        }
    }
}
