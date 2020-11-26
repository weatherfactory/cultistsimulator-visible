Shader "Custom/MapBurn"
{
	Properties
	{
		[PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
		
		_Color ("Tint", Color) = (1,1,1,1)
		
		_FadeTex("Fade Texture", 2D) = "white" {}
		_FadeSoftness("Fade Softness", Float) = 0.2

		_RampTex("Ramp Texture", 2D) = "white" {}

		_StencilComp ("Stencil Comparison", Float) = 8
		_Stencil ("Stencil ID", Float) = 0
		_StencilOp ("Stencil Operation", Float) = 0
		_StencilWriteMask ("Stencil Write Mask", Float) = 255
		_StencilReadMask ("Stencil Read Mask", Float) = 255

		_ColorMask ("Color Mask", Float) = 15

		[Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip ("Use Alpha Clip", Float) = 0
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
		Blend SrcAlpha OneMinusSrcAlpha
		ColorMask [_ColorMask]

		Pass
		{
			Name "Default"
		CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 2.0

			#include "UnityCG.cginc"
			#include "UnityUI.cginc"

			#pragma multi_compile __ UNITY_UI_ALPHACLIP
			
			struct appdata_t
			{
				float4 vertex   : POSITION;
				float4 color    : COLOR;
				float2 texcoord : TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct v2f
			{
				float4 vertex   : SV_POSITION;
				fixed4 color    : COLOR;
				float2 texcoord  : TEXCOORD0;
				float4 worldPosition : TEXCOORD1;
				float2 texcoord2  : TEXCOORD2;
				UNITY_VERTEX_OUTPUT_STEREO
			};
			
			fixed4 _Color;
			fixed4 _TextureSampleAdd;
			float4 _FadeTex_ST;
			float4 _ClipRect;

			v2f vert(appdata_t IN)
			{
				v2f OUT;
				UNITY_SETUP_INSTANCE_ID(IN);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);
				OUT.worldPosition = IN.vertex;
				OUT.vertex = UnityObjectToClipPos(OUT.worldPosition);

				OUT.texcoord2 = IN.texcoord;
				OUT.texcoord = TRANSFORM_TEX(IN.texcoord, _FadeTex);
				
				OUT.color = IN.color * _Color;
				return OUT;
			}

			sampler2D _MainTex;
			sampler2D _FadeTex;
			sampler2D _RampTex;
			float _FadeSoftness;

			fixed4 frag(v2f IN) : SV_Target
			{
				half4 mapTex = tex2D(_FadeTex, IN.texcoord) + _TextureSampleAdd;
				half2 rampPos = half2(mapTex.r * 0.5 + 1.0 - IN.color.r * 2.0, IN.texcoord.x * 0.5 + IN.color.r * 0.5);

				// IN.color.r is determines the burn anim (ramp over red values)
				// IN.color.g is determines the vanish anim (green values to change alpha)
				// IN.color.b determines the blend between Main and Ramp Tex

				half4 color = tex2D(_RampTex, rampPos);

				color.rgb = lerp(color.rgb, tex2D(_MainTex, IN.texcoord2).rgb + _TextureSampleAdd.rgb, IN.color.b);

				color.a *= mapTex.a * IN.color.a;
				color.a *= smoothstep(mapTex.g, mapTex.g + _FadeSoftness, (1 + _FadeSoftness) * IN.color.g );
				color.a *= UnityGet2DClipping(IN.worldPosition.xy, _ClipRect);
				
				#ifdef UNITY_UI_ALPHACLIP
				clip (color.a - 0.001);
				#endif

				return color;
			}
		ENDCG
		}
	}
}
