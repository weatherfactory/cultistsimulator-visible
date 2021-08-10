// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Ripple shader.
// Should be used only "_RippleStartColor", "_RippleEndColor", "_RippleMaxSize" and "_RippleSpeed" shader properties,
// other properties should have the default value to be compatible with Unity UI.

Shader "Custom/New UI Widgets/UIRipple"
{
	Properties
	{
		// Sprite texture
		[PerRendererData] _MainTex("Sprite Texture", 2D) = "white" {}
		// Tint
		_Color("Tint", Color) = (1,1,1,1)

		_RippleStartColor("Start Color", Color) = (1,0,0,1)
		_RippleEndColor("End Color", Color) = (1,0,0,1)
		_RippleSpeed("Speed", Float) = 0.5
		_RippleMaxSize("Max Size", Float) = 1

		_StencilComp("Stencil Comparison", Float) = 8
		_Stencil("Stencil ID", Float) = 0
		_StencilOp("Stencil Operation", Float) = 0
		_StencilWriteMask("Stencil Write Mask", Float) = 255
		_StencilReadMask("Stencil Read Mask", Float) = 255

		_ColorMask("Color Mask", Float) = 15

		[Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip("Use Alpha Clip", Float) = 0
	}

	SubShader
	{
		Tags
		{
			"Queue" = "Transparent"
			"IgnoreProjector" = "True"
			"RenderType" = "Transparent"
			"PreviewType" = "Plane"
			"CanUseSpriteAtlas" = "True"
		}

		Stencil
		{
			Ref[_Stencil]
			Comp[_StencilComp]
			Pass[_StencilOp]
			ReadMask[_StencilReadMask]
			WriteMask[_StencilWriteMask]
		}

		Cull Off
		Lighting Off
		ZWrite Off
		ZTest[unity_GUIZTestMode]
		Blend SrcAlpha OneMinusSrcAlpha
		ColorMask[_ColorMask]

		Pass
		{
			Name "Default"

		CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 2.0

			#include "UnityCG.cginc"
			#include "UnityUI.cginc"

			#pragma multi_compile_local _ UNITY_UI_CLIP_RECT
			#pragma multi_compile_local _ UNITY_UI_ALPHACLIP

			struct appdata_t
			{
				float4 vertex    : POSITION;
				float4 color     : COLOR;
				float2 texcoord  : TEXCOORD0;
				float2 texcoord1 : TEXCOORD1;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct v2f
			{
				float4 vertex        : SV_POSITION;
				fixed4 color         : COLOR;
				float2 texcoord      : TEXCOORD0;
				float4 worldPosition : TEXCOORD1;

				float2 texcoord1     : TEXCOORD2;
				UNITY_VERTEX_OUTPUT_STEREO
			};

			sampler2D _MainTex;
			fixed4 _Color;
			fixed4 _TextureSampleAdd;
			float4 _ClipRect;
			float4 _MainTex_ST;

			fixed4 _RippleStartColor;
			fixed4 _RippleEndColor;
			float _RippleSpeed;
			float _RippleMaxSize;

			int _RippleCount = 0;
			float _Ripple[30]; // 10 * [x, y, start time]

			v2f vert(appdata_t v)
			{
				v2f OUT;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);
				OUT.worldPosition = v.vertex;
				OUT.vertex = UnityObjectToClipPos(OUT.worldPosition);

				OUT.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);

				OUT.color = v.color * _Color;

				OUT.texcoord1 = v.texcoord1.xy;

				return OUT;
			}

			inline float CircleAlpha(in float2 pos, in float radius)
			{
				float radius2 = radius * 1;
				float alpha = 1.0 - smoothstep(radius - (radius * 0.01), radius2 + (radius2 * 0.01), dot(pos, pos) * 2.0);

				return alpha;
			}

			fixed4 frag(v2f IN) : SV_Target
			{
				float4 color = (tex2D(_MainTex, IN.texcoord) + _TextureSampleAdd) * IN.color;
				float radius = _RippleMaxSize / 2;

				float ttl = _RippleMaxSize / _RippleSpeed;
				float3 circle_color;
				for (int i = 0; i < _RippleCount; i++)
				{
					float lived = _Time.y - _Ripple[i * 3 + 2];
					if ((lived < ttl) && (_Ripple[i * 3] > -1))
					{
						float lived_percent = lived / ttl;
						float4 ripple_color = lerp(_RippleStartColor, _RippleEndColor, lived_percent);
						circle_color = lerp(color.rgb, ripple_color.rgb, ripple_color.a);

						float2 center = float2(_Ripple[i * 3], _Ripple[i * 3 + 1]);
						float a = CircleAlpha(IN.texcoord1 - center, radius * lived_percent);
						color.rgb = lerp(color.rgb, circle_color.rgb, a);
					}
				}

				#ifdef UNITY_UI_CLIP_RECT
				color.a *= UnityGet2DClipping(IN.worldPosition.xy, _ClipRect);
				#endif

				#ifdef UNITY_UI_ALPHACLIP
				clip(color.a - 0.001);
				#endif

				return color;
			}
		ENDCG
		}
	}
}
