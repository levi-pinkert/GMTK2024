Shader "Unlit/GolfWater"
{
    Properties
    {
        _PrimaryColor ("Primary Color", Color) = (1.0, 1.0, 1.0)
        _SecondaryColor ("Secondary Color", Color) = (0.0, 0.0, 0.0)
        _NoiseScale ("Noise Scale", Float) = 1.0
        _RippleOffset ("Ripple Offset", Float) = 0.1
        _GridMultiplier ("Grid Multiplier", Float) = 2.0
        _RippleRate ("Ripple Rate", Float) = 1.0
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
                float3 worldPosition : POSITIONT;
                float3 normal : NORMAL;
            };

            fixed3 _PrimaryColor;
            fixed3 _SecondaryColor;
            float _NoiseScale;
            float _RippleOffset;
            float _GridMultiplier;
            float _RippleRate;

            float _GridSize;
            float _EdgeNudge;
            float4 _GridOrigin;

            float2 unity_gradientNoise_dir(float2 p)
            {
                p = p % 289;
                float x = (34 * p.x + 1) * p.x % 289 + p.y;
                x = (34 * x + 1) * x % 289;
                x = frac(x / 41) * 2 - 1;
                return normalize(float2(x - floor(x + 0.5), abs(x) - 0.5));
            }

            float unity_gradientNoise(float2 p)
            {
                float2 ip = floor(p);
                float2 fp = frac(p);
                float d00 = dot(unity_gradientNoise_dir(ip), fp);
                float d01 = dot(unity_gradientNoise_dir(ip + float2(0, 1)), fp - float2(0, 1));
                float d10 = dot(unity_gradientNoise_dir(ip + float2(1, 0)), fp - float2(1, 0));
                float d11 = dot(unity_gradientNoise_dir(ip + float2(1, 1)), fp - float2(1, 1));
                fp = fp * fp * fp * (fp * (fp * 6 - 15) + 10);
                return lerp(lerp(d00, d01, fp.y), lerp(d10, d11, fp.y), fp.x);
            }

            void Unity_GradientNoise_float(float2 UV, float Scale, out float Out)
            {
                Out = unity_gradientNoise(UV * Scale) + 0.5;
            }

            v2f vert (appdata v)
            {
                v2f o;
                o.worldPosition = mul(unity_ObjectToWorld, float4(v.vertex.xyz, 1.0)).xyz;
                o.normal = mul(unity_ObjectToWorld, float4(v.normal, 0.0)).xyz;
                o.vertex = UnityObjectToClipPos(v.vertex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float3 normal = normalize(i.normal);
                float fullGridSize = _GridSize * _GridMultiplier;
                float3 gridPosition = ((i.worldPosition - _GridOrigin.xyz) / fullGridSize) + (normal * _EdgeNudge);
                float perlinNoise;
                float2 noiseSamplePos = gridPosition.xz + (_Time.y * _RippleRate * float2(1.0, 1.0));
                Unity_GradientNoise_float(noiseSamplePos, _NoiseScale, perlinNoise);
                gridPosition += float3(1.0, 0.0, 1.0) * (perlinNoise * _RippleOffset);

                int3 gridIndex = floor(gridPosition);
                bool isPrimary = frac(gridIndex.x * 0.5) < 0.5;
                isPrimary = isPrimary ^ (frac(gridIndex.y * 0.5) < 0.5);
                isPrimary = isPrimary ^ (frac(gridIndex.z * 0.5) < 0.5);
                fixed3 gridColor = lerp(_SecondaryColor, _PrimaryColor, isPrimary);

                fixed4 finalColor = fixed4(gridColor, 1.0);
                UNITY_APPLY_FOG(i.fogCoord, finalColor);
                return finalColor;
            }
            ENDCG
        }
    }
}
