Shader "Hidden/FillMember/ReactiveDiffusionDisplacement"
{
	Properties
	{

		_MainTex("Main Texture", 2D) = "" {}
		_rdTex("Reactive Diffusion Simulation Buffer", 2D) = "red" {}
		_workBuffer("Displaced Image Buffer", 2D) = "black" {}
		// _motionBuffer("Motion Vector Buffer", 2D) = "black" {}

	}

	CGINCLUDE

	#include "UnityCG.cginc"

	// Textures
	sampler2D _MainTex;
	float4 _MainTex_TexelSize;

	sampler2D _rdTex;
	float4 _rdTex_TexelSize;

	sampler2D _workBuffer;
	float4 _workBuffer_TexelSize;

	// Motion Vector
	sampler2D_half _CameraMotionVectorsTexture;
	float4 _CameraMotionVectorsTexture_TexelSize;

	// Settings
	float texelSize;
	float feedRate;
	float killRate;
	float displaceStrength;
	float dryWet;

	// Vertex Shader

	struct v2f {
		float4 pos : SV_POSITION;
		float2 uv : TEXCOORD0;
		float2 uv2 : TEXCOORD1;
	};

	v2f vert(appdata_full v) {

		v2f o;

		o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
		o.uv = v.texcoord.xy;
		o.uv2 = v.texcoord.xy;
		
	#if UNITY_UV_STARTS_AT_TOP
		if (_MainTex_TexelSize.y < 0.0) {
			o.uv.y = 1.0 - o.uv.y;
		}
	#endif

		return o;

	}

	// Fragment Shader

	float4 frag_rd(v2f source) : SV_Target {

		// Amount of Motion
		float2 mv = tex2D( _CameraMotionVectorsTexture , source.uv ).rg;
		mv = mv * _CameraMotionVectorsTexture_TexelSize.zw * displaceStrength;
		float motion = length( mv );

		// Simulation
		float2 stepX = float2( texelSize , 0 );
		float2 stepY = float2( 0 , texelSize );

		float2 v0 = tex2D( _MainTex , source.uv ).rg;
		float2 v1 = tex2D( _MainTex , source.uv - stepX ).rg;
		float2 v2 = tex2D( _MainTex , source.uv - stepY ).rg;
		float2 v3 = tex2D( _MainTex , source.uv + stepX ).rg;
		float2 v4 = tex2D( _MainTex , source.uv + stepY ).rg;

		float2 laplace = 0.25 * ( v1 + v2 + v3 + v4 ) - v0;

		float reaction = v0.r * v0.g * v0.g + lerp( 0 , 0.003 , motion );
		float du = 1.0 * laplace.r - reaction + feedRate * ( 1.0 - v0.r );
		float dv = 0.5 * laplace.g + reaction - ( feedRate + killRate ) * v0.g;

		float2 result = v0 + float2(du, dv) * 0.9;

		// mix motion

		float2 amv = tex2D( _MainTex , source.uv ).ba;
		amv = amv * 0.99 + mv * 0.01;

		return float4( result , amv );

	}

	float4 frag_init(v2f source) : SV_Target {

		return float4(0, 0, 0, 0);

	}

	float3 displace( in v2f source , in float intensity ) {
		
		float4 _rd = tex2D( _rdTex , source.uv2 );

		float2 newUV = source.uv2 + _rd.ba * _rd.g * intensity;

		float3 result = tex2D( _MainTex , newUV ).rgb;

		return result;

	}

	float4 frag_disp_distort(v2f source) : SV_Target {

		float3 _main_disp = displace( source , displaceStrength );

		return float4( _main_disp , 1.0 );

	}

	float4 frag_disp_full(v2f source) : SV_Target {

		float3 _main_disp = displace( source , 1 );

		// color blend

		float3 _main = tex2D( _MainTex , source.uv ).rgb;
		float2 _rd = tex2D( _rdTex , source.uv2 ).rg;
		float3 _work = tex2D( _workBuffer , source.uv2 ).rgb;

		float v = lerp( dryWet , 1 , _rd.g );
		
		float3 blend = lerp( _main , _work , v );

		float3 final = lerp( blend , _main_disp , _rd.g ).rgb;

		return float4( final , 1.0 );

	}

	ENDCG

	SubShader {

		Pass {
			ZTest Always Cull Off ZWrite Off Fog { Mode Off }
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag_init
			#pragma target 3.0
			ENDCG
		}

		Pass {
			ZTest Always Cull Off ZWrite Off Fog { Mode Off }
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag_rd
			#pragma target 3.0
			ENDCG
		}

		Pass {
			ZTest Always Cull Off ZWrite Off Fog { Mode Off }
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag_disp_distort
			#pragma target 3.0
			ENDCG
		}

		Pass {
			ZTest Always Cull Off ZWrite Off Fog { Mode Off }
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag_disp_full
			#pragma target 3.0
			ENDCG
		}

	}

}
