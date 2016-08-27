Shader "Hidden/FillMember/ReactiveDiffusionDisplacement"
{
	Properties
	{

		_MainTex("", 2D) = ""{}

	}

    CGINCLUDE

    #include "UnityCG.cginc"

    sampler2D _MainTex;
    float4 _MainTex_TexelSize;

    sampler2D rdTex;
	float4 rdTex_TexelSize;

    sampler2D originalTex;
	float4 originalTex_TexelSize;

	sampler2D accumulatedMotionVector;
	float4 accumulatedMotionVector_TexelSize;

	float texelSize;

	float feedRate;
	float killRate;

	float decayRate;

	// Motion Vector
	sampler2D_half _CameraMotionVectorsTexture;
	float4 _CameraMotionVectorsTexture_TexelSize;

	fixed4 frag_rd(v2f_img source) : SV_Target {

		float2 v0 = tex2D( rdTex , source.uv ).rg;
		half2 mv = tex2D( _CameraMotionVectorsTexture , source.uv ).rg;

		float laplaceFactor = lerp( 0.0 , 1.1 , length(mv) );
		float claplaceFactor = (1 - laplaceFactor) * 0.25;

		float2 laplace =
			+ laplaceFactor * tex2D( rdTex , source.uv + mv ).rg
			+ claplaceFactor * (
				tex2D( rdTex , source.uv + float2( -texelSize , 0.0 ) ).rg +
				tex2D( rdTex , source.uv + float2( 0.0 , -texelSize ) ).rg +
				tex2D( rdTex , source.uv + float2(  texelSize , 0.0 ) ).rg +
				tex2D( rdTex , source.uv + float2( 0.0 ,  texelSize ) ).rg
			)
			- v0;

		float reaction = v0.r * v0.g * v0.g;
		float du = 1.0 * laplace.r - reaction + feedRate * ( 1.0 - v0.r );
		float dv = 0.5 * laplace.g + reaction - ( feedRate + killRate ) * v0.g;

		float2 dst = v0 + float2(du, dv) * 0.9;
		dst.g += min(0.5,length(mv));

		return fixed4( dst , mv.rg );

	}

	half4 frag_init(v2f_img source) : SV_Target {
		return 0;
	}

	half4 frag_update(v2f_img source) : SV_Target {
		half2 mv = tex2D( _CameraMotionVectorsTexture , source.uv ).rg;
		half2 amv = tex2D( accumulatedMotionVector , source.uv ).rg;
		return half4( (amv + mv) * decayRate , 0.0 , 1.0 );
	}

	half4 frag_disp_full(v2f_img source) : SV_Target {

		float2 uv = source.uv;
		float4 simulation = tex2D( rdTex , uv );
		float4 acc = tex2D( accumulatedMotionVector , uv );

		float2 newUv = uv + (simulation.ba + acc.rg) * simulation.g;

		float4 main = tex2D( _MainTex , uv );
		float4 original = tex2D( originalTex , uv );
		float4 disp = tex2D( _MainTex , newUv );
		float4 work = lerp( main , original , 0.8 + simulation.g );

		return half4( lerp( work , disp , simulation.g ).rgb , 1.0 );

	}

	half4 frag_disp_distort(v2f_img source) : SV_Target {

		float2 uv = source.uv;
		float4 simulation = tex2D( rdTex , uv );
		float4 acc = tex2D( accumulatedMotionVector , uv );
		float4 disp = tex2D( _MainTex , uv + (simulation.ba + acc.rg) * simulation.g );

		return half4( disp.rgb , 1.0 );

	}

	ENDCG

	SubShader {

		// No culling or depth
		Cull Off ZWrite Off ZTest Always

		Pass {
			CGPROGRAM
			#pragma vertex vert_img
			#pragma fragment frag_init
			#pragma target 3.0
			ENDCG
		}

		Pass {
			CGPROGRAM
			#pragma vertex vert_img
			#pragma fragment frag_rd
			#pragma target 3.0
			ENDCG
		}

		Pass {
			CGPROGRAM
			#pragma vertex vert_img
			#pragma fragment frag_update
			#pragma target 3.0
			ENDCG
		}


		Pass {
			CGPROGRAM
			#pragma vertex vert_img
			#pragma fragment frag_disp_distort
			#pragma target 3.0
			ENDCG
		}

		Pass {
			CGPROGRAM
			#pragma vertex vert_img
			#pragma fragment frag_disp_full
			#pragma target 3.0
			ENDCG
		}

	}

}
