Shader "Hidden/SnapshotPro/HeightFog"
{
	SubShader
	{
		Cull Off ZWrite Off ZTest Always

		Pass
		{
			HLSLPROGRAM

			#pragma vertex vert
			#pragma fragment Frag

			#include "Packages/com.unity.postprocessing/PostProcessing/Shaders/StdLib.hlsl"

			TEXTURE2D_SAMPLER2D(_MainTex, sampler_MainTex);

			float _FogStrength;

			float _StartDistance;
			float3 _DistanceStartColor;
			float _EndDistance;
			float3 _DistanceEndColor;
			float _DistanceFalloff;

			float _StartHeight;
			float3 _HeightStartColor;
			float _EndHeight;
			float3 _HeightEndColor;
			float _HeightFalloff;

			TEXTURE2D_SAMPLER2D(_CameraDepthTexture, sampler_CameraDepthTexture);
			TEXTURE2D_SAMPLER2D(_CameraDepthNormalsTexture, sampler_CameraDepthNormalsTexture);

			float4x4 clipToWorld;

			// Credit to https://alexanderameye.github.io/outlineshader.html:
			float3 DecodeNormal(float4 enc)
			{
				float kScale = 1.7777;
				float3 nn = enc.xyz*float3(2 * kScale, 2 * kScale, 0) + float3(-kScale, -kScale, 1);
				float g = 2.0 / dot(nn.xyz, nn.xyz);
				float3 n;
				n.xy = g * nn.xy;
				n.z = g - 1;
				return n;
			}

			struct v2f 
			{
				float4 vertex : SV_POSITION;
				float2 texcoord : TEXCOORD0;
				float3 worldDir : TEXCOORD1;
			};

			v2f vert(AttributesDefault i)
			{
				v2f o;
				o.vertex = float4(i.vertex.xy, 0.0, 1.0);
				o.texcoord = TransformTriangleVertexToUV(i.vertex.xy);

#if UNITY_UV_STARTS_AT_TOP
				o.texcoord = o.texcoord * float2(1.0, -1.0) + float2(0.0, 1.0);
#endif

				float4 clip = float4(o.texcoord.xy*2-1, 0.0, 1.0);
				o.worldDir = mul(clipToWorld, clip) - _WorldSpaceCameraPos;
				return o;
			}

			float4 Frag(v2f i) : SV_Target
			{
				float3 col = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.texcoord);

#if UNITY_REVERSED_Z
				float depth = SAMPLE_TEXTURE2D(_CameraDepthTexture, sampler_CameraDepthTexture, i.texcoord).r;
				depth = LinearEyeDepth(depth);
#else
				float depth = SAMPLE_TEXTURE2D(_CameraDepthTexture, sampler_CameraDepthTexture, i.texcoord).r;
				depth = lerp(UNITY_NEAR_CLIP_VALUE, 1, LinearEyeDepth(depth));
#endif
				
				float3 worldPos = i.worldDir * depth + _WorldSpaceCameraPos;

				float worldDistance = distance(worldPos, _WorldSpaceCameraPos);

				float distanceMask = saturate((worldDistance - _StartDistance) / (_EndDistance - _StartDistance));
				distanceMask = pow(distanceMask, _DistanceFalloff);

				float heightMask = saturate((worldPos.y - _StartHeight) / (_EndHeight - _StartHeight));
				heightMask = pow(heightMask, _HeightFalloff);

				float3 fogDistanceColor = lerp(_DistanceStartColor.rgb, _DistanceEndColor.rgb, distanceMask);
				float3 fogHeightColor = lerp(_HeightStartColor, _HeightEndColor, heightMask);

				col = lerp(col, fogDistanceColor, distanceMask * _FogStrength);
				col = lerp(col, fogHeightColor, heightMask * _FogStrength);

				return float4(col, 1.0f);
			}

			ENDHLSL
		}
	}
}
