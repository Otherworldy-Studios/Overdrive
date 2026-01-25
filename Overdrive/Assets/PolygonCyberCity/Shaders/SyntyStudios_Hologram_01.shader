// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "SyntyStudios_Hologram_01"
{
	Properties
	{
		[Toggle]_Albedo_FlatColourSwitch("Albedo_FlatColourSwitch", Float) = 1
		_BaseColour("BaseColour", Color) = (0.2216981,0.7184748,1,0)
		[NoScaleOffset][SingleLineTexture]_AlbedoTexture("AlbedoTexture", 2D) = "white" {}
		_Opacity("Opacity", Range( 0 , 1)) = 0.8
		_Neon_Colour_01("Neon_Colour_01", Color) = (0.6965517,1,0,0)
		[Toggle]_EmissionMask_FlatColourSwitch("EmissionMask_FlatColourSwitch", Float) = 1
		[Toggle]_EmissionMask_TintSwitch("EmissionMask_TintSwitch", Float) = 0
		[NoScaleOffset][SingleLineTexture]_EmissionMask("EmissionMask", 2D) = "black" {}
		_Emission_Power("Emission_Power", Range( 0 , 10)) = 0
		[NoScaleOffset][SingleLineTexture]_HoloLines("HoloLines", 2D) = "white" {}
		_Scroll_Speed("Scroll_Speed", Range( 0 , 10)) = 0.1
		_Metallic("Metallic", Range( 0 , 1)) = 0
		_Smoothness("Smoothness", Range( 0 , 1)) = 0
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Transparent"  "Queue" = "Transparent+0" "IgnoreProjector" = "True" "IsEmissive" = "true"  }
		Cull Off
		CGINCLUDE
		#include "UnityShaderVariables.cginc"
		#include "UnityPBSLighting.cginc"
		#include "Lighting.cginc"
		#pragma target 3.0
		struct Input
		{
			float2 uv_texcoord;
			float3 worldPos;
		};

		uniform float _Albedo_FlatColourSwitch;
		uniform float4 _BaseColour;
		uniform sampler2D _AlbedoTexture;
		uniform float _EmissionMask_FlatColourSwitch;
		uniform float4 _Neon_Colour_01;
		uniform float _EmissionMask_TintSwitch;
		uniform sampler2D _EmissionMask;
		uniform sampler2D _HoloLines;
		uniform float _Scroll_Speed;
		uniform float _Emission_Power;
		uniform float _Metallic;
		uniform float _Smoothness;
		uniform float _Opacity;

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float2 uv_AlbedoTexture16 = i.uv_texcoord;
			float4 tex2DNode16 = tex2D( _AlbedoTexture, uv_AlbedoTexture16 );
			o.Albedo = (( _Albedo_FlatColourSwitch )?( tex2DNode16 ):( _BaseColour )).rgb;
			float2 uv_EmissionMask2 = i.uv_texcoord;
			float4 tex2DNode2 = tex2D( _EmissionMask, uv_EmissionMask2 );
			float4 color10 = IsGammaSpace() ? float4(0,0,0,0) : float4(0,0,0,0);
			float4 lerpResult12 = lerp( (( _EmissionMask_FlatColourSwitch )?( (( _EmissionMask_TintSwitch )?( ( _Neon_Colour_01 * tex2DNode2 ) ):( tex2DNode2 )) ):( _Neon_Colour_01 )) , color10 , 0.3);
			float2 temp_cast_1 = (_Scroll_Speed).xx;
			float3 ase_worldPos = i.worldPos;
			float2 temp_cast_2 = (ase_worldPos.y).xx;
			float2 panner9 = ( 1.0 * _Time.y * temp_cast_1 + temp_cast_2);
			float4 lerpResult14 = lerp( lerpResult12 , (( _EmissionMask_FlatColourSwitch )?( (( _EmissionMask_TintSwitch )?( ( _Neon_Colour_01 * tex2DNode2 ) ):( tex2DNode2 )) ):( _Neon_Colour_01 )) , tex2D( _HoloLines, panner9 ));
			o.Emission = ( lerpResult14 * _Emission_Power ).rgb;
			o.Metallic = _Metallic;
			o.Smoothness = _Smoothness;
			o.Alpha = ( tex2DNode16.a * _Opacity );
		}

		ENDCG
		CGPROGRAM
		#pragma surface surf Standard alpha:fade keepalpha fullforwardshadows 

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
			sampler3D _DitherMaskLOD;
			struct v2f
			{
				V2F_SHADOW_CASTER;
				float2 customPack1 : TEXCOORD1;
				float3 worldPos : TEXCOORD2;
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
				o.customPack1.xy = customInputData.uv_texcoord;
				o.customPack1.xy = v.texcoord;
				o.worldPos = worldPos;
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
				float3 worldPos = IN.worldPos;
				half3 worldViewDir = normalize( UnityWorldSpaceViewDir( worldPos ) );
				surfIN.worldPos = worldPos;
				SurfaceOutputStandard o;
				UNITY_INITIALIZE_OUTPUT( SurfaceOutputStandard, o )
				surf( surfIN, o );
				#if defined( CAN_SKIP_VPOS )
				float2 vpos = IN.pos;
				#endif
				half alphaRef = tex3D( _DitherMaskLOD, float3( vpos.xy * 0.25, o.Alpha * 0.9375 ) ).a;
				clip( alphaRef - 0.01 );
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
-3426;9;2956;1351;2954.335;948.5743;1.3;True;True
Node;AmplifyShaderEditor.SamplerNode;2;-2395.968,-287.7979;Inherit;True;Property;_EmissionMask;EmissionMask;7;2;[NoScaleOffset];[SingleLineTexture];Create;True;0;0;0;False;0;False;-1;31993c12d00def94f877434602256422;31993c12d00def94f877434602256422;True;0;False;black;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;11;-2208.16,-561.5784;Inherit;False;Property;_Neon_Colour_01;Neon_Colour_01;4;0;Create;True;0;0;0;False;0;False;0.6965517,1,0,0;0,0.5883774,1,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;24;-2043.391,-173.5644;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;7;-2021.695,302.8646;Inherit;False;Property;_Scroll_Speed;Scroll_Speed;10;0;Create;True;0;0;0;False;0;False;0.1;0.11;0;10;0;1;FLOAT;0
Node;AmplifyShaderEditor.WorldPosInputsNode;6;-2176.36,17.90878;Float;True;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.ToggleSwitchNode;23;-1848.391,-255.5644;Inherit;False;Property;_EmissionMask_TintSwitch;EmissionMask_TintSwitch;6;0;Create;True;0;0;0;False;0;False;0;True;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.ColorNode;10;-1751.87,-556.9861;Inherit;False;Constant;_Color0;Color 0;3;0;Create;True;0;0;0;False;0;False;0,0,0,0;0.6965517,1,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ToggleSwitchNode;20;-1561.548,-279.9929;Inherit;False;Property;_EmissionMask_FlatColourSwitch;EmissionMask_FlatColourSwitch;5;0;Create;True;0;0;0;False;0;False;1;True;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;8;-1518.405,-403.8411;Float;False;Constant;_AlphaValue;AlphaValue;8;0;Create;True;0;0;0;False;0;False;0.3;6.24;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.PannerNode;9;-1869.769,42.764;Inherit;False;3;0;FLOAT2;1,1;False;2;FLOAT2;0,1;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SamplerNode;13;-1644.464,20.95347;Inherit;True;Property;_HoloLines;HoloLines;9;2;[NoScaleOffset];[SingleLineTexture];Create;True;0;0;0;False;0;False;-1;None;c6f333e0336a6e54bad50e8d1ed26f02;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.LerpOp;12;-1292.897,-467.6203;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;14;-1123.648,-15.2723;Inherit;True;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.ColorNode;1;-916,-600.5;Inherit;False;Property;_BaseColour;BaseColour;1;0;Create;True;0;0;0;False;0;False;0.2216981,0.7184748,1,0;0,0.8282828,1,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;15;-866.9976,26.8797;Float;False;Property;_Emission_Power;Emission_Power;8;0;Create;True;0;0;0;False;0;False;0;10;0;10;0;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;16;-925.6416,-390.5948;Inherit;True;Property;_AlbedoTexture;AlbedoTexture;2;2;[NoScaleOffset];[SingleLineTexture];Create;True;0;0;0;False;0;False;-1;18f698a95ab215742a622c214015079e;18f698a95ab215742a622c214015079e;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;18;-510.9585,225.9816;Inherit;False;Property;_Opacity;Opacity;3;0;Create;True;0;0;0;False;0;False;0.8;0.147;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.ToggleSwitchNode;21;-552.5806,-419.6949;Inherit;False;Property;_Albedo_FlatColourSwitch;Albedo_FlatColourSwitch;0;0;Create;True;0;0;0;False;0;False;1;True;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;4;-511,130.5;Inherit;False;Property;_Smoothness;Smoothness;12;0;Create;True;0;0;0;False;0;False;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;17;-577.1661,-99.55439;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;10;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;3;-509,52.5;Inherit;False;Property;_Metallic;Metallic;11;0;Create;True;0;0;0;False;0;False;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;25;-160.6351,205.8256;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;0,0;Float;False;True;-1;2;ASEMaterialInspector;0;0;Standard;SyntyStudios_Hologram_01;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;False;False;False;False;False;False;Off;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Transparent;0.5;True;True;0;False;Transparent;;Transparent;All;16;all;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;True;2;5;False;-1;10;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;False;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;24;0;11;0
WireConnection;24;1;2;0
WireConnection;23;0;2;0
WireConnection;23;1;24;0
WireConnection;20;0;11;0
WireConnection;20;1;23;0
WireConnection;9;0;6;2
WireConnection;9;2;7;0
WireConnection;13;1;9;0
WireConnection;12;0;20;0
WireConnection;12;1;10;0
WireConnection;12;2;8;0
WireConnection;14;0;12;0
WireConnection;14;1;20;0
WireConnection;14;2;13;0
WireConnection;21;0;1;0
WireConnection;21;1;16;0
WireConnection;17;0;14;0
WireConnection;17;1;15;0
WireConnection;25;0;16;4
WireConnection;25;1;18;0
WireConnection;0;0;21;0
WireConnection;0;2;17;0
WireConnection;0;3;3;0
WireConnection;0;4;4;0
WireConnection;0;9;25;0
ASEEND*/
//CHKSM=5D49671E8F454B0414FA0A96FE63F99C0D60739F