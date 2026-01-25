// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "SyntyStudios_PolygonCyberCity_Triplanar_01"
{
	Properties
	{
		_Albedo("Albedo", 2D) = "white" {}
		_Normal("Normal", 2D) = "bump" {}
		_Metallic("Metallic", Float) = 0
		_Smoothness("Smoothness", Float) = 0.2
		_Emissive("Emissive", 2D) = "black" {}
		_EmissionAmount("EmissionAmount", Range( 0 , 5)) = 1
		[Header(Layer 1)]_TriplanarTiling_1_X("TriplanarTiling_1_X", Float) = 1
		_TriplanarTiling_1_Y("TriplanarTiling_1_Y", Float) = 1
		[Toggle]_LinkTiling_1useXonly("LinkTiling_1 (use X only)", Float) = 1
		_TriplanarTex_1("TriplanarTex_1", 2D) = "white" {}
		_TriplanarAlpha_1("TriplanarAlpha_1", 2D) = "white" {}
		_DecalTransparency_1("DecalTransparency_1", Range( 0 , 1)) = 1
		_WallMask_1("WallMask_1", 2D) = "white" {}
		[Toggle]_GrungeDecalSwitch_1("GrungeDecalSwitch_1", Float) = 0
		[Header(Layer 2)]_TriplanarTiling_2_X("TriplanarTiling_2_X", Float) = 1
		_TriplanarTiling_2_Y("TriplanarTiling_2_Y", Float) = 1
		[Toggle]_LinkTiling_2useXonly("LinkTiling_2 (use X only)", Float) = 1
		_TriplanarTex_2("TriplanarTex_2", 2D) = "white" {}
		_TriplanarAlpha_2("TriplanarAlpha_2", 2D) = "white" {}
		_DecalTransparency_2("DecalTransparency_2", Range( 0 , 1)) = 1
		_WallMask_2("WallMask_2", 2D) = "white" {}
		[Toggle]_GrungeDecalSwitch_2("GrungeDecalSwitch_2", Float) = 0
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque"  "Queue" = "Geometry+0" "IsEmissive" = "true"  }
		Cull Back
		CGINCLUDE
		#include "UnityPBSLighting.cginc"
		#include "Lighting.cginc"
		#pragma target 3.0
		#ifdef UNITY_PASS_SHADOWCASTER
			#undef INTERNAL_DATA
			#undef WorldReflectionVector
			#undef WorldNormalVector
			#define INTERNAL_DATA half3 internalSurfaceTtoW0; half3 internalSurfaceTtoW1; half3 internalSurfaceTtoW2;
			#define WorldReflectionVector(data,normal) reflect (data.worldRefl, half3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal)))
			#define WorldNormalVector(data,normal) half3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal))
		#endif
		struct Input
		{
			float2 uv_texcoord;
			float3 worldPos;
			float3 worldNormal;
			INTERNAL_DATA
		};

		uniform sampler2D _Normal;
		uniform float4 _Normal_ST;
		uniform float _GrungeDecalSwitch_2;
		uniform float _GrungeDecalSwitch_1;
		uniform sampler2D _Albedo;
		uniform float4 _Albedo_ST;
		sampler2D _TriplanarTex_1;
		uniform float _TriplanarTiling_1_X;
		uniform float _LinkTiling_1useXonly;
		uniform float _TriplanarTiling_1_Y;
		uniform sampler2D _WallMask_1;
		uniform float4 _WallMask_1_ST;
		sampler2D _TriplanarAlpha_1;
		uniform float _DecalTransparency_1;
		sampler2D _TriplanarTex_2;
		uniform float _TriplanarTiling_2_X;
		uniform float _LinkTiling_2useXonly;
		uniform float _TriplanarTiling_2_Y;
		uniform sampler2D _WallMask_2;
		uniform float4 _WallMask_2_ST;
		sampler2D _TriplanarAlpha_2;
		uniform float _DecalTransparency_2;
		uniform sampler2D _Emissive;
		uniform float4 _Emissive_ST;
		uniform float _EmissionAmount;
		uniform float _Metallic;
		uniform float _Smoothness;


		inline float4 TriplanarSampling1( sampler2D topTexMap, float3 worldPos, float3 worldNormal, float falloff, float2 tiling, float3 normalScale, float3 index )
		{
			float3 projNormal = ( pow( abs( worldNormal ), falloff ) );
			projNormal /= ( projNormal.x + projNormal.y + projNormal.z ) + 0.00001;
			float3 nsign = sign( worldNormal );
			half4 xNorm; half4 yNorm; half4 zNorm;
			xNorm = tex2D( topTexMap, tiling * worldPos.zy * float2(  nsign.x, 1.0 ) );
			yNorm = tex2D( topTexMap, tiling * worldPos.xz * float2(  nsign.y, 1.0 ) );
			zNorm = tex2D( topTexMap, tiling * worldPos.xy * float2( -nsign.z, 1.0 ) );
			return xNorm * projNormal.x + yNorm * projNormal.y + zNorm * projNormal.z;
		}


		inline float4 TriplanarSampling42( sampler2D topTexMap, float3 worldPos, float3 worldNormal, float falloff, float2 tiling, float3 normalScale, float3 index )
		{
			float3 projNormal = ( pow( abs( worldNormal ), falloff ) );
			projNormal /= ( projNormal.x + projNormal.y + projNormal.z ) + 0.00001;
			float3 nsign = sign( worldNormal );
			half4 xNorm; half4 yNorm; half4 zNorm;
			xNorm = tex2D( topTexMap, tiling * worldPos.zy * float2(  nsign.x, 1.0 ) );
			yNorm = tex2D( topTexMap, tiling * worldPos.xz * float2(  nsign.y, 1.0 ) );
			zNorm = tex2D( topTexMap, tiling * worldPos.xy * float2( -nsign.z, 1.0 ) );
			return xNorm * projNormal.x + yNorm * projNormal.y + zNorm * projNormal.z;
		}


		inline float4 TriplanarSampling53( sampler2D topTexMap, float3 worldPos, float3 worldNormal, float falloff, float2 tiling, float3 normalScale, float3 index )
		{
			float3 projNormal = ( pow( abs( worldNormal ), falloff ) );
			projNormal /= ( projNormal.x + projNormal.y + projNormal.z ) + 0.00001;
			float3 nsign = sign( worldNormal );
			half4 xNorm; half4 yNorm; half4 zNorm;
			xNorm = tex2D( topTexMap, tiling * worldPos.zy * float2(  nsign.x, 1.0 ) );
			yNorm = tex2D( topTexMap, tiling * worldPos.xz * float2(  nsign.y, 1.0 ) );
			zNorm = tex2D( topTexMap, tiling * worldPos.xy * float2( -nsign.z, 1.0 ) );
			return xNorm * projNormal.x + yNorm * projNormal.y + zNorm * projNormal.z;
		}


		inline float4 TriplanarSampling49( sampler2D topTexMap, float3 worldPos, float3 worldNormal, float falloff, float2 tiling, float3 normalScale, float3 index )
		{
			float3 projNormal = ( pow( abs( worldNormal ), falloff ) );
			projNormal /= ( projNormal.x + projNormal.y + projNormal.z ) + 0.00001;
			float3 nsign = sign( worldNormal );
			half4 xNorm; half4 yNorm; half4 zNorm;
			xNorm = tex2D( topTexMap, tiling * worldPos.zy * float2(  nsign.x, 1.0 ) );
			yNorm = tex2D( topTexMap, tiling * worldPos.xz * float2(  nsign.y, 1.0 ) );
			zNorm = tex2D( topTexMap, tiling * worldPos.xy * float2( -nsign.z, 1.0 ) );
			return xNorm * projNormal.x + yNorm * projNormal.y + zNorm * projNormal.z;
		}


		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float2 uv_Normal = i.uv_texcoord * _Normal_ST.xy + _Normal_ST.zw;
			o.Normal = UnpackNormal( tex2D( _Normal, uv_Normal ) );
			float2 uv_Albedo = i.uv_texcoord * _Albedo_ST.xy + _Albedo_ST.zw;
			float4 tex2DNode3 = tex2D( _Albedo, uv_Albedo );
			float4 color15 = IsGammaSpace() ? float4(1,1,1,0) : float4(1,1,1,0);
			float2 appendResult11 = (float2(_TriplanarTiling_1_X , (( _LinkTiling_1useXonly )?( _TriplanarTiling_1_X ):( _TriplanarTiling_1_Y ))));
			float3 ase_worldPos = i.worldPos;
			float3 ase_worldNormal = WorldNormalVector( i, float3( 0, 0, 1 ) );
			float4 triplanar1 = TriplanarSampling1( _TriplanarTex_1, ase_worldPos, ase_worldNormal, 1.0, appendResult11, 1.0, 0 );
			float2 uv_WallMask_1 = i.uv_texcoord * _WallMask_1_ST.xy + _WallMask_1_ST.zw;
			float4 tex2DNode13 = tex2D( _WallMask_1, uv_WallMask_1 );
			float4 lerpResult14 = lerp( color15 , triplanar1 , tex2DNode13.r);
			float4 color44 = IsGammaSpace() ? float4(0,0,0,0) : float4(0,0,0,0);
			float4 triplanar42 = TriplanarSampling42( _TriplanarAlpha_1, ase_worldPos, ase_worldNormal, 1.0, appendResult11, 1.0, 0 );
			float4 lerpResult43 = lerp( color44 , triplanar42 , tex2DNode13.r);
			float4 lerpResult20 = lerp( tex2DNode3 , lerpResult14 , ( lerpResult43 * _DecalTransparency_1 ));
			float4 lerpResult32 = lerp( color15 , lerpResult14 , ( triplanar42 * _DecalTransparency_1 ));
			float4 color61 = IsGammaSpace() ? float4(1,1,1,0) : float4(1,1,1,0);
			float2 appendResult48 = (float2(_TriplanarTiling_2_X , (( _LinkTiling_2useXonly )?( _TriplanarTiling_2_X ):( _TriplanarTiling_2_Y ))));
			float4 triplanar53 = TriplanarSampling53( _TriplanarTex_2, ase_worldPos, ase_worldNormal, 1.0, appendResult48, 1.0, 0 );
			float2 uv_WallMask_2 = i.uv_texcoord * _WallMask_2_ST.xy + _WallMask_2_ST.zw;
			float4 tex2DNode51 = tex2D( _WallMask_2, uv_WallMask_2 );
			float4 lerpResult56 = lerp( color61 , triplanar53 , tex2DNode51.r);
			float4 color50 = IsGammaSpace() ? float4(0,0,0,0) : float4(0,0,0,0);
			float4 triplanar49 = TriplanarSampling49( _TriplanarAlpha_2, ase_worldPos, ase_worldNormal, 1.0, appendResult48, 1.0, 0 );
			float4 lerpResult54 = lerp( color50 , triplanar49 , tex2DNode51.r);
			float4 lerpResult58 = lerp( (( _GrungeDecalSwitch_1 )?( ( tex2DNode3 * lerpResult32 ) ):( lerpResult20 )) , lerpResult56 , ( lerpResult54 * _DecalTransparency_2 ));
			float4 lerpResult57 = lerp( color61 , lerpResult56 , ( triplanar49 * _DecalTransparency_2 ));
			o.Albedo = (( _GrungeDecalSwitch_2 )?( ( (( _GrungeDecalSwitch_1 )?( ( tex2DNode3 * lerpResult32 ) ):( lerpResult20 )) * lerpResult57 ) ):( lerpResult58 )).rgb;
			float2 uv_Emissive = i.uv_texcoord * _Emissive_ST.xy + _Emissive_ST.zw;
			o.Emission = ( tex2D( _Emissive, uv_Emissive ) * _EmissionAmount ).rgb;
			o.Metallic = _Metallic;
			o.Smoothness = _Smoothness;
			o.Alpha = 1;
		}

		ENDCG
		CGPROGRAM
		#pragma surface surf Standard keepalpha fullforwardshadows 

		ENDCG
		Pass
		{
			Name "ShadowCaster"
			Tags{ "LightMode" = "ShadowCaster" }
			ZWrite On
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 3.0
			#pragma multi_compile_shadowcaster
			#pragma multi_compile UNITY_PASS_SHADOWCASTER
			#pragma skip_variants FOG_LINEAR FOG_EXP FOG_EXP2
			#include "HLSLSupport.cginc"
			#if ( SHADER_API_D3D11 || SHADER_API_GLCORE || SHADER_API_GLES || SHADER_API_GLES3 || SHADER_API_METAL || SHADER_API_VULKAN )
				#define CAN_SKIP_VPOS
			#endif
			#include "UnityCG.cginc"
			#include "Lighting.cginc"
			#include "UnityPBSLighting.cginc"
			struct v2f
			{
				V2F_SHADOW_CASTER;
				float2 customPack1 : TEXCOORD1;
				float4 tSpace0 : TEXCOORD2;
				float4 tSpace1 : TEXCOORD3;
				float4 tSpace2 : TEXCOORD4;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};
			v2f vert( appdata_full v )
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID( v );
				UNITY_INITIALIZE_OUTPUT( v2f, o );
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO( o );
				UNITY_TRANSFER_INSTANCE_ID( v, o );
				Input customInputData;
				float3 worldPos = mul( unity_ObjectToWorld, v.vertex ).xyz;
				half3 worldNormal = UnityObjectToWorldNormal( v.normal );
				half3 worldTangent = UnityObjectToWorldDir( v.tangent.xyz );
				half tangentSign = v.tangent.w * unity_WorldTransformParams.w;
				half3 worldBinormal = cross( worldNormal, worldTangent ) * tangentSign;
				o.tSpace0 = float4( worldTangent.x, worldBinormal.x, worldNormal.x, worldPos.x );
				o.tSpace1 = float4( worldTangent.y, worldBinormal.y, worldNormal.y, worldPos.y );
				o.tSpace2 = float4( worldTangent.z, worldBinormal.z, worldNormal.z, worldPos.z );
				o.customPack1.xy = customInputData.uv_texcoord;
				o.customPack1.xy = v.texcoord;
				TRANSFER_SHADOW_CASTER_NORMALOFFSET( o )
				return o;
			}
			half4 frag( v2f IN
			#if !defined( CAN_SKIP_VPOS )
			, UNITY_VPOS_TYPE vpos : VPOS
			#endif
			) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID( IN );
				Input surfIN;
				UNITY_INITIALIZE_OUTPUT( Input, surfIN );
				surfIN.uv_texcoord = IN.customPack1.xy;
				float3 worldPos = float3( IN.tSpace0.w, IN.tSpace1.w, IN.tSpace2.w );
				half3 worldViewDir = normalize( UnityWorldSpaceViewDir( worldPos ) );
				surfIN.worldPos = worldPos;
				surfIN.worldNormal = float3( IN.tSpace0.z, IN.tSpace1.z, IN.tSpace2.z );
				surfIN.internalSurfaceTtoW0 = IN.tSpace0.xyz;
				surfIN.internalSurfaceTtoW1 = IN.tSpace1.xyz;
				surfIN.internalSurfaceTtoW2 = IN.tSpace2.xyz;
				SurfaceOutputStandard o;
				UNITY_INITIALIZE_OUTPUT( SurfaceOutputStandard, o )
				surf( surfIN, o );
				#if defined( CAN_SKIP_VPOS )
				float2 vpos = IN.pos;
				#endif
				SHADOW_CASTER_FRAGMENT( IN )
			}
			ENDCG
		}
	}
	Fallback "Diffuse"
	//CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=18909
