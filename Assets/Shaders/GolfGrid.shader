Shader "Unlit/GolfGrid"
{
	Properties
	{
		_PrimaryColor ("Primary Color", Color) = (1.0, 1.0, 1.0)
		_PrimaryGradientColor ("Primary Gradient Color", Color) = (1.0, 1.0, 1.0)
		_SecondaryColor ("Secondary Color", Color) = (0.0, 0.0, 0.0)
		_SecondaryGradientColor ("Secondary Gradient Color", Color) = (0.0, 0.0, 0.0)
		_PrimaryWallColor ("Primary Wall Color", Color) = (0.0, 0.0, 0.0)
		_PrimaryWallGradientColor ("Primary Wall Gradient Color", Color) = (0.0, 0.0, 0.0)
		_SecondaryWallColor ("Secondary Wall Color", Color) = (0.0, 0.0, 0.0)
		_SecondaryWallGradientColor ("Secondary Wall Gradient Color", Color) = (0.0, 0.0, 0.0)
		_ShadowColor ("Shadow Color", Color) = (0.0, 0.0, 0.0, 0.5)
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
			fixed3 _PrimaryGradientColor;
			fixed3 _SecondaryColor;
			fixed3 _SecondaryGradientColor;
			fixed3 _PrimaryWallColor;
			fixed3 _PrimaryWallGradientColor;
			fixed3 _SecondaryWallColor;
			fixed3 _SecondaryWallGradientColor;
			fixed4 _ShadowColor;

			float _GridSize;
			float _EdgeNudge;
			float4 _GridOrigin;
			float3 _SunDirection;
			float4 _GradientMinPosition;
			float _GradientMaxDistance;

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
				float3 gridPosition = ((i.worldPosition - _GridOrigin.xyz) / _GridSize) + (normal * _EdgeNudge);
				int3 gridIndex = floor(gridPosition);
				bool isPrimary = frac(gridIndex.x * 0.5) < 0.5;
				isPrimary = isPrimary ^ (frac(gridIndex.y * 0.5) < 0.5);
				isPrimary = isPrimary ^ (frac(gridIndex.z * 0.5) < 0.5);

				bool isWall = normal.y < 0.1;
				fixed3 primaryColor = lerp(_PrimaryColor, _PrimaryWallColor, isWall);
				fixed3 primaryColorGradient = lerp(_PrimaryGradientColor, _PrimaryWallGradientColor, isWall);
				fixed3 secondaryColor = lerp(_SecondaryColor, _SecondaryWallColor, isWall);
				fixed3 secondaryColorGradient = lerp(_SecondaryGradientColor, _SecondaryWallGradientColor, isWall);
				fixed3 gridColor = lerp(secondaryColor, primaryColor, isPrimary);
				fixed3 gridColorGradient = lerp(secondaryColorGradient, primaryColorGradient, isPrimary);

				float3 gradientOffset = i.worldPosition - _GradientMinPosition;
				float gradientDistance = gradientOffset.x + gradientOffset.z;
				float gradientProgress = saturate(gradientDistance / _GradientMaxDistance);
				// gradientProgress = smoothstep(0.0, 1.0, gradientProgress);
				gridColor = lerp(gridColor, gridColorGradient, gradientProgress);

				float sunDot = dot(normal, _SunDirection);
				float sunT = 0.5 - (sunDot * 0.5);
				fixed3 shadedColor = lerp(gridColor, _ShadowColor.rgb, (1.0 - sunT) * _ShadowColor.a);

				fixed4 finalColor = fixed4(shadedColor, 1.0);
				UNITY_APPLY_FOG(i.fogCoord, finalColor);
				return finalColor;
			}
			ENDCG
		}
	}
}
