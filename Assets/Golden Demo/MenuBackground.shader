// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/MenuBackground"
{
	Properties
	{
		_MainTex("Texture", 2D) = "black" {}

		_EffectsLayer1Tex("Effect Texture", 2D) = "black"{}
		_EffectsLayer1Color("Effect Color", Color) = (1,1,1,1)
		_EffectsLayer1Motion("Effect Motion Texture", 2D) = "black"{}
		_EffectsLayer1MotionSpeed("Effect Speed", float) = 0 
		_EffectsLayer1Rotation("", float) = 0
		_EffectsLayer1PivotScale("", Vector) = (0.5,0.5,1,1)
		_EffectsLayer1Translation("", Vector) = (0,0,0,0)
	}

		SubShader
		{
			Tags {
				"Queue" = "Transparent"
				"RenderType" = "Transparent"
				"PreviewType" = "Plane"
			}
			LOD 100
			ZWrite Off

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float4 vertex : SV_POSITION;
				float2 uv : TEXCOORD0;
				float2 effect1uv : TEXCOORD1;
			};

			sampler2D _MainTex;

			sampler2D _EffectsLayer1Tex;
			sampler2D _EffectsLayer1Motion;
			float _EffectsLayer1MotionSpeed;
			float _EffectsLayer1Rotation;
			float4 _EffectsLayer1PivotScale;
			half4 _EffectsLayer1Color;
			float2 _EffectsLayer1Translation;

			v2f vert(appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;

				float2x2 rotationMatrix;
				float sinTheta;
				float cosTheta;

				// For each effect channel, calculate UV rotations and scale about the pivot, and translate the point.
				o.effect1uv = o.uv - _EffectsLayer1PivotScale.xy;
				sinTheta = sin(_EffectsLayer1Rotation * _Time);
				cosTheta = cos(_EffectsLayer1Rotation * _Time);
				rotationMatrix = float2x2(cosTheta, -sinTheta, sinTheta, cosTheta);
				o.effect1uv = (mul( (o.effect1uv - _EffectsLayer1Translation.xy) *
					(1 / _EffectsLayer1PivotScale.zw), rotationMatrix)
					+ _EffectsLayer1PivotScale.xy);

				return o;
			}

			float _DistStrength;
			float _DistSpeed;

			fixed4 frag(v2f i) : SV_Target
			{
				fixed4 col = tex2D(_MainTex, i.uv);
				fixed bg = col.a;
				fixed4 motion1 = tex2D(_EffectsLayer1Motion, i.uv);

				if (_EffectsLayer1MotionSpeed)
					motion1.y -= _Time.x * _EffectsLayer1MotionSpeed;
				else
					motion1 = fixed4(i.effect1uv.rg, motion1.b, motion1.a);

				fixed4 effect1 = tex2D(_EffectsLayer1Tex, motion1.xy) * motion1.a;
				effect1 *= _EffectsLayer1Color;

				col = col * (1 - effect1.a) + effect1 * effect1.a;

				return col;
		}
		ENDCG
	}
	}

	CustomEditor "BackgroundMaterialEditor"
}
