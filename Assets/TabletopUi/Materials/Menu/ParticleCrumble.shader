// Unity built-in shader source. Copyright (c) 2016 Unity Technologies. MIT license (see license.txt)

Shader "Particles/Custom/ParticleCrumble" {
Properties {
	_TintColor ("Tint Color", Color) = (0.5,0.5,0.5,0.5)
	_MainTex ("Particle Texture", 2D) = "white" {}
}

Category {
	Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" "PreviewType"="Plane" }
	Blend SrcAlpha One
	ColorMask RGB
	Cull Off Lighting Off ZWrite Off

	SubShader {
		Pass {
		
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 2.0
			#pragma multi_compile_particles
			#pragma multi_compile_fog
			
			#include "UnityCG.cginc"

			sampler2D _MainTex;
			fixed4 _TintColor;
			
			struct appdata_t {
				float4 vertex : POSITION;
				fixed4 color : COLOR;
				float2 texcoord : TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct v2f {
				float4 vertex : SV_POSITION;
				fixed4 color : COLOR;
				float2 texcoord : TEXCOORD0;
				float4 screenPos : TEXCOORD1;
				UNITY_FOG_COORDS(1)
				UNITY_VERTEX_OUTPUT_STEREO
			};
			
			float4 _MainTex_ST;

			v2f vert (appdata_t v)
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.color = v.color * _TintColor;
				o.texcoord = TRANSFORM_TEX(v.texcoord,_MainTex);
				o.screenPos = ComputeScreenPos(v.vertex);
				UNITY_TRANSFER_FOG(o, o.vertex);
				return o;
			}

			UNITY_DECLARE_DEPTH_TEXTURE(_CameraDepthTexture);
			
			fixed4 frag (v2f i) : SV_Target
			{
				fixed4 col = tex2D(_MainTex, i.texcoord);
				col.rgb *= 2.0f * i.color;
				UNITY_APPLY_FOG(i.fogCoord, col);		
				col.a = step(1 - i.color.a, col.a);
				return col;
			}
			ENDCG 
		}
	}	
}
}
