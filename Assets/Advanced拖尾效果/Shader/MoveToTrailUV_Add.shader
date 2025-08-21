/*
��β��ЧShader��֧��UV�����ͽ���Ч����
(רΪ����ϵͳ��ƣ�ͨ��UV����ʵ����β�ƶ�Ч��)
*/

Shader "MyShader/Trail"
{
	Properties
	{
		_MainTex("������ (RGB)", 2D) = "white" {}
		_MainTexVFade("������ֱ����", Range(0, 1)) = 0
		_MainTexVFadePow("��������ǿ��", Float) = 1
		_MainTexPow("������٤��ֵ", Float) = 1
		_MainTexMultiplier("���������ȱ���", Float) = 1
		_TintTex("ɫ������ (RGB)", 2D) = "white" {}
		_Multiplier("�������ȱ���", Float) = 1
		_MainScrollSpeedU("������U��������ٶ�", Float) = 10
		_MainScrollSpeedV("������V��������ٶ�", Float) = 0
	}
		SubShader
		{
			Tags { "RenderType" = "Transparent" "RenderPipeline" = "UniversalPipeline" "Queue" = "Transparent"}
			Blend One One // �ӷ����
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
					float2 uvOrigin : TEXCOORD1; // ԭʼUV����
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
					
					// ���ڿ�����βUV�ƶ��Ĳ���
					// ��������ɽű����ƣ�����ʵ����β���ƶ�Ч��
					// ͨ����̬����UVƫ������ģ����β��������
					half _MoveToMaterialUV;
				CBUFFER_END

				Varyings vert(Attributes IN)
				{
					Varyings o;
					o.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
					o.uv = TRANSFORM_TEX(IN.uv, _MainTex);
					// Ӧ��UV������ˮƽ�������ʱ���������βƫ��
					o.uv.x -= frac(_Time.x * _MainScrollSpeedU) + _MoveToMaterialUV;
					// ��ֱ����Ĺ�������
					o.uv.y -= frac(_Time.x * _MainScrollSpeedV);
					o.uvOrigin = IN.uv; // ����ԭʼUV���ڽ������
					o.color = IN.color;
					return o;
				}

				half4 frag(Varyings IN) : SV_Target
				{
					half4 mainTex = tex2D(_MainTex, IN.uv);

					// ���㴹ֱ����ĵ���Ч��
					// �����������˽��䣬Ӫ����β����Ȼ˥��Ч��
					half vFade = 1 - abs(IN.uvOrigin.y - 0.5) * 2; 
					vFade = pow(abs(vFade), _MainTexVFadePow); // ������������
					vFade = lerp(1, vFade, _MainTexVFade); // ���ƽ���ǿ��
					mainTex.rgb *= vFade; // Ӧ�ô�ֱ����
					
					// ��������������ȺͶԱȶ�
					mainTex.rgb = pow(abs(mainTex.rgb), _MainTexPow) * _MainTexMultiplier;
					
					// �������յķ���ǿ��
					half intensity = _Multiplier * IN.color.a;

					// �������ȵ�ɫ��ӳ��
					// ��RGBת��Ϊ�Ҷ�ֵ��Ȼ��ӳ�䵽ɫ������
					half avr = mainTex.r * 0.3333 + mainTex.g * 0.3334 + mainTex.b * 0.3333;
					avr = saturate(avr * intensity); 
					half4 col = tex2D(_TintTex, half2(avr, 0.5));

					// ֧��HDR����Ч������intensity����1ʱ���ֳ���Ч��
					half intensityHigh = max(1, intensity);
					col.rgb *= intensityHigh * IN.color.rgb;
					return col;
				}
				ENDHLSL
			}
		}
}