Shader "Custom/Distort" {
	Properties {
		_DistTex("Distortion Texture", 2D) = "grey" {}
		_DistMask("Distortion Mask", 2D) = "white" {}
		[HideInInspector] _MainTex("Tint Color (RGB)", 2D) = "white" {}
	}

	Category {
		// We must be transparent, so other objects are drawn before this one.
		Tags{ "Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Opaque" }

		SubShader {

			GrabPass {
				"_GrabTex"
			}

			Pass {
				CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag
				#pragma fragmentoption ARB_precision_hint_fastest
				#include "UnityCG.cginc"

				struct appdata_t {
					float4 vertex : POSITION;
					float2 texcoord: TEXCOORD0;
				};

				struct v2f {
					float4 vertex : POSITION;
					float4 uvgrab : TEXCOORD0;
					float2 uvmain : TEXCOORD1;
				};

				sampler2D _MainTex;
				float4 _MainTex_ST;

				v2f vert(appdata_t v)
				{
					v2f o;
					o.vertex = UnityObjectToClipPos(v.vertex);

			#if UNITY_UV_STARTS_AT_TOP
					float scale = -1.0;
			#else
					float scale = 1.0;
			#endif

					o.uvgrab.xy = (float2(o.vertex.x, o.vertex.y * scale) + o.vertex.w) * 0.5;
					o.uvgrab.zw = o.vertex.zw;

					o.uvmain = TRANSFORM_TEX(v.texcoord, _MainTex);
					return o;
				}

				sampler2D _DistTex;
				sampler2D _DistMask;
				sampler2D _GrabTex;

				half4 frag(v2f i) : COLOR
				{
					float2 distScroll = float2(_Time.x * 0.2, _Time.x * 0.5);
					fixed2 dist = (tex2D(_DistTex, i.uvmain + distScroll).rg - 0.5) * 2;
					fixed distMask = tex2D(_DistMask, i.uvmain)[0];

					return tex2D(_GrabTex, UNITY_PROJ_COORD(i.uvgrab) + dist * distMask * 0.025);
				}
				ENDCG
			}

		}
	}
}