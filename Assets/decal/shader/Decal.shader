Shader "BranchDemo/Decal"
{
	Properties
	{
		_OcclusionBias ("OcclusionBias", Range(0.01, 10)) = 2.0
		_BlendColor("BlendColor ",Color)= (1,1,1,0.2)
	}
	SubShader
	{ 
		Tags { "Queue" = "Transparent" "IgnoreProjector"="True" "RenderType"="Transparent"}
		LOD 200
		Pass
		{
			Blend SrcAlpha OneMinusSrcAlpha
            Cull Off
            ZWrite Off
            //ZTest always
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
            uniform sampler2D _CameraDepthTexture; //Depth Texture
            //camera coordinates
            uniform float4 	  CameraRight;
            uniform float4 	  CameraUp;
            uniform float4 	  CameraWorldPos;
            uniform float4 	  CameraNearLeftBottomCornerPoint;
            uniform float4 	  CameraNearSize;
            //------------------------------------

			struct appdata
			{
				float4 pos 	 : POSITION;
			};
			struct v2f
			{
				float4 pos 		: POSITION;
				float4 uv        : TEXCOORD0;
				float4 objPos	: TEXCOORD1;
			};
			#include "UnityCG.cginc"
			v2f vert (appdata v)
			{ 
				v2f o;
				UNITY_INITIALIZE_OUTPUT(v2f,o);
				o.pos = mul(UNITY_MATRIX_MVP, v.pos);
				o.uv =  ComputeScreenPos(o.pos);
				o.uv.z  =  -mul( UNITY_MATRIX_MV, v.pos ).z;
				o.objPos = v.pos;
				return o;
			}
			//viewz is simply the dist from the point to camera.
			inline float3 CalcWorldSpacePos(float2 depthUV,float viewZ)
			{
				//make the depth uv start at left botton
				//depthUV = (depthUV - 0.5) * float2(1,_ProjectionParams.x)+0.5;
				float2 offset = depthUV * CameraNearSize.xy;
				float3 nearPlanePos = CameraNearLeftBottomCornerPoint.xyz + offset.x * CameraRight.xyz + offset.y * CameraUp.xyz;
				float3 dirFromCamera = normalize(nearPlanePos - CameraWorldPos.xyz);
				return CameraWorldPos.xyz + dirFromCamera * viewZ * _ProjectionParams.z;
			}
			half4 frag (v2f i): COLOR
			{
				float2 depthUV = i.uv.xy/i.uv.w; 
				float  zFromDepthMap=Linear01Depth(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture,depthUV));
				half   zFromCurrentFrag = i.uv.z;		//just for debug
				float3 worldPos = CalcWorldSpacePos(depthUV,zFromDepthMap);
				float3 objPos = mul(_World2Object, float4(worldPos,1)).xyz;
				//objPos = i.objPos;
				clip (float3(0.5,0.5,0.5) - abs(objPos.xyz));
				//objPos =max(ceil(0.5 - abs(objPos)),float3(0,0,0));
				half4 col = 1;
				//col.a = objPos.x * objPos.y * objPos.z;
				//col.a=1;
				//col.rgb = objPos.y+0.5;
				col.rgb = objPos.y+0.5;

				return col;
			}
			ENDCG
		}
	}
	Fallback Off
}
