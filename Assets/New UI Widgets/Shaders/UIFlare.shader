// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Display the two colors interpolation between RGB colors from the left side to the right side.
// _ColorFlare - RGB color on the left side.
// Should be used only "_ColorFlare" and "_WidthFlare" shader property
// other properties should have the default value to be compatible with Unity UI.

Shader "Custom/UIWidgets/UIFlare"
{
	Properties
	{
		// Sprite texture
		[PerRendererData] _MainTex("Sprite Texture", 2D) = "white" {}
		// Tint
		_Color("Tint", Color) = (1,1,1,1)
		// Flare
		_FlareColor("Flare Color", Color) = (1,1,1,1)
		_FlareSize("Flare Size", Float) = 0.2
		_FlareSpeed("Flare Speed", Float) = 0.2

		_StencilComp("Stencil Comparison", Float) = 8
		_Stencil("Stencil ID", Float) = 0
		_StencilOp("Stencil Operation", Float) = 0
		_StencilWriteMask("Stencil Write Mask", Float) = 255
		_StencilReadMask("Stencil Read Mask", Float) = 255

		_ColorMask("Color Mask", Float) = 15
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
		Fog{ Mode Off }
		Blend SrcAlpha OneMinusSrcAlpha
		ColorMask[_ColorMask]

		Pass
		{
		CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"
			#include "UIWidgets.cginc"

			struct appdata_t
			{
				float4 vertex   : POSITION;
				float4 color    : COLOR;
				float2 texcoord : TEXCOORD0;
			};

			struct v2f
			{
				float4 vertex   : SV_POSITION;
				fixed4 color : COLOR;
				half2 texcoord  : TEXCOORD0;
			};

			fixed4 _Color;
			float4 _FlareColor;
			float _FlareSize;
			float _FlareSpeed;

			v2f vert(appdata_t IN)
			{
				v2f OUT;
				OUT.vertex = UnityObjectToClipPos(IN.vertex);
				OUT.texcoord = IN.texcoord;
				#ifdef UNITY_HALF_TEXEL_OFFSET
				OUT.vertex.xy += (_ScreenParams.zw - 1.0) * float2(-1,1);
				#endif
				OUT.color = IN.color * _Color;
				return OUT;
			}

			sampler2D _MainTex;

			float flare_distance(float center, float size, float pos)
			{
				float half_size = size / 2.0;
				float left = center - half_size;
				float right = center + half_size;
				if ((left < 0) && (pos > (1.0 + left)))
				{
					pos -= 1.0;
				}
				else if ((right > 1.0) && (pos < (right - 1.0)))
				{
					pos += 1.0;
				}

				return abs(smoothstep(left, right, pos) - 0.5) * 2.0;
			}
			
			fixed4 frag(v2f IN) : SV_Target
			{
				float center = frac(_Time.y * _FlareSpeed);
				float rate = flare_distance(center, _FlareSize, IN.texcoord.x);

				half4 color = tex2D(_MainTex, IN.texcoord) * IN.color;
				#if defined(UIWIDGETS_COLORSPACE_GAMMA) || defined(UNITY_COLORSPACE_GAMMA)
				color = lerp(_FlareColor, color, rate);
				#else
				color = lerp(LinearToGammaSpace4(_FlareColor), color, rate);
				color = GammaToLinearSpace4(color);
				#endif

				clip(color.a - 0.01);

				return color;
			}
		ENDCG
		}
	}
}
