// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "SyntyStudios_SkinColourChange"
{
	Properties
	{
		_Colour_Skin("Colour_Skin", Color) = (0.2603808,0.2274353,0.3176471,0)
		_Spec_Smoothness("Spec_Smoothness", Range( 0 , 1)) = 0.2
		_Spec_Metallic("Spec_Metallic", Range( 0 , 1)) = 0
		_PolygonSciFiCyberCity_Texture("PolygonSciFiCyberCity_Texture", 2D) = "white" {}
		_Emissive("Emissive", Range( 0 , 5)) = 1
		_skin_Mask("skin_Mask", 2D) = "black" {}
		_Emissive_01("Emissive_01", 2D) = "white" {}
		_Colour_Multiply("Colour_Multiply", 2D) = "white" {}
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque"  "Queue" = "Geometry+0" "IsEmissive" = "true"  }
		Cull Back
		CGPROGRAM
		#pragma target 3.0
		#pragma surface surf Standard keepalpha addshadow fullforwardshadows 
		struct Input
		{
			float2 uv_texcoord;
		};

		uniform sampler2D _Colour_Multiply;
		uniform float4 _Colour_Multiply_ST;
		uniform sampler2D _PolygonSciFiCyberCity_Texture;
		uniform float4 _PolygonSciFiCyberCity_Texture_ST;
		uniform float4 _Colour_Skin;
		uniform sampler2D _skin_Mask;
		uniform float4 _skin_Mask_ST;
		uniform sampler2D _Emissive_01;
		uniform float4 _Emissive_01_ST;
		uniform float _Emissive;
		uniform float _Spec_Metallic;
		uniform float _Spec_Smoothness;

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float2 uv_Colour_Multiply = i.uv_texcoord * _Colour_Multiply_ST.xy + _Colour_Multiply_ST.zw;
			float2 uv_PolygonSciFiCyberCity_Texture = i.uv_texcoord * _PolygonSciFiCyberCity_Texture_ST.xy + _PolygonSciFiCyberCity_Texture_ST.zw;
			float2 uv_skin_Mask = i.uv_texcoord * _skin_Mask_ST.xy + _skin_Mask_ST.zw;
			float4 lerpResult12 = lerp( tex2D( _PolygonSciFiCyberCity_Texture, uv_PolygonSciFiCyberCity_Texture ) , _Colour_Skin , tex2D( _skin_Mask, uv_skin_Mask ));
			float4 blendOpSrc23 = tex2D( _Colour_Multiply, uv_Colour_Multiply );
			float4 blendOpDest23 = lerpResult12;
			o.Albedo = ( saturate( ( blendOpSrc23 * blendOpDest23 ) )).rgb;
			float2 uv_Emissive_01 = i.uv_texcoord * _Emissive_01_ST.xy + _Emissive_01_ST.zw;
			o.Emission = ( tex2D( _Emissive_01, uv_Emissive_01 ) * _Emissive ).rgb;
			o.Metallic = _Spec_Metallic;
			o.Smoothness = _Spec_Smoothness;
			o.Alpha = 1;
		}

		ENDCG
	}
	Fallback "Diffuse"
	//CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=18909
-3426;9;2956;1351;2443.424;495.5947;1;True;True
Node;AmplifyShaderEditor.SamplerNode;2;-1997.836,-223.3227;Inherit;True;Property;_PolygonSciFiCyberCity_Texture;PolygonSciFiCyberCity_Texture;3;0;Create;True;0;0;0;False;0;False;-1;18f698a95ab215742a622c214015079e;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;8;-1971.397,57.7821;Inherit;False;Property;_Colour_Skin;Colour_Skin;0;0;Create;True;0;0;0;False;0;False;0.2603808,0.2274353,0.3176471,0;0.4223075,0.4549831,0.7867647,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;9;-1927.033,277.424;Inherit;True;Property;_skin_Mask;skin_Mask;5;0;Create;True;0;0;0;False;0;False;-1;99bf8163a73647c4b9b93c57f58defca;None;True;0;False;black;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.LerpOp;12;-1601.253,-124.1857;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SamplerNode;14;-871.4436,91.04436;Inherit;True;Property;_Emissive_01;Emissive_01;6;0;Create;True;0;0;0;False;0;False;-1;31993c12d00def94f877434602256422;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;22;-1308.574,-330.8783;Inherit;True;Property;_Colour_Multiply;Colour_Multiply;7;0;Create;True;0;0;0;False;0;False;-1;ffb54e2f1915d9e478300bdd2cef8fba;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;25;-818.5961,295.0685;Inherit;False;Property;_Emissive;Emissive;4;0;Create;True;0;0;0;False;0;False;1;1;0;5;0;1;FLOAT;0
Node;AmplifyShaderEditor.BlendOpsNode;23;-924.9014,-169.6863;Inherit;False;Multiply;True;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;1;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;24;-397.5063,174.7221;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;20;-501.8286,401.6668;Inherit;False;Property;_Spec_Metallic;Spec_Metallic;2;0;Create;True;0;0;0;False;0;False;0;0.291;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;21;-500.0288,489.1456;Inherit;False;Property;_Spec_Smoothness;Spec_Smoothness;1;0;Create;True;0;0;0;False;0;False;0.2;0.15;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;0,0;Float;False;True;-1;2;ASEMaterialInspector;0;0;Standard;SyntyStudios_SkinColourChange;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Back;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Opaque;0.5;True;True;0;False;Opaque;;Geometry;All;16;all;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;True;0;0;False;-1;0;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;False;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;12;0;2;0
WireConnection;12;1;8;0
WireConnection;12;2;9;0
WireConnection;23;0;22;0
WireConnection;23;1;12;0
WireConnection;24;0;14;0
WireConnection;24;1;25;0
WireConnection;0;0;23;0
WireConnection;0;2;24;0
WireConnection;0;3;20;0
WireConnection;0;4;21;0
ASEEND*/
//CHKSM=01D14047239D60B4667609A7574182542301ACDE