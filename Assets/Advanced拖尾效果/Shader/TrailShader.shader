/*
拖尾特效Shader，支持UV动画和渐变效果。
(专为粒子系统设计，通过UV动画实现拖尾移动效果)
*/

Shader "MyShader/Trail"
{
	Properties
	{
		_MainTex("主纹理 (RGB)", 2D) = "white" {}
		_MainTexVFade("主纹理垂直渐变", Range(0, 1)) = 0
		_MainTexVFadePow("主纹理渐变强度", Float) = 1
		_MainTexPow("主纹理伽马值", Float) = 1
		_MainTexMultiplier("主纹理亮度倍数", Float) = 1
		_TintTex("色调纹理 (RGB)", 2D) = "white" {}
		_Multiplier("整体亮度倍数", Float) = 1
		_MainScrollSpeedU("主纹理U方向滚动速度", Float) = 10
		_MainScrollSpeedV("主纹理V方向滚动速度", Float) = 0
	}
		SubShader
		{
			Tags { "RenderType" = "Transparent" "RenderPipeline" = "UniversalPipeline" "Queue" = "Transparent"}
			Blend One One // 加法混合
			ZWrite Off

			Pass
			{
				HLSLPROGRAM
				#pragma vertex vert
				#pragma fragment frag
				#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

				struct Attributes
				{
					float4 positionOS : POSITION;
					float2 uv : TEXCOORD0;
					half4 color : COLOR;
				};

				struct Varyings
				{
					float2 uv : TEXCOORD0;
					float2 uvOrigin : TEXCOORD1; // 原始UV坐标
					float4 positionHCS : SV_POSITION;
					half4 color : COLOR;
				};

				sampler2D _MainTex;
				sampler2D _TintTex;

				CBUFFER_START(UnityPerMaterial)
					half4 _MainTex_ST;
					half _MainTexVFade;
					half _MainTexVFadePow;
					half _MainTexPow;
					half _MainTexMultiplier;
					half _Multiplier;
					half _MainScrollSpeedU;
					half _MainScrollSpeedV;
					
					// 用于控制拖尾UV移动的参数
					// 这个变量由脚本控制，用来实现拖尾的移动效果
					// 通过动态调整UV偏移量来模拟拖尾的流动感
					half _MoveToMaterialUV;
				CBUFFER_END

				Varyings vert(Attributes IN)
				{
					Varyings o;
					o.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
					o.uv = TRANSFORM_TEX(IN.uv, _MainTex);
					// 应用UV动画：水平方向加上时间滚动和拖尾偏移
					o.uv.x -= frac(_Time.x * _MainScrollSpeedU) + _MoveToMaterialUV;
					// 垂直方向的滚动动画
					o.uv.y -= frac(_Time.x * _MainScrollSpeedV);
					o.uvOrigin = IN.uv; // 保存原始UV用于渐变计算
					o.color = IN.color;
					return o;
				}

				half4 frag(Varyings IN) : SV_Target
				{
					half4 mainTex = tex2D(_MainTex, IN.uv);

					// 计算垂直方向的淡出效果
					// 从中心向两端渐变，营造拖尾的自然衰减效果
					half vFade = 1 - abs(IN.uvOrigin.y - 0.5) * 2; 
					vFade = pow(abs(vFade), _MainTexVFadePow); // 调整渐变曲线
					vFade = lerp(1, vFade, _MainTexVFade); // 控制渐变强度
					mainTex.rgb *= vFade; // 应用垂直渐变
					
					// 调整主纹理的亮度和对比度
					mainTex.rgb = pow(abs(mainTex.rgb), _MainTexPow) * _MainTexMultiplier;
					
					// 计算最终的发光强度
					half intensity = _Multiplier * IN.color.a;

					// 基于亮度的色调映射
					// 将RGB转换为灰度值，然后映射到色调纹理
					half avr = mainTex.r * 0.3333 + mainTex.g * 0.3334 + mainTex.b * 0.3333;
					avr = saturate(avr * intensity); 
					half4 col = tex2D(_TintTex, half2(avr, 0.5));

					// 支持HDR发光效果：当intensity大于1时保持超亮效果
					half intensityHigh = max(1, intensity);
					col.rgb *= intensityHigh * IN.color.rgb;
					return col;
				}
				ENDHLSL
			}
		}
}