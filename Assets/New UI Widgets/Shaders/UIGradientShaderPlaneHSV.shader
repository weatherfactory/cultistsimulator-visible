// Display the four colors interpolation between HSV colors from the side.
// _ColorLeft - HSV color on the left side.
// _ColorRight - HSV color on the right side.
// _ColorBottom - HSV color on the bottom side.
// _ColorTop - HSV color on the bottom side.
// Should be used only "_ColorLeft", "_ColorRight", "_ColorBottom", and "_ColorTop" shader properties,
// other properties should have the default value to be compatible with Unity UI.

Shader "Custom/UIWidgets/GradientShaderPlaneHSV"
{
	Properties
	{
		// Sprite texture
		[PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
		// Tint
		_Color ("Tint", Color) = (1,1,1,1)
		// HSV color on the left side
		_ColorLeft ("Color left", Color) = (1,1,1,1)
		// HSV color on the right side
		_ColorRight ("Color right", Color) = (1,1,1,1)
		// HSV color on the bottom side
		_ColorBottom ("Color bottom", Color) = (1,1,1,1)
		// HSV color on the top side
		_ColorTop ("Color top", Color) = (1,1,1,1)
		
		_StencilComp ("Stencil Comparison", Float) = 8
		_Stencil ("Stencil ID", Float) = 0
		_StencilOp ("Stencil Operation", Float) = 0
		_StencilWriteMask ("Stencil Write Mask", Float) = 255
		_StencilReadMask ("Stencil Read Mask", Float) = 255

		_ColorMask ("Color Mask", Float) = 15
	}

	SubShader
	{
		Tags
		{ 
			"Queue"="Transparent" 
			"IgnoreProjector"="True" 
			"RenderType"="Transparent" 
			"PreviewType"="Plane"
			"CanUseSpriteAtlas"="True"
		}
		
		Stencil
		{
			Ref [_Stencil]
			Comp [_StencilComp]
			Pass [_StencilOp] 
			ReadMask [_StencilReadMask]
			WriteMask [_StencilWriteMask]
		}

		Cull Off
		Lighting Off
		ZWrite Off
		ZTest [unity_GUIZTestMode]
		Fog { Mode Off }
		Blend SrcAlpha OneMinusSrcAlpha
		ColorMask [_ColorMask]

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
				fixed4 color    : COLOR;
				half2 texcoord  : TEXCOORD0;
			};
			
			fixed4 _Color;
			float4 _ColorLeft;
			float4 _ColorRight;
			float4 _ColorTop;
			float4 _ColorBottom;
						
			v2f vert(appdata_t IN)
			{
				v2f OUT;
				OUT.vertex = UnityObjectToClipPos(IN.vertex);
				OUT.texcoord = IN.texcoord;
				#ifdef UNITY_HALF_TEXEL_OFFSET
				OUT.vertex.xy += (_ScreenParams.zw-1.0) * float2(-1,1);
				#endif
				OUT.color = IN.color * _Color;
				return OUT;
			}

			sampler2D _MainTex;

			fixed4 frag(v2f IN) : SV_Target
			{
				#if defined(UIWIDGETS_COLORSPACE_GAMMA) || defined(UNITY_COLORSPACE_GAMMA)
				half4 color = tex2D(_MainTex, IN.texcoord) * IN.color * HSVtoRGB(lerp(_ColorLeft, _ColorRight, IN.texcoord.x) + lerp(_ColorBottom, _ColorTop, IN.texcoord.y));
				#else
				half4 color = tex2D(_MainTex, IN.texcoord) * IN.color * HSVtoRGB(lerp(LinearToGammaSpace4(_ColorLeft), LinearToGammaSpace4(_ColorRight), IN.texcoord.x) + lerp(LinearToGammaSpace4(_ColorBottom), LinearToGammaSpace4(_ColorTop), IN.texcoord.y));
				color = GammaToLinearSpace4(color);
				#endif

				clip(color.a - 0.01);

				return color;
			}
		ENDCG
		}
	}
}