-3424;3;2956;1369;3592.787;994.248;1.606366;True;True
Node;AmplifyShaderEditor.RangedFloatNode;9;-3783.954,407.3972;Inherit;False;Property;_TriplanarTiling_1_X;TriplanarTiling_1_X;6;1;[Header];Create;True;1;Layer 1;0;0;False;0;False;1;-0.2;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;10;-3782.954,540.3971;Inherit;False;Property;_TriplanarTiling_1_Y;TriplanarTiling_1_Y;7;0;Create;True;0;0;0;False;0;False;1;-0.36;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.ToggleSwitchNode;24;-3566.954,500.3974;Inherit;False;Property;_LinkTiling_1useXonly;LinkTiling_1 (use X only);8;0;Create;True;0;0;0;False;0;False;1;True;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;45;-3433.23,-600.1127;Inherit;False;Property;_TriplanarTiling_2_X;TriplanarTiling_2_X;14;1;[Header];Create;True;1;Layer 2;0;0;False;0;False;1;0.26;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;11;-3318.953,447.3971;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;46;-3432.23,-467.1125;Inherit;False;Property;_TriplanarTiling_2_Y;TriplanarTiling_2_Y;15;0;Create;True;0;0;0;False;0;False;1;-0.36;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;44;-3017.638,-160.5869;Inherit;False;Constant;_Color1;Color 1;12;0;Create;True;0;0;0;False;0;False;0,0,0,0;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TriplanarNode;1;-3138.66,392.62;Inherit;True;Spherical;World;False;TriplanarTex_1;_TriplanarTex_1;white;9;Assets/_Temp/Jason/triplanar_Grunge_01.png;Mid Texture 0;_MidTexture0;white;-1;None;Bot Texture 0;_BotTexture0;white;-1;None;Triplanar Sampler;Tangent;10;0;SAMPLER2D;;False;5;FLOAT;1;False;1;SAMPLER2D;;False;6;FLOAT;0;False;2;SAMPLER2D;;False;7;FLOAT;0;False;9;FLOAT3;0,0,0;False;8;FLOAT;1;False;3;FLOAT2;1,1;False;4;FLOAT;1;False;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ToggleSwitchNode;47;-3216.23,-507.1124;Inherit;False;Property;_LinkTiling_2useXonly;LinkTiling_2 (use X only);16;0;Create;True;0;0;0;False;0;False;1;True;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TriplanarNode;42;-3328.611,55.22384;Inherit;True;Spherical;World;False;TriplanarAlpha_1;_TriplanarAlpha_1;white;10;Assets/_Temp/Jason/triplanar_Grunge_01.png;Mid Texture 1;_MidTexture1;white;-1;None;Bot Texture 1;_BotTexture1;white;-1;None;Triplanar Sampler;Tangent;10;0;SAMPLER2D;;False;5;FLOAT;1;False;1;SAMPLER2D;;False;6;FLOAT;0;False;2;SAMPLER2D;;False;7;FLOAT;0;False;9;FLOAT3;0,0,0;False;8;FLOAT;1;False;3;FLOAT2;1,1;False;4;FLOAT;1;False;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;27;-2629.648,650.6108;Inherit;False;Property;_DecalTransparency_1;DecalTransparency_1;11;0;Create;True;0;0;0;False;0;False;1;0.709;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;15;-2934.234,206.5117;Inherit;False;Constant;_Color0;Color 0;8;0;Create;True;0;0;0;False;0;False;1,1,1,0;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;13;-3004.14,701.1534;Inherit;True;Property;_WallMask_1;WallMask_1;12;0;Create;True;0;0;0;False;0;False;-1;c8bc4546ad27b0244a3db2f011ba4f77;c8bc4546ad27b0244a3db2f011ba4f77;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.LerpOp;14;-2657.281,380.1237;Inherit;True;3;0;FLOAT4;0,0,0,0;False;1;FLOAT4;0,0,0,0;False;2;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.DynamicAppendNode;48;-2968.231,-560.1125;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.LerpOp;43;-2695.439,86.89668;Inherit;False;3;0;FLOAT4;0,0,0,0;False;1;FLOAT4;0,0,0,0;False;2;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;29;-2317.335,628.7189;Inherit;False;2;2;0;FLOAT4;0,0,0,0;False;1;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.TriplanarNode;49;-2968.044,-950.6451;Inherit;True;Spherical;World;False;TriplanarAlpha_2;_TriplanarAlpha_2;white;18;Assets/_Temp/Jason/triplanar_Grunge_01.png;Mid Texture 2;_MidTexture2;white;-1;None;Bot Texture 2;_BotTexture2;white;-1;None;Triplanar Sampler;Tangent;10;0;SAMPLER2D;;False;5;FLOAT;1;False;1;SAMPLER2D;;False;6;FLOAT;0;False;2;SAMPLER2D;;False;7;FLOAT;0;False;9;FLOAT3;0,0,0;False;8;FLOAT;1;False;3;FLOAT2;1,1;False;4;FLOAT;1;False;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;61;-2603.198,-784.5926;Inherit;False;Constant;_Color3;Color 3;8;0;Create;True;0;0;0;False;0;False;1,1,1,0;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;50;-2575.043,-1133.645;Inherit;False;Constant;_Color2;Color 2;12;0;Create;True;0;0;0;False;0;False;0,0,0,0;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.LerpOp;32;-2233.24,364.0069;Inherit;False;3;0;FLOAT4;0,0,0,0;False;1;FLOAT4;0,0,0,0;False;2;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.SamplerNode;51;-2707.552,-398.2271;Inherit;True;Property;_WallMask_2;WallMask_2;20;0;Create;True;0;0;0;False;0;False;-1;c8bc4546ad27b0244a3db2f011ba4f77;c8bc4546ad27b0244a3db2f011ba4f77;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TriplanarNode;53;-2787.937,-616.5303;Inherit;True;Spherical;World;False;TriplanarTex_2;_TriplanarTex_2;white;17;Assets/_Temp/Jason/triplanar_Grunge_01.png;Mid Texture 3;_MidTexture3;white;-1;None;Bot Texture 3;_BotTexture3;white;-1;None;Triplanar Sampler;Tangent;10;0;SAMPLER2D;;False;5;FLOAT;1;False;1;SAMPLER2D;;False;6;FLOAT;0;False;2;SAMPLER2D;;False;7;FLOAT;0;False;9;FLOAT3;0,0,0;False;8;FLOAT;1;False;3;FLOAT2;1,1;False;4;FLOAT;1;False;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;33;-2366.851,209.8468;Inherit;False;2;2;0;FLOAT4;0,0,0,0;False;1;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.RangedFloatNode;52;-2262.52,-342.1339;Inherit;False;Property;_DecalTransparency_2;DecalTransparency_2;19;0;Create;True;0;0;0;False;0;False;1;0.604;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;3;-2451.693,-64.1735;Inherit;True;Property;_Albedo;Albedo;0;0;Create;True;0;0;0;False;0;False;-1;f0ae4d94c0a8e1249bd5b47cac8b6b2e;f0ae4d94c0a8e1249bd5b47cac8b6b2e;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;6;-1998.362,347.1394;Inherit;True;2;2;0;COLOR;0,0,0,0;False;1;FLOAT4;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;54;-2366.044,-897.6452;Inherit;False;3;0;FLOAT4;0,0,0,0;False;1;FLOAT4;0,0,0,0;False;2;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.LerpOp;20;-2029.499,157.5246;Inherit;False;3;0;FLOAT4;0,0,0,0;False;1;FLOAT4;0,0,0,0;False;2;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.LerpOp;56;-2290.155,-612.6213;Inherit;True;3;0;FLOAT4;0,0,0,0;False;1;FLOAT4;0,0,0,0;False;2;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;55;-1950.207,-364.0258;Inherit;False;2;2;0;FLOAT4;0,0,0,0;False;1;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.LerpOp;57;-1866.112,-628.7381;Inherit;False;3;0;FLOAT4;0,0,0,0;False;1;FLOAT4;0,0,0,0;False;2;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;63;-1999.722,-782.898;Inherit;False;2;2;0;FLOAT4;0,0,0,0;False;1;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.ToggleSwitchNode;23;-1687.814,312.1394;Inherit;False;Property;_GrungeDecalSwitch_1;GrungeDecalSwitch_1;13;0;Create;True;0;0;0;False;0;False;0;True;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;58;-1067.652,-349.2052;Inherit;False;3;0;FLOAT4;0,0,0,0;False;1;FLOAT4;0,0,0,0;False;2;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;59;-1087.361,-123.6982;Inherit;True;2;2;0;COLOR;0,0,0,0;False;1;FLOAT4;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;66;-651.4473,432.1395;Inherit;False;Property;_EmissionAmount;EmissionAmount;5;0;Create;True;0;0;0;False;0;False;1;2;0;5;0;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;71;-672.4133,205.7074;Inherit;True;Property;_Emissive;Emissive;4;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;black;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ToggleSwitchNode;60;-794.7595,-215.5278;Inherit;False;Property;_GrungeDecalSwitch_2;GrungeDecalSwitch_2;21;0;Create;True;0;0;0;False;0;False;0;True;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SamplerNode;4;-642.7433,-50.47886;Inherit;True;Property;_Normal;Normal;1;0;Create;True;0;0;0;False;0;False;-1;fd88a15ea6ea4ab4e89e62157e3e74ff;fd88a15ea6ea4ab4e89e62157e3e74ff;True;0;True;bump;Auto;True;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;65;-209.5878,81.57029;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;8;-225.4999,293;Inherit;False;Property;_Smoothness;Smoothness;3;0;Create;True;0;0;0;False;0;False;0.2;0.2;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;7;-208.9998,207.7;Inherit;False;Property;_Metallic;Metallic;2;0;Create;True;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;0,0;Float;False;True;-1;2;ASEMaterialInspector;0;0;Standard;SyntyStudios_PolygonCyberCity_Triplanar_01;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Back;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Opaque;0.5;True;True;0;False;Opaque;;Geometry;All;16;all;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;True;0;0;False;-1;0;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;False;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;24;0;10;0
WireConnection;24;1;9;0
WireConnection;11;0;9;0
WireConnection;11;1;24;0
WireConnection;1;3;11;0
WireConnection;47;0;46;0
WireConnection;47;1;45;0
WireConnection;42;3;11;0
WireConnection;14;0;15;0
WireConnection;14;1;1;0
WireConnection;14;2;13;1
WireConnection;48;0;45;0
WireConnection;48;1;47;0
WireConnection;43;0;44;0
WireConnection;43;1;42;0
WireConnection;43;2;13;1
WireConnection;29;0;42;0
WireConnection;29;1;27;0
WireConnection;49;3;48;0
WireConnection;32;0;15;0
WireConnection;32;1;14;0
WireConnection;32;2;29;0
WireConnection;53;3;48;0
WireConnection;33;0;43;0
WireConnection;33;1;27;0
WireConnection;6;0;3;0
WireConnection;6;1;32;0
WireConnection;54;0;50;0
WireConnection;54;1;49;0
WireConnection;54;2;51;1
WireConnection;20;0;3;0
WireConnection;20;1;14;0
WireConnection;20;2;33;0
WireConnection;56;0;61;0
WireConnection;56;1;53;0
WireConnection;56;2;51;1
WireConnection;55;0;49;0
WireConnection;55;1;52;0
WireConnection;57;0;61;0
WireConnection;57;1;56;0
WireConnection;57;2;55;0
WireConnection;63;0;54;0
WireConnection;63;1;52;0
WireConnection;23;0;20;0
WireConnection;23;1;6;0
WireConnection;58;0;23;0
WireConnection;58;1;56;0
WireConnection;58;2;63;0
WireConnection;59;0;23;0
WireConnection;59;1;57;0
WireConnection;60;0;58;0
WireConnection;60;1;59;0
WireConnection;65;0;71;0
WireConnection;65;1;66;0
WireConnection;0;0;60;0
WireConnection;0;1;4;0
WireConnection;0;2;65;0
WireConnection;0;3;7;0
WireConnection;0;4;8;0
ASEEND*/
//CHKSM=707B66807060A3107D26BA87ADE629D8D14CD367