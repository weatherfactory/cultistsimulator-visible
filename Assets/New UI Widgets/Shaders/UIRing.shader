// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Ring shader.
// Should be used only "_RingColor", "_Thickness", "_Padding", "_Resolution" and "_Transparent" shader properties,
// other properties should have the default value to be compatible with Unity UI.

Shader "Custom/New UI Widgets/UIRing"
{
	Properties
	{
		// Sprite texture
		[PerRendererData] _MainTex("Sprite Texture", 2D) = "white" {}
		// Tint
		_Color("Tint", Color) = (1,1,1,1)

		_RingColor("Ring Color", Color) = (1,0,0,1)
		_Thickness("Thickness", Float) = 4
		_Padding("Padding", Float) = 0
		_Resolution("Resolution", Float) = 200
		_Transparent("Transparent", Float) = 0

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

			fixed4 _RingColor;
			float _Thickness;
			float _Padding;
			float _Resolution;
			float _Transparent;

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

			fixed4 frag(v2f IN) : SV_Target
			{
				float4 color = (tex2D(_MainTex, IN.texcoord) + _TextureSampleAdd) * IN.color;
				float2 pos = IN.texcoord1 - float2(0.5, 0.5);
				if (_Transparent > 0)
				{
					color.rgb = _RingColor.rgb;
					color.a = 0;
				}

				float quality = 2.0 / _Resolution;
				float outer_radius = 0.5 - ((_Padding + 1) / _Resolution);
				float inner_radius = max(0, min(outer_radius - (_Thickness / _Resolution), outer_radius - (1.0 / _Resolution)));
				float outer_alpha = 1.0 - smoothstep(outer_radius - (outer_radius * quality), outer_radius + (outer_radius * quality), length(pos));
				float inner_alpha = 1.0 - smoothstep(inner_radius - (inner_radius * quality), inner_radius + (inner_radius * quality), length(pos));

				float4 temp_color = lerp(color, _RingColor, outer_alpha);
				color = lerp(temp_color, color, inner_alpha);

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