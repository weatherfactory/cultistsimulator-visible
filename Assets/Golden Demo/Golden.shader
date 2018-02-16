// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Hearthstone/Golden"
{
	Properties
	{
		_MainTex("Texture", 2D) = "black" {}
		_DistTex("Distortion Texture", 2D) = "grey" {}
		_DistMask("Distortion Mask", 2D) = "black" {}

		_EffectsLayer1Tex("", 2D) = "black"{}
		_EffectsLayer1Color("", Color) = (1,1,1,1)
		_EffectsLayer1Motion("", 2D) = "black"{}
		_EffectsLayer1MotionSpeed("", float) = 0 
		_EffectsLayer1Rotation("", float) = 0
		_EffectsLayer1PivotScale("", Vector) = (0.5,0.5,1,1)
		_EffectsLayer1Translation("", Vector) = (0,0,0,0)
		_EffectsLayer1Foreground("", float) = 0

		_EffectsLayer2Tex("", 2D) = "black"{}
		_EffectsLayer2Color("", Color) = (1,1,1,1)
		_EffectsLayer2Motion("", 2D) = "black"{}
		_EffectsLayer2MotionSpeed("", float) = 0
		_EffectsLayer2Rotation("", float) = 0
		_EffectsLayer2PivotScale("", Vector) = (0.5,0.5,1,1)
		_EffectsLayer2Translation("", Vector) = (0,0,0,0)
		_EffectsLayer2Foreground("", float) = 0

		_EffectsLayer3Tex("", 2D) = "black"{}
		_EffectsLayer3Color("", Color) = (1,1,1,1)
		_EffectsLayer3Motion("", 2D) = "black"{}
		_EffectsLayer3MotionSpeed("", float) = 0
		_EffectsLayer3Rotation("", float) = 0
		_EffectsLayer3PivotScale("", Vector) = (0.5,0.5,1,1)
		_EffectsLayer3Translation("", Vector) = (0,0,0,0)
		_EffectsLayer3Foreground("", float) = 0

		_EffectsLayer4Tex("", 2D) = "black"{}
		_EffectsLayer4Color("", Color) = (1,1,1,1)
		_EffectsLayer4Motion("", 2D) = "black"{}
		_EffectsLayer4MotionSpeed("", float) = 0
		_EffectsLayer4Rotation("", float) = 0
		_EffectsLayer4PivotScale("", Vector) = (0.5,0.5,1,1)
		_EffectsLayer4Translation("", Vector) = (0,0,0,0)
		_EffectsLayer4Foreground("", float) = 0
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
			#pragma shader_feature EFFECTS_LAYER_1_OFF EFFECTS_LAYER_1_ON
			#pragma shader_feature EFFECTS_LAYER_2_OFF EFFECTS_LAYER_2_ON
			#pragma shader_feature EFFECTS_LAYER_3_OFF EFFECTS_LAYER_3_ON
			#pragma shader_feature EFFECTS_LAYER_4_OFF EFFECTS_LAYER_4_ON

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

#if EFFECTS_LAYER_1_ON	
				float2 effect1uv : TEXCOORD1;
#endif
#if EFFECTS_LAYER_2_ON	
				float2 effect2uv : TEXCOORD2;
#endif
#if EFFECTS_LAYER_3_ON	
				float2 effect3uv : TEXCOORD3;
#endif
#if EFFECTS_LAYER_4_ON	
				float2 effect4uv : TEXCOORD4;
#endif
			};

			sampler2D _MainTex;
			sampler2D _DistTex;
			sampler2D _DistMask;

			sampler2D _EffectsLayer1Tex;
			sampler2D _EffectsLayer1Motion;
			float _EffectsLayer1MotionSpeed;
			float _EffectsLayer1Rotation;
			float4 _EffectsLayer1PivotScale;
			half4 _EffectsLayer1Color;
			float _EffectsLayer1Foreground;
			float2 _EffectsLayer1Translation;

			sampler2D _EffectsLayer2Tex;
			sampler2D _EffectsLayer2Motion;
			float _EffectsLayer2MotionSpeed;
			float _EffectsLayer2Rotation;
			float4 _EffectsLayer2PivotScale;
			half4 _EffectsLayer2Color;
			float _EffectsLayer2Foreground;
			float2 _EffectsLayer2Translation;

			sampler2D _EffectsLayer3Tex;
			sampler2D _EffectsLayer3Motion;
			float _EffectsLayer3MotionSpeed;
			float _EffectsLayer3Rotation;
			float4 _EffectsLayer3PivotScale;
			half4 _EffectsLayer3Color;
			float _EffectsLayer3Foreground;
			float2 _EffectsLayer3Translation;

			sampler2D _EffectsLayer4Tex;
			sampler2D _EffectsLayer4Motion;
			float _EffectsLayer4MotionSpeed;
			float _EffectsLayer4Rotation;
			float4 _EffectsLayer4PivotScale;
			half4 _EffectsLayer4Color;
			float _EffectsLayer4Foreground;
			float2 _EffectsLayer4Translation;

			v2f vert(appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;

				float2x2 rotationMatrix;
				float sinTheta;
				float cosTheta;

				// For each effect channel, calculate UV rotations and scale about the pivot, and translate the point.

#if EFFECTS_LAYER_1_ON		
				o.effect1uv = o.uv - _EffectsLayer1PivotScale.xy;
				sinTheta = sin(_EffectsLayer1Rotation * _Time);
				cosTheta = cos(_EffectsLayer1Rotation * _Time);
				rotationMatrix = float2x2(cosTheta, -sinTheta, sinTheta, cosTheta);
				o.effect1uv = (mul( (o.effect1uv - _EffectsLayer1Translation.xy) *
					(1 / _EffectsLayer1PivotScale.zw), rotationMatrix)
					+ _EffectsLayer1PivotScale.xy);
#endif

#if EFFECTS_LAYER_2_ON		
				o.effect2uv = o.uv - _EffectsLayer2PivotScale.xy;
				sinTheta = sin(_EffectsLayer2Rotation * _Time);
				cosTheta = cos(_EffectsLayer2Rotation * _Time);
				rotationMatrix = float2x2(cosTheta, -sinTheta, sinTheta, cosTheta);
				o.effect2uv = (mul((o.effect2uv - _EffectsLayer2Translation.xy) * (1 / _EffectsLayer2PivotScale.zw), rotationMatrix) + _EffectsLayer2PivotScale.xy);
#endif

#if EFFECTS_LAYER_3_ON		
				o.effect3uv = o.uv - _EffectsLayer3PivotScale.xy;
				sinTheta = sin(_EffectsLayer3Rotation * _Time);
				cosTheta = cos(_EffectsLayer3Rotation * _Time);
				rotationMatrix = float2x2(cosTheta, -sinTheta, sinTheta, cosTheta);
				o.effect3uv = (mul((o.effect3uv - _EffectsLayer3Translation.xy) * (1 / _EffectsLayer3PivotScale.zw), rotationMatrix) + _EffectsLayer3PivotScale.xy);
#endif

#if EFFECTS_LAYER_4_ON		
				o.effect4uv = o.uv - _EffectsLayer4PivotScale.xy;
				sinTheta = sin(_EffectsLayer4Rotation * _Time);
				cosTheta = cos(_EffectsLayer4Rotation * _Time);
				rotationMatrix = float2x2(cosTheta, -sinTheta, sinTheta, cosTheta);
				o.effect4uv = (mul((o.effect4uv - _EffectsLayer4Translation.xy) * (1 / _EffectsLayer4PivotScale.zw), rotationMatrix) + _EffectsLayer4PivotScale.xy);
#endif	
				return o;
			}

			fixed4 frag(v2f i) : SV_Target
			{
				float2 distScroll = float2(_Time.x * 0.2, _Time.x * 0.5);
				fixed2 dist = (tex2D(_DistTex, i.uv + distScroll).rg - 0.5) * 2;
				fixed distMask = tex2D(_DistMask, i.uv)[0];

				fixed4 col = tex2D(_MainTex, i.uv + dist * distMask * 0.025);
				fixed bg = col.a;

#if EFFECTS_LAYER_1_ON		

				// Grab the motion texture, if the speed value is non-zero, we'll use the red and green channels as the UVs for the effect texture.
				// Else, we use the EffectUVs as is, but still keep the blue and alpha channels of the motion texture for later use (blending).

				fixed4 motion1 = tex2D(_EffectsLayer1Motion, i.uv);

				if (_EffectsLayer1MotionSpeed)
					motion1.y -= _Time.x * _EffectsLayer1MotionSpeed;
				else
					motion1 = fixed4(i.effect1uv.rg, motion1.b, motion1.a);

				fixed4 effect1 = tex2D(_EffectsLayer1Tex, motion1.xy) * motion1.a;
				effect1 *= _EffectsLayer1Color;

				// To the base color, we add the effect color, multiplied by it's own alpha, and then byu the back ground mask alpha (if this effect is not in the foreground).
				// TODO: Add support for alpha blending instead of additive, some cards seem to use that.

				col += effect1 * effect1.a * max(bg, _EffectsLayer1Foreground);
#endif

#if EFFECTS_LAYER_2_ON		

				fixed4 motion2 = tex2D(_EffectsLayer2Motion, i.uv);

				if (_EffectsLayer2MotionSpeed)
					motion2.y -= _Time.x * _EffectsLayer2MotionSpeed;
				else
					motion2 = fixed4(i.effect2uv.rg, motion2.b, motion2.a);

				fixed4 effect2 = tex2D(_EffectsLayer2Tex, motion2.rg) * motion2.a;
				effect2 *= _EffectsLayer2Color;

				col += effect2 * effect2.a * max(bg, _EffectsLayer2Foreground);
#endif

#if EFFECTS_LAYER_3_ON		

				fixed4 motion3 = tex2D(_EffectsLayer3Motion, i.uv);

				if (_EffectsLayer3MotionSpeed)
					motion3.y -= _Time.x * _EffectsLayer3MotionSpeed;
				else
					motion3 = fixed4(i.effect3uv.rg, motion3.b, motion3.a);

				fixed4 effect3 = tex2D(_EffectsLayer3Tex, motion3.rg) * motion3.a;
				effect3 *= _EffectsLayer3Color;

				col += effect3 * effect3.a * max(bg, _EffectsLayer3Foreground);
#endif

#if EFFECTS_LAYER_4_ON		

				fixed4 motion4 = tex2D(_EffectsLayer4Motion, i.uv);

				if (_EffectsLayer4MotionSpeed)
					motion4.y -= _Time.x * _EffectsLayer4MotionSpeed;
				else
					motion4 = fixed4(i.effect4uv.rg, motion4.b, motion4.a);

				fixed4 effect4 = tex2D(_EffectsLayer4Tex, motion4.rg) * motion4.a;
				effect4 *= _EffectsLayer4Color;

				col += effect4 * effect4.a * max(bg, _EffectsLayer4Foreground);
#endif

				return col;
		}
		ENDCG
	}
	}

	CustomEditor "GoldenMaterialEditor"
}
