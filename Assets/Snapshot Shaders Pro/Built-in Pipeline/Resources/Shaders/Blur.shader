Shader "Hidden/SnapshotPro/Blur"
{
	HLSLINCLUDE
	#include "Packages/com.unity.postprocessing/PostProcessing/Shaders/StdLib.hlsl"

	TEXTURE2D_SAMPLER2D(_MainTex, sampler_MainTex);
	float2 _MainTex_TexelSize;
	uint _KernelSize;
	float _Spread;
	uint _BlurStepSize;

	// Define Gaussian function constants.
	static const float E = 2.71828f;

	inline float gaussianWeight(int x, float constant, float exponent)
	{
		return constant * pow(E, -(x * x) * exponent);
	}
	ENDHLSL

	SubShader
	{
		Cull Off ZWrite Off ZTest Always

		Pass
		{
			Name "HorizontalGaussian"
			HLSLPROGRAM

			#pragma vertex VertDefault
			#pragma fragment FragHorizontal

			float4 FragHorizontal(VaryingsDefault i) : SV_Target
			{
				float3 col = float3(0.0f, 0.0f, 0.0f);
				float kernelSum = 0.0f;

				// Calculate part of gaussian weight expression.
                float spreadSqu = _Spread * _Spread;
                float constant = 1.0f / sqrt(TWO_PI * spreadSqu);
                float exponent = 1.0f / (2 * spreadSqu);

				int upper = ((_KernelSize - 1) / 2);
				int lower = -upper;

				float2 uv;

				for (int x = lower; x <= upper; x += _BlurStepSize)
				{
					float gauss = gaussianWeight(x, constant, exponent);
					kernelSum += gauss;
					uv = i.texcoord + float2(_MainTex_TexelSize.x * x, 0.0f);
					col += gauss * SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv).rgb;
				}

				col /= kernelSum;

				return float4(col, 1.0f);
			}

			ENDHLSL
		}

		Pass
		{
			Name "VerticalGaussian"
			HLSLPROGRAM

			#pragma vertex VertDefault
			#pragma fragment FragVertical

			float4 FragVertical(VaryingsDefault i) : SV_Target
			{
				float3 col = float3(0.0f, 0.0f, 0.0f);
				float kernelSum = 0.0f;

				// Calculate part of gaussian weight expression.
                float spreadSqu = _Spread * _Spread;
                float constant = 1.0f / sqrt(TWO_PI * spreadSqu);
                float exponent = 1.0f / (2 * spreadSqu);

				int upper = ((_KernelSize - 1) / 2);
				int lower = -upper;

				float2 uv;

				for (int y = lower; y <= upper; y += _BlurStepSize)
				{
					float gauss = gaussianWeight(y, constant, exponent);
					kernelSum += gauss;
					uv = i.texcoord + float2(0.0f, _MainTex_TexelSize.y * y);
					col += gauss * SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv).rgb;
				}

				col /= kernelSum;
				return float4(col, 1.0f);
			}

			ENDHLSL
		}

		Pass
		{
			Name "HorizontalBox"
			HLSLPROGRAM

			#pragma vertex VertDefault
			#pragma fragment FragHorizontal

			float4 FragHorizontal(VaryingsDefault i) : SV_Target
			{
				float3 col = float3(0.0f, 0.0f, 0.0f);
				float kernelSum = 0.0f;

				int upper = ((_KernelSize - 1) / 2);
				int lower = -upper;

				float2 uv;

				for (int x = lower; x <= upper; x += _BlurStepSize)
				{
					kernelSum++;
					uv = i.texcoord + float2(_MainTex_TexelSize.x * x, 0.0f);
					col += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv).rgb;
				}

				col /= kernelSum;

				return float4(col, 1.0f);
			}

			ENDHLSL
		}

		Pass
		{
			Name "VerticalBox"
			HLSLPROGRAM

			#pragma vertex VertDefault
			#pragma fragment FragVertical

			float4 FragVertical(VaryingsDefault i) : SV_Target
			{
				float3 col = float3(0.0f, 0.0f, 0.0f);
				float kernelSum = 0.0f;

				int upper = ((_KernelSize - 1) / 2);
				int lower = -upper;

				float2 uv;

				for (int y = lower; y <= upper; y += _BlurStepSize)
				{
					kernelSum++;
					uv = i.texcoord + float2(0.0f, _MainTex_TexelSize.y * y);
					col += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv).rgb;
				}

				col /= kernelSum;
				return float4(col, 1.0f);
			}

			ENDHLSL
		}
	}
}
