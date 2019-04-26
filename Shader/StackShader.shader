Shader "StackShader"{
	Properties{
		_Color("Color", Color)= (1,1,1,1)
		_MainTex("Base (RGB)", 2D) = "white" {}

		_Glossiness("Smoothness", Range(0,1))=0.5
		_Metallic("Metallic", Range(0,1))=0.0

		_OutlineColor("OutlineColor", Color) = (1,1,1,1)
		_Outline("OutLine",Range(0,10))=0.0
	}

	SubShader{

			CGPROGRAM
			//ZWrite Off
			#pragma surface surf Lambert vertex:vert

			struct Input {
				float2 uv_MainTex;
			};
		struct appdata
		{
			float4 vertex:POSITION;
			float3 normal:NORMAL;
			float4 texcoord:TEXCOORD0;
			};

		float _Outline;
		float4 _OutlineColor;

		void vert(inout appdata v) {
			v.vertex.xyz += v.normal*_Outline;
		}

		sampler2D _MainTex;
		void surf(Input IN, inout SurfaceOutput o) {
			o.Emission = _OutlineColor.rgb;
		}
		ENDCG

		CGPROGRAM
			//Zwrite On
			#pragma surface surf Lambert

			struct Input {
				float2 uv_MainTex;
			};

			sampler2D _MainTex;
			void surf(Input IN, inout SurfaceOutput o) {
				o.Albedo = tex2D(_MainTex, IN.uv_MainTex).rgb;
			}
			ENDCG
	}
	FallBack "DIffuse"
}
