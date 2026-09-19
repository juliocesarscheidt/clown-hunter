Shader "Hidden/SnapshotPro/Posterize"
{
	SubShader
	{
		Cull Off ZWrite Off ZTest Always

		Pass
		{
			HLSLPROGRAM

			#pragma vertex VertDefault
			#pragma fragment Frag

			#include "Packages/com.unity.postprocessing/PostProcessing/Shaders/StdLib.hlsl"

			TEXTURE2D_SAMPLER2D(_MainTex, sampler_MainTex);
			int _RedLevels;
			int _GreenLevels;
			int _BlueLevels;
			float _PowerRamp;

			float4 Frag(VaryingsDefault i) : SV_Target
			{
				float3 col = saturate(SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.texcoord).rgb);
				col = pow(col, _PowerRamp);

				int r = lerp(0.0f, 1.0f - EPSILON, col.r) * _RedLevels;
				int g = lerp(0.0f, 1.0f - EPSILON, col.g) * _GreenLevels;
				int b = lerp(0.0f, 1.0f - EPSILON, col.b) * _BlueLevels;

				return float4(r / (float)_RedLevels, g / (float)_GreenLevels, b / (float)_BlueLevels, 1.0f);
			}

			ENDHLSL
		}
	}
}
