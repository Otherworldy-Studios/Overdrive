// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "SyntyStudios_Parallax_Full"
{
	Properties
	{
		[NoScaleOffset][SingleLineTexture]_Albedo("Albedo", 2D) = "white" {}
		[Toggle(_SHOWFACADE_ON)] _ShowFacade("ShowFacade", Float) = 0
		_FacadeTiling("Facade Tiling", Range( 0.5 , 20)) = 0
		_WallEmission("WallEmission", Range( 0 , 5)) = 1
		_WallGlowThreshold("WallGlowThreshold", Float) = 0.95
		[NoScaleOffset][SingleLineTexture]_Normals("Normals", 2D) = "bump" {}
		_NormalAmount("NormalAmount", Range( 0 , 5)) = 1
		_Metallic("Metallic", Range( 0 , 1)) = 0
		_Smoothness("Smoothness", Range( 0 , 1)) = 1
		_FacadeSmoothness("FacadeSmoothness", Range( 0 , 1)) = 0.2
		[Header(Rooms)][NoScaleOffset][SingleLineTexture]_WindowAlpha("WindowAlpha", 2D) = "white" {}
		_RoomEmission("RoomEmission", Range( 0 , 100)) = 1
		_RoomTint("RoomTint", Color) = (0,0,0,0)
		[NoScaleOffset][SingleLineTexture]_Floor("Floor", 2D) = "white" {}
		_FloorTexTiling("Floor Tex Tiling", Range( 0 , 100)) = 0
		[NoScaleOffset][SingleLineTexture]_Wall("Wall", 2D) = "white" {}
		_WalltexTiling("Wall tex Tiling", Range( 0 , 100)) = 0
		[Toggle(_TOGGLEPROPLAYER_ON)] _TogglePropLayer("Toggle Prop Layer", Float) = 0
		[NoScaleOffset][SingleLineTexture]_Props("Props", 2D) = "white" {}
		[Toggle(_SWITCHPLANE_ON)] _SwitchPlane("Switch Plane", Float) = 0
		_PropsTexTiling("Props Tex Tiling", Range( 0 , 100)) = 0
		[NoScaleOffset][SingleLineTexture]_Back("Back", 2D) = "white" {}
		_BackWallTexTiling("Back Wall Tex Tiling", Range( 0 , 100)) = 0
		[NoScaleOffset][SingleLineTexture]_Ceiling("Ceiling", 2D) = "white" {}
		_CeilingTexTiling("Ceiling Tex Tiling", Range( 0 , 100)) = 0
		_RoomTile("Room Tile", Range( 0.1 , 10)) = 0
		_RoomsXYZPropsW("Rooms X Y Z , Props W", Vector) = (1,1,1,1)
		_PositionOffsetXYZroomsWprops("Position Offset, XYZ = rooms, W = props", Vector) = (0,0,0,0)
		[Header(Layer 1)][NoScaleOffset][SingleLineTexture]_TriplanarTexture_1("TriplanarTexture_1", 2D) = "white" {}
		_TriplanarTiling_1_X("TriplanarTiling_1_X", Float) = 1
		_TriplanarTiling_1_Y("TriplanarTiling_1_Y", Float) = 1
		[Toggle]_LinkTiling_1useXonly("LinkTiling_1 (use X only)", Float) = 1
		_DecalTransparency_1("DecalTransparency_1", Range( 0 , 1)) = 1
		[Toggle]_GrungeDecalSwitch_1("GrungeDecalSwitch_1", Float) = 1
		[Header(Layer 2)][NoScaleOffset][SingleLineTexture]_TriplanarTexture_2("TriplanarTexture_2", 2D) = "white" {}
		_TriplanarTiling_2_X("TriplanarTiling_2_X", Float) = 1
		_TriplanarTiling_2_Y("TriplanarTiling_2_Y", Float) = 1
		[Toggle]_LinkTiling_2useXonly("LinkTiling_2 (use X only)", Float) = 1
		_DecalTransparency_2("DecalTransparency_2", Range( 0 , 1)) = 1
		[Toggle]_GrungeDecalSwitch_2("GrungeDecalSwitch_2", Float) = 1
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque"  "Queue" = "Geometry+0" "IsEmissive" = "true"  }
		Cull Back
		CGINCLUDE
		#include "UnityShaderVariables.cginc"
		#include "UnityPBSLighting.cginc"
		#include "Lighting.cginc"
		#pragma target 3.0
		#pragma shader_feature_local _SHOWFACADE_ON
		#pragma shader_feature_local _SWITCHPLANE_ON
		#pragma shader_feature_local _TOGGLEPROPLAYER_ON
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
			float3 vertexToFrag152;
			float3 worldPos;
			float3 worldNormal;
			INTERNAL_DATA
		};

		uniform sampler2D _Normals;
		uniform float _FacadeTiling;
		uniform float _NormalAmount;
		uniform float4 _RoomsXYZPropsW;
		uniform float _RoomTile;
		uniform float4 _PositionOffsetXYZroomsWprops;
		uniform sampler2D _Props;
		uniform float _PropsTexTiling;
		uniform sampler2D _Wall;
		uniform float _WalltexTiling;
		uniform sampler2D _Back;
		uniform float _BackWallTexTiling;
		uniform sampler2D _Floor;
		uniform float _FloorTexTiling;
		uniform sampler2D _Ceiling;
		uniform float _CeilingTexTiling;
		uniform float4 _RoomTint;
		uniform float _GrungeDecalSwitch_2;
		uniform float _GrungeDecalSwitch_1;
		uniform sampler2D _Albedo;
		uniform sampler2D _TriplanarTexture_1;
		uniform float _TriplanarTiling_1_X;
		uniform float _LinkTiling_1useXonly;
		uniform float _TriplanarTiling_1_Y;
		uniform sampler2D _WindowAlpha;
		uniform float _DecalTransparency_1;
		uniform sampler2D _TriplanarTexture_2;
		uniform float _TriplanarTiling_2_X;
		uniform float _LinkTiling_2useXonly;
		uniform float _TriplanarTiling_2_Y;
		uniform float _DecalTransparency_2;
		uniform float _WallGlowThreshold;
		uniform float _WallEmission;
		uniform float _RoomEmission;
		uniform float _Metallic;
		uniform float _FacadeSmoothness;
		uniform float _Smoothness;


		inline float4 TriplanarSampling191( sampler2D topTexMap, float3 worldPos, float3 worldNormal, float falloff, float2 tiling, float3 normalScale, float3 index )
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


		inline float4 TriplanarSampling200( sampler2D topTexMap, float3 worldPos, float3 worldNormal, float falloff, float2 tiling, float3 normalScale, float3 index )
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


		float3 RGBToHSV(float3 c)
		{
			float4 K = float4(0.0, -1.0 / 3.0, 2.0 / 3.0, -1.0);
			float4 p = lerp( float4( c.bg, K.wz ), float4( c.gb, K.xy ), step( c.b, c.g ) );
			float4 q = lerp( float4( p.xyw, c.r ), float4( c.r, p.yzx ), step( p.x, c.r ) );
			float d = q.x - min( q.w, q.y );
			float e = 1.0e-10;
			return float3( abs(q.z + (q.w - q.y) / (6.0 * d + e)), d / (q.x + e), q.x);
		}

		void vertexDataFunc( inout appdata_full v, out Input o )
		{
			UNITY_INITIALIZE_OUTPUT( Input, o );
			float3 ase_vertex3Pos = v.vertex.xyz;
			o.vertexToFrag152 = ase_vertex3Pos;
		}

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float2 uv1142 = ( i.uv_texcoord * _FacadeTiling );
			o.Normal = ( UnpackNormal( tex2D( _Normals, uv1142 ) ) * _NormalAmount );
			float4 temp_output_28_0 = ( ( _RoomsXYZPropsW + float4( -1E-05,-1E-05,-1E-05,-1E-05 ) ) * _RoomTile );
			#ifdef _SWITCHPLANE_ON
				float staticSwitch13 = (i.vertexToFrag152).z;
			#else
				float staticSwitch13 = (i.vertexToFrag152).x;
			#endif
			float4 appendResult17 = (float4(i.vertexToFrag152 , staticSwitch13));
			float4 InterpVertexPos21 = appendResult17;
			float4 temp_output_25_0 = ( InterpVertexPos21 - _PositionOffsetXYZroomsWprops );
			float4 appendResult4 = (float4(_WorldSpaceCameraPos , 1.0));
			float4 temp_output_154_0 = mul( unity_WorldToObject, appendResult4 );
			#ifdef _SWITCHPLANE_ON
				float staticSwitch9 = (temp_output_154_0).z;
			#else
				float staticSwitch9 = (temp_output_154_0).x;
			#endif
			float4 appendResult15 = (float4((temp_output_154_0).xyz , staticSwitch9));
			float4 TransCameraPos18 = appendResult15;
			float4 V226 = ( TransCameraPos18 - _PositionOffsetXYZroomsWprops );
			float4 V131 = ( temp_output_25_0 - V226 );
			float4 temp_output_39_0 = ( ( ( ( floor( ( temp_output_28_0 * temp_output_25_0 ) ) + step( float4( 0,0,0,0 ) , V131 ) ) / temp_output_28_0 ) - V226 ) / V131 );
			float Y82 = (temp_output_39_0).y;
			float newPlane43 = (temp_output_39_0).w;
			float Z63 = (temp_output_39_0).z;
			float4 temp_output_47_0 = ( ( newPlane43 * V131 ) + V226 );
			#ifdef _SWITCHPLANE_ON
				float2 staticSwitch51 = (temp_output_47_0).xy;
			#else
				float2 staticSwitch51 = (temp_output_47_0).yz;
			#endif
			float2 break53 = ( staticSwitch51 * _PropsTexTiling );
			float2 appendResult55 = (float2(break53.y , break53.x));
			float2 appendResult54 = (float2(break53.x , break53.y));
			#ifdef _SWITCHPLANE_ON
				float2 staticSwitch59 = appendResult54;
			#else
				float2 staticSwitch59 = appendResult55;
			#endif
			float4 tex2DNode62 = tex2Dbias( _Props, float4( staticSwitch59, 0, -1.0) );
			float4 break67 = tex2DNode62;
			float4 appendResult73 = (float4(break67.r , break67.g , break67.b , 0.0));
			#ifdef _TOGGLEPROPLAYER_ON
				float4 staticSwitch78 = tex2DNode62;
			#else
				float4 staticSwitch78 = appendResult73;
			#endif
			float4 PropsVar84 = staticSwitch78;
			float temp_output_108_0 = step( newPlane43 , ( Z63 * (PropsVar84).w ) );
			float ifLocalVar118 = 0;
			if( temp_output_108_0 <= 0.0 )
				ifLocalVar118 = Z63;
			else
				ifLocalVar118 = newPlane43;
			float X81 = (temp_output_39_0).x;
			float temp_output_123_0 = step( ifLocalVar118 , X81 );
			float ifLocalVar127 = 0;
			if( temp_output_123_0 <= 0.0 )
				ifLocalVar127 = X81;
			else
				ifLocalVar127 = ifLocalVar118;
			float2 break96 = ( (( ( Z63 * V131 ) + V226 )).xy * _WalltexTiling );
			float2 appendResult102 = (float2(break96.x , break96.y));
			float4 WallVar117 = tex2D( _Wall, appendResult102 );
			float4 ifLocalVar125 = 0;
			if( temp_output_108_0 <= 0.0 )
				ifLocalVar125 = WallVar117;
			else
				ifLocalVar125 = PropsVar84;
			float4 BackVar126 = tex2D( _Back, ( (( ( X81 * V131 ) + V226 )).zy * _BackWallTexTiling ) );
			float4 ifLocalVar131 = 0;
			if( temp_output_123_0 <= 0.0 )
				ifLocalVar131 = BackVar126;
			else
				ifLocalVar131 = ifLocalVar125;
			float2 temp_output_105_0 = (( ( Y82 * V131 ) + V226 )).xz;
			float Y_inverted111 = (V131).y;
			float4 lerpResult124 = lerp( tex2D( _Floor, ( temp_output_105_0 * _FloorTexTiling ) ) , tex2D( _Ceiling, ( temp_output_105_0 * _CeilingTexTiling ) ) , step( 0.0 , Y_inverted111 ));
			float4 CeilVar128 = lerpResult124;
			float4 ifLocalVar137 = 0;
			if( Y82 <= ifLocalVar127 )
				ifLocalVar137 = CeilVar128;
			else
				ifLocalVar137 = ifLocalVar131;
			float4 temp_output_166_0 = ( ifLocalVar137 + _RoomTint );
			float4 tex2DNode197 = tex2D( _Albedo, uv1142 );
			float4 color186 = IsGammaSpace() ? float4(1,1,1,0) : float4(1,1,1,0);
			float2 appendResult184 = (float2(_TriplanarTiling_1_X , (( _LinkTiling_1useXonly )?( _TriplanarTiling_1_X ):( _TriplanarTiling_1_Y ))));
			float3 ase_worldPos = i.worldPos;
			float3 ase_worldNormal = WorldNormalVector( i, float3( 0, 0, 1 ) );
			float4 triplanar191 = TriplanarSampling191( _TriplanarTexture_1, ase_worldPos, ase_worldNormal, 1.0, appendResult184, 1.0, 0 );
			float4 tex2DNode155 = tex2D( _WindowAlpha, uv1142 );
			float WindowMask223 = tex2DNode155.r;
			float4 lerpResult193 = lerp( color186 , triplanar191 , WindowMask223);
			float4 color192 = IsGammaSpace() ? float4(0,0,0,0) : float4(0,0,0,0);
			float4 temp_cast_12 = (triplanar191.w).xxxx;
			float4 lerpResult195 = lerp( color192 , temp_cast_12 , WindowMask223);
			float4 lerpResult208 = lerp( tex2DNode197 , lerpResult193 , ( lerpResult195 * _DecalTransparency_1 ));
			float4 lerpResult202 = lerp( color186 , lerpResult193 , ( triplanar191.w * _DecalTransparency_1 ));
			float4 color205 = IsGammaSpace() ? float4(1,1,1,0) : float4(1,1,1,0);
			float2 appendResult194 = (float2(_TriplanarTiling_2_X , (( _LinkTiling_2useXonly )?( _TriplanarTiling_2_X ):( _TriplanarTiling_2_Y ))));
			float4 triplanar200 = TriplanarSampling200( _TriplanarTexture_2, ase_worldPos, ase_worldNormal, 1.0, appendResult194, 1.0, 0 );
			float4 lerpResult209 = lerp( color205 , triplanar200 , WindowMask223);
			float4 color204 = IsGammaSpace() ? float4(0,0,0,0) : float4(0,0,0,0);
			float4 temp_cast_19 = (triplanar200.w).xxxx;
			float4 lerpResult207 = lerp( color204 , temp_cast_19 , WindowMask223);
			float4 lerpResult216 = lerp( (( _GrungeDecalSwitch_1 )?( ( tex2DNode197 * lerpResult202 ) ):( lerpResult208 )) , lerpResult209 , ( lerpResult207 * _DecalTransparency_2 ));
			float4 lerpResult215 = lerp( color205 , lerpResult209 , ( triplanar200.w * _DecalTransparency_2 ));
			float4 lerpResult149 = lerp( temp_output_166_0 , (( _GrungeDecalSwitch_2 )?( ( (( _GrungeDecalSwitch_1 )?( ( tex2DNode197 * lerpResult202 ) ):( lerpResult208 )) * lerpResult215 ) ):( lerpResult216 )) , tex2DNode155.r);
			#ifdef _SHOWFACADE_ON
				float4 staticSwitch151 = lerpResult149;
			#else
				float4 staticSwitch151 = temp_output_166_0;
			#endif
			o.Albedo = staticSwitch151.xyz;
			float3 hsvTorgb214 = RGBToHSV( tex2DNode197.rgb );
			float4 lerpResult157 = lerp( temp_output_166_0 , float4( 0,0,0,0 ) , tex2DNode155.r);
			o.Emission = ( ( ( hsvTorgb214.z > _WallGlowThreshold ? tex2DNode197 : float4( 0,0,0,0 ) ) * _WallEmission ) + ( lerpResult157 * _RoomEmission ) ).rgb;
			o.Metallic = _Metallic;
			o.Smoothness = ( saturate( ( tex2DNode155.b + _FacadeSmoothness ) ) * _Smoothness );
			o.Alpha = 1;
		}

		ENDCG
		CGPROGRAM
		#pragma surface surf Standard keepalpha fullforwardshadows vertex:vertexDataFunc 

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
				float3 customPack2 : TEXCOORD2;
				float4 tSpace0 : TEXCOORD3;
				float4 tSpace1 : TEXCOORD4;
				float4 tSpace2 : TEXCOORD5;
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
				vertexDataFunc( v, customInputData );
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
				o.customPack2.xyz = customInputData.vertexToFrag152;
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
				surfIN.vertexToFrag152 = IN.customPack2.xyz;
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
-3426;9;2956;1351;5554.018;2308.207;1;True;True
Node;AmplifyShaderEditor.CommentaryNode;1;-9680.334,1242.602;Inherit;False;1682.485;471.1538;Comment;11;154;153;18;15;10;9;8;7;4;3;2;;1,1,1,1;0;0
Node;AmplifyShaderEditor.RangedFloatNode;3;-9534.335,1539.494;Float;False;Constant;_Float0;Float 0;11;0;Create;True;0;0;0;False;0;False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.WorldSpaceCameraPos;2;-9630.334,1443.493;Inherit;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.DynamicAppendNode;4;-9326.336,1475.493;Inherit;False;FLOAT4;4;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.WorldToObjectMatrix;153;-9390.336,1347.493;Inherit;False;0;1;FLOAT4x4;0
Node;AmplifyShaderEditor.CommentaryNode;5;-9441.215,795.0357;Inherit;False;1411.799;332.554;Comment;7;152;21;17;13;12;11;6;;1,1,1,1;0;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;154;-9118.806,1318.315;Inherit;False;2;2;0;FLOAT4x4;0,0,0,0,0,1,0,0,0,0,1,0,0,0,0,1;False;1;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.PosVertexDataNode;6;-9391.215,845.0345;Inherit;False;0;0;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.VertexToFragmentNode;152;-9157.36,871.2286;Inherit;False;False;False;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.ComponentMaskNode;7;-8926.806,1414.316;Inherit;False;False;False;True;False;1;0;FLOAT4;0,0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ComponentMaskNode;8;-8926.806,1510.316;Inherit;False;True;False;False;False;1;0;FLOAT4;0,0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ComponentMaskNode;12;-8910.806,1030.315;Inherit;False;True;False;False;False;1;0;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ComponentMaskNode;11;-8910.806,950.3155;Inherit;False;False;False;True;False;1;0;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ComponentMaskNode;10;-8926.806,1318.315;Inherit;False;True;True;True;False;1;0;FLOAT4;0,0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.StaticSwitch;9;-8686.806,1446.316;Float;False;Property;_SwitchPlane;Switch Plane;18;0;Create;True;0;0;0;False;0;False;0;0;0;True;;Toggle;2;Key0;Key1;Create;True;True;9;1;FLOAT;0;False;0;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT;0;False;7;FLOAT;0;False;8;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.StaticSwitch;13;-8670.806,982.3155;Float;False;Property;_SwitchPlane;Switch Plane;19;0;Create;True;0;0;0;False;0;False;0;0;0;True;;Toggle;2;Key0;Key1;Create;True;True;9;1;FLOAT;0;False;0;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT;0;False;7;FLOAT;0;False;8;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;15;-8462.806,1318.315;Inherit;False;FLOAT4;4;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.DynamicAppendNode;17;-8462.806,870.3155;Inherit;False;FLOAT4;4;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;18;-8254.806,1318.315;Float;False;TransCameraPos;-1;True;1;0;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.CommentaryNode;14;-10289.37,1818.703;Inherit;False;2944.635;705.8434;Comment;32;111;99;95;82;81;69;68;63;57;43;40;39;38;37;36;35;34;33;32;31;30;29;28;27;26;25;24;23;22;20;19;16;;1,1,1,1;0;0
Node;AmplifyShaderEditor.Vector4Node;19;-10225.29,2216.778;Float;False;Property;_PositionOffsetXYZroomsWprops;Position Offset, XYZ = rooms, W = props;27;0;Create;True;0;0;0;False;0;False;0,0,0,0;0.001,0,0.001,0;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.GetLocalVarNode;16;-10097.29,2408.774;Inherit;False;18;TransCameraPos;1;0;OBJECT;;False;1;FLOAT4;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;21;-8254.806,886.3155;Float;False;InterpVertexPos;-1;True;1;0;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.GetLocalVarNode;23;-10081.29,2088.778;Inherit;False;21;InterpVertexPos;1;0;OBJECT;;False;1;FLOAT4;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;20;-9745.291,2296.776;Inherit;False;2;0;FLOAT4;0,0,0,0;False;1;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.Vector4Node;22;-10193.29,1880.777;Float;False;Property;_RoomsXYZPropsW;Rooms X Y Z , Props W;26;0;Create;True;0;0;0;False;0;False;1,1,1,1;0.0499,0.167,0.0499,1.12;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;24;-9889.291,1992.777;Float;False;Property;_RoomTile;Room Tile;25;0;Create;True;0;0;0;False;0;False;0;2;0.1;10;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;27;-9809.291,1880.777;Inherit;False;2;2;0;FLOAT4;0,0,0,0;False;1;FLOAT4;-1E-05,-1E-05,-1E-05,-1E-05;False;1;FLOAT4;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;25;-9745.291,2088.778;Inherit;False;2;0;FLOAT4;0,0,0,0;False;1;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;26;-9537.291,2248.778;Float;False;V2;-1;True;1;0;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;29;-9265.291,2168.778;Inherit;False;2;0;FLOAT4;0,0,0,0;False;1;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;28;-9553.291,1880.777;Inherit;False;2;2;0;FLOAT4;0,0,0,0;False;1;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;30;-9329.291,1992.777;Inherit;False;2;2;0;FLOAT4;0,0,0,0;False;1;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;31;-9089.291,2168.778;Float;False;V1;-1;True;1;0;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.FloorOpNode;33;-9041.291,1992.777;Inherit;False;1;0;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.StepOpNode;32;-8865.292,2088.778;Inherit;False;2;0;FLOAT4;0,0,0,0;False;1;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.SimpleAddOpNode;34;-8721.29,1992.777;Inherit;False;2;2;0;FLOAT4;0,0,0,0;False;1;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.GetLocalVarNode;35;-8577.291,2024.777;Inherit;False;26;V2;1;0;OBJECT;;False;1;FLOAT4;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;36;-8577.291,1864.777;Inherit;False;2;0;FLOAT4;0,0,0,0;False;1;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.GetLocalVarNode;38;-8337.292,2024.777;Inherit;False;31;V1;1;0;OBJECT;;False;1;FLOAT4;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;37;-8369.292,1864.777;Inherit;False;2;0;FLOAT4;0,0,0,0;False;1;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;39;-8158.747,1867.037;Inherit;False;2;0;FLOAT4;0,0,0,0;False;1;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.ComponentMaskNode;40;-7873.29,2200.778;Inherit;False;False;False;False;True;1;0;FLOAT4;0,0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;41;-6884.608,2421.577;Inherit;False;3419.267;463.6299;;21;84;78;73;67;66;62;59;58;55;54;53;52;51;50;49;48;47;46;45;44;42;Props;1,1,1,1;0;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;43;-7633.29,2200.778;Float;False;newPlane;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;44;-6813.402,2484.83;Inherit;False;43;newPlane;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;42;-6813.402,2596.83;Inherit;False;31;V1;1;0;OBJECT;;False;1;FLOAT4;0
Node;AmplifyShaderEditor.GetLocalVarNode;46;-6541.402,2596.83;Inherit;False;26;V2;1;0;OBJECT;;False;1;FLOAT4;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;45;-6541.402,2484.83;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.SimpleAddOpNode;47;-6301.402,2484.83;Inherit;False;2;2;0;FLOAT4;0,0,0,0;False;1;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.ComponentMaskNode;49;-6141.402,2484.83;Inherit;False;True;True;False;False;1;0;FLOAT4;0,0,0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.ComponentMaskNode;48;-6141.402,2564.83;Inherit;False;False;True;True;False;1;0;FLOAT4;0,0,0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.StaticSwitch;51;-5909.805,2511.169;Float;False;Property;_SwitchPlane;Switch Plane;17;0;Create;True;0;0;0;False;0;False;0;0;0;True;;Toggle;2;Key0;Key1;Create;True;True;9;1;FLOAT2;0,0;False;0;FLOAT2;0,0;False;2;FLOAT2;0,0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT2;0,0;False;6;FLOAT2;0,0;False;7;FLOAT2;0,0;False;8;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;50;-5988.27,2675.908;Float;False;Property;_PropsTexTiling;Props Tex Tiling;20;0;Create;True;0;0;0;False;0;False;0;2;0;100;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;52;-5677.401,2628.83;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.BreakToComponentsNode;53;-5534.944,2504.496;Inherit;False;FLOAT2;1;0;FLOAT2;0,0;False;16;FLOAT;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT;5;FLOAT;6;FLOAT;7;FLOAT;8;FLOAT;9;FLOAT;10;FLOAT;11;FLOAT;12;FLOAT;13;FLOAT;14;FLOAT;15
Node;AmplifyShaderEditor.DynamicAppendNode;54;-5264.687,2478.624;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.DynamicAppendNode;55;-5267.191,2585.797;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.ComponentMaskNode;57;-7873.29,2088.778;Inherit;False;False;False;True;False;1;0;FLOAT4;0,0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;63;-7633.29,2088.778;Float;False;Z;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;56;-6891.768,1854.373;Inherit;False;2337.914;375.4672;;12;117;106;102;96;89;77;76;71;65;64;61;60;Walls;1,1,1,1;0;0
Node;AmplifyShaderEditor.StaticSwitch;59;-5061.643,2489.82;Float;False;Property;_SwitchPlane;Switch Plane;20;0;Create;True;0;0;0;False;0;False;0;0;0;True;;Toggle;2;Key0;Key1;Create;True;True;9;1;FLOAT2;0,0;False;0;FLOAT2;0,0;False;2;FLOAT2;0,0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT2;0,0;False;6;FLOAT2;0,0;False;7;FLOAT2;0,0;False;8;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;58;-4944.567,2700.405;Float;False;Constant;_Float1;Float 1;26;0;Create;True;0;0;0;False;0;False;-1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;60;-6863.625,1939.009;Inherit;False;63;Z;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;61;-6867.358,2057.593;Inherit;False;31;V1;1;0;OBJECT;;False;1;FLOAT4;0
Node;AmplifyShaderEditor.SamplerNode;62;-4789.143,2507.781;Inherit;True;Property;_Props;Props;19;2;[NoScaleOffset];[SingleLineTexture];Create;True;0;0;0;False;0;False;-1;None;43051e0d1c99bfd43b0bce1baae61e8e;True;0;False;white;Auto;False;Object;-1;MipBias;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.BreakToComponentsNode;67;-4461.903,2596.348;Inherit;False;COLOR;1;0;COLOR;0,0,0,0;False;16;FLOAT;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT;5;FLOAT;6;FLOAT;7;FLOAT;8;FLOAT;9;FLOAT;10;FLOAT;11;FLOAT;12;FLOAT;13;FLOAT;14;FLOAT;15
Node;AmplifyShaderEditor.ComponentMaskNode;69;-7873.29,1976.777;Inherit;False;False;True;False;False;1;0;FLOAT4;0,0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;64;-6611.853,2051.993;Inherit;False;26;V2;1;0;OBJECT;;False;1;FLOAT4;0
Node;AmplifyShaderEditor.RangedFloatNode;66;-4428.553,2754.33;Float;False;Constant;_Float3;Float 3;21;0;Create;True;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.ComponentMaskNode;68;-7873.29,1880.777;Inherit;False;True;False;False;False;1;0;FLOAT4;0,0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;65;-6585.669,1925.772;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.CommentaryNode;70;-6873.008,332.5044;Inherit;False;1975.845;668.0294;;16;128;124;120;115;113;112;110;107;105;103;100;92;87;86;83;80;Floor Ceiling;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;72;-6891.537,1177.634;Inherit;False;1809.348;420.048;;10;126;119;109;101;98;91;90;85;75;74;Back Wall;1,1,1,1;0;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;82;-7633.29,1976.777;Float;False;Y;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;81;-7633.29,1880.777;Float;False;X;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;71;-6367.669,1961.573;Inherit;False;2;2;0;FLOAT4;0,0,0,0;False;1;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.DynamicAppendNode;73;-4168.765,2619.168;Inherit;False;FLOAT4;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.CommentaryNode;122;-3360.565,-231.0579;Inherit;False;1149.9;438.2001;Facade;5;142;136;132;129;155;Facade Texture;1,1,1,1;0;0
Node;AmplifyShaderEditor.RangedFloatNode;129;-3225.236,29.42742;Float;False;Property;_FacadeTiling;Facade Tiling;2;0;Create;True;0;0;0;False;0;False;0;1;0.5;20;0;1;FLOAT;0
Node;AmplifyShaderEditor.StaticSwitch;78;-3973.919,2489.274;Float;False;Property;_TogglePropLayer;Toggle Prop Layer;17;0;Create;True;0;0;0;False;0;False;0;0;0;True;;Toggle;2;Key0;Key1;Create;True;True;9;1;FLOAT4;0,0,0,0;False;0;FLOAT4;0,0,0,0;False;2;FLOAT4;0,0,0,0;False;3;FLOAT4;0,0,0,0;False;4;FLOAT4;0,0,0,0;False;5;FLOAT4;0,0,0,0;False;6;FLOAT4;0,0,0,0;False;7;FLOAT4;0,0,0,0;False;8;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.GetLocalVarNode;83;-6841.008,396.5044;Inherit;False;82;Y;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;80;-6841.008,492.5044;Inherit;False;31;V1;1;0;OBJECT;;False;1;FLOAT4;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;132;-3168.589,-153.565;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.GetLocalVarNode;75;-6857.008,1356.504;Inherit;False;31;V1;1;0;OBJECT;;False;1;FLOAT4;0
Node;AmplifyShaderEditor.ComponentMaskNode;77;-6196.568,1947.972;Inherit;False;True;True;False;False;1;0;FLOAT4;0,0,0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.GetLocalVarNode;74;-6857.008,1244.504;Inherit;False;81;X;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;76;-6268.65,2117.744;Float;False;Property;_WalltexTiling;Wall tex Tiling;16;0;Create;True;0;0;0;False;0;False;0;0.334;0;100;0;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;85;-6587.957,1363.051;Inherit;False;26;V2;1;0;OBJECT;;False;1;FLOAT4;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;89;-5949.868,2010.871;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.GetLocalVarNode;86;-6569.008,508.5044;Inherit;False;26;V2;1;0;OBJECT;;False;1;FLOAT4;0
Node;AmplifyShaderEditor.CommentaryNode;79;-4160.197,307.5961;Inherit;False;1936.548;573.6697;;17;137;133;131;130;127;125;123;121;118;116;114;108;104;97;94;93;88;Mix;0,0.7426471,0.6100315,1;0;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;136;-2918.021,-83.76395;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;87;-6569.008,396.5044;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;84;-3706.515,2489.169;Float;False;PropsVar;-1;True;1;0;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;90;-6562.437,1234.833;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.GetLocalVarNode;95;-8113.29,2312.775;Inherit;False;31;V1;1;0;OBJECT;;False;1;FLOAT4;0
Node;AmplifyShaderEditor.BreakToComponentsNode;96;-5783.354,1992.03;Inherit;False;FLOAT2;1;0;FLOAT2;0,0;False;16;FLOAT;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT;5;FLOAT;6;FLOAT;7;FLOAT;8;FLOAT;9;FLOAT;10;FLOAT;11;FLOAT;12;FLOAT;13;FLOAT;14;FLOAT;15
Node;AmplifyShaderEditor.SimpleAddOpNode;91;-6361.008,1244.504;Inherit;False;2;2;0;FLOAT4;0,0,0,0;False;1;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.RangedFloatNode;180;-4834.876,-954.4898;Inherit;False;Property;_TriplanarTiling_1_X;TriplanarTiling_1_X;29;0;Create;True;1;Layer 1;0;0;False;0;False;1;0.07;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;88;-4101.649,725.5958;Inherit;False;84;PropsVar;1;0;OBJECT;;False;1;FLOAT4;0
Node;AmplifyShaderEditor.ComponentMaskNode;99;-7877.817,2314.837;Inherit;False;False;True;False;False;1;0;FLOAT4;0,0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;142;-2751.002,-66.76592;Float;False;uv1;-1;True;1;0;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleAddOpNode;92;-6361.008,396.5044;Inherit;False;2;2;0;FLOAT4;0,0,0,0;False;1;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.RangedFloatNode;181;-4834.876,-810.4903;Inherit;False;Property;_TriplanarTiling_1_Y;TriplanarTiling_1_Y;30;0;Create;True;0;0;0;False;0;False;1;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SwizzleNode;101;-6187.49,1242.789;Inherit;False;FLOAT2;2;1;2;3;1;0;FLOAT4;0,0,0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.ComponentMaskNode;93;-3893.649,629.5958;Inherit;False;False;False;False;True;1;0;FLOAT4;0,0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;102;-5433.894,1981.329;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.ToggleSwitchNode;182;-4626.876,-858.4902;Inherit;False;Property;_LinkTiling_1useXonly;LinkTiling_1 (use X only);31;0;Create;True;0;0;0;False;0;False;1;True;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;94;-4101.649,485.5957;Inherit;False;63;Z;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;103;-6249.008,700.5043;Float;False;Property;_CeilingTexTiling;Ceiling Tex Tiling;24;0;Create;True;0;0;0;False;0;False;0;0.3;0;100;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;98;-6293.308,1467.177;Float;False;Property;_BackWallTexTiling;Back Wall Tex Tiling;22;0;Create;True;0;0;0;False;0;False;0;0.334;0;100;0;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;155;-2492.377,-90.15101;Inherit;True;Property;_WindowAlpha;WindowAlpha;10;3;[Header];[NoScaleOffset];[SingleLineTexture];Create;True;1;Rooms;0;0;False;0;False;-1;a451e81424298fc4b87bdb35a0592adb;fbb7ec839ea98d54398069dcf788bc77;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;100;-6249.008,524.5044;Float;False;Property;_FloorTexTiling;Floor Tex Tiling;14;0;Create;True;0;0;0;False;0;False;0;0.3;0;100;0;1;FLOAT;0
Node;AmplifyShaderEditor.ComponentMaskNode;105;-6169.008,396.5044;Inherit;False;True;False;True;False;1;0;FLOAT4;0,0,0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;111;-7633.29,2312.775;Float;False;Y_inverted;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;184;-4370.876,-906.4899;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;183;-4498.876,-1962.49;Inherit;False;Property;_TriplanarTiling_2_X;TriplanarTiling_2_X;35;0;Create;True;1;Layer 2;0;0;False;0;False;1;0.19;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.TexturePropertyNode;229;-4485.954,-1130.096;Inherit;True;Property;_TriplanarTexture_1;TriplanarTexture_1;28;3;[Header];[NoScaleOffset];[SingleLineTexture];Create;True;1;Layer 1;0;0;False;0;False;None;981e0e23fe0c3384c931d0b547e49cc6;False;white;Auto;Texture2D;-1;0;2;SAMPLER2D;0;SAMPLERSTATE;1
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;107;-5881.007,588.5044;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.GetLocalVarNode;97;-4101.649,389.5959;Inherit;False;43;newPlane;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;109;-6004.234,1335.234;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.GetLocalVarNode;112;-5737.007,860.5043;Inherit;False;111;Y_inverted;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;185;-4498.876,-1818.49;Inherit;False;Property;_TriplanarTiling_2_Y;TriplanarTiling_2_Y;36;0;Create;True;0;0;0;False;0;False;1;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;106;-5277.916,1931.323;Inherit;True;Property;_Wall;Wall;15;2;[NoScaleOffset];[SingleLineTexture];Create;True;0;0;0;False;0;False;-1;None;abf4bc711532ce9418b431ecc541c6b5;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;110;-5881.007,396.5044;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;104;-3685.649,565.5956;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;223;-2143.264,-155.5588;Inherit;False;WindowMask;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.StepOpNode;108;-3509.649,533.5957;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;117;-4839.207,1922.812;Float;False;WallVar;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.TriplanarNode;191;-4194.876,-972.2134;Inherit;True;Spherical;World;False;Top Texture 0;_TopTexture0;white;28;Assets/_Temp/Jason/graphicTest_02.tga;Mid Texture 0;_MidTexture0;white;-1;None;Bot Texture 0;_BotTexture0;white;-1;None;Triplanar Sampler;Tangent;10;0;SAMPLER2D;;False;5;FLOAT;1;False;1;SAMPLER2D;;False;6;FLOAT;0;False;2;SAMPLER2D;;False;7;FLOAT;0;False;9;FLOAT3;0,0,0;False;8;FLOAT;1;False;3;FLOAT2;1,1;False;4;FLOAT;1;False;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.StepOpNode;120;-5497.007,844.5043;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;186;-4021.976,-1176.391;Inherit;False;Constant;_Color1;Color 1;8;0;Create;True;0;0;0;False;0;False;1,1,1,0;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;115;-5673.007,636.5044;Inherit;True;Property;_Ceiling;Ceiling;23;2;[NoScaleOffset];[SingleLineTexture];Create;True;0;0;0;False;0;False;-1;None;96db0ef7541e12947baea7ceea66d512;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;113;-5673.007,412.5044;Inherit;True;Property;_Floor;Floor;13;2;[NoScaleOffset];[SingleLineTexture];Create;True;0;0;0;False;0;False;-1;None;b198d2d8a34741b4dbf5cac7c356a5dc;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;119;-5656.835,1259.934;Inherit;True;Property;_Back;Back;21;2;[NoScaleOffset];[SingleLineTexture];Create;True;0;0;0;False;0;False;-1;None;abf4bc711532ce9418b431ecc541c6b5;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;187;-3682.875,-714.4903;Inherit;False;Property;_DecalTransparency_1;DecalTransparency_1;32;0;Create;True;0;0;0;False;0;False;1;0.081;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;225;-4038.302,-622.1827;Inherit;False;223;WindowMask;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.ToggleSwitchNode;189;-4274.876,-1866.49;Inherit;False;Property;_LinkTiling_2useXonly;LinkTiling_2 (use X only);37;0;Create;True;0;0;0;False;0;False;1;True;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;192;-4082.876,-1514.49;Inherit;False;Constant;_Color0;Color 0;12;0;Create;True;0;0;0;False;0;False;0,0,0,0;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.DynamicAppendNode;194;-4034.876,-1914.49;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.LerpOp;193;-3701.875,-984.7896;Inherit;True;3;0;FLOAT4;0,0,0,0;False;1;FLOAT4;0,0,0,0;False;2;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.ConditionalIfNode;118;-3349.649,357.5961;Inherit;False;False;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;4;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TexturePropertyNode;228;-4243.802,-2240.786;Inherit;True;Property;_TriplanarTexture_2;TriplanarTexture_2;34;3;[Header];[NoScaleOffset];[SingleLineTexture];Create;True;1;Layer 2;0;0;False;0;False;None;62031f7df88a7ac4db5aa28e74ff1e35;False;white;Auto;Texture2D;-1;0;2;SAMPLER2D;0;SAMPLERSTATE;1
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;196;-3378.875,-730.4903;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;114;-3349.649,549.5956;Inherit;False;81;X;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;124;-5321.007,508.5044;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;195;-3718.274,-1283.59;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;116;-3637.649,789.5959;Inherit;False;117;WallVar;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;126;-5337.007,1276.504;Float;False;BackVar;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;224;-3599.641,-1706.485;Inherit;False;223;WindowMask;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.ConditionalIfNode;125;-3349.649,677.5956;Inherit;False;False;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT4;0,0,0,0;False;3;COLOR;0,0,0,0;False;4;COLOR;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;199;-3426.875,-1146.49;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;128;-5129.007,524.5044;Float;False;CeilVar;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;198;-3168.875,-1718.49;Inherit;False;Property;_DecalTransparency_2;DecalTransparency_2;38;0;Create;True;0;0;0;False;0;False;1;1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.TriplanarNode;200;-3851.493,-1956.081;Inherit;True;Spherical;World;False;Top Texture 1;_TopTexture1;white;34;Assets/_Temp/Jason/triplanar_Grunge_01.png;Mid Texture 3;_MidTexture3;white;-1;None;Bot Texture 3;_BotTexture3;white;-1;None;Triplanar Sampler;Tangent;10;0;SAMPLER2D;;False;5;FLOAT;1;False;1;SAMPLER2D;;False;6;FLOAT;0;False;2;SAMPLER2D;;False;7;FLOAT;0;False;9;FLOAT3;0,0,0;False;8;FLOAT;1;False;3;FLOAT2;1,1;False;4;FLOAT;1;False;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.StepOpNode;123;-3061.648,597.5957;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;205;-3666.875,-2138.49;Inherit;False;Constant;_Color2;Color 2;8;0;Create;True;0;0;0;False;0;False;1,1,1,0;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;197;-3506.875,-1418.49;Inherit;True;Property;_Albedo;Albedo;0;2;[NoScaleOffset];[SingleLineTexture];Create;True;0;0;0;False;0;False;-1;f0ae4d94c0a8e1249bd5b47cac8b6b2e;a28552de79d74f64398402b59a7befc8;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.GetLocalVarNode;121;-3109.648,773.5958;Inherit;False;126;BackVar;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;202;-3298.875,-986.4897;Inherit;False;3;0;FLOAT4;0,0,0,0;False;1;FLOAT4;0,0,0,0;False;2;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.ColorNode;204;-3634.875,-2490.49;Inherit;False;Constant;_Color3;Color 3;12;0;Create;True;0;0;0;False;0;False;0,0,0,0;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.GetLocalVarNode;130;-2645.648,757.5958;Inherit;False;128;CeilVar;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;209;-3123.875,-1960.49;Inherit;True;3;0;FLOAT4;0,0,0,0;False;1;FLOAT4;0,0,0,0;False;2;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;206;-3063.891,-1015.071;Inherit;True;2;2;0;COLOR;0,0,0,0;False;1;FLOAT4;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;207;-3203.875,-2248.49;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;210;-2864.875,-1734.49;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ConditionalIfNode;127;-2837.648,357.5961;Inherit;False;False;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;4;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;133;-2837.648,549.5956;Inherit;False;82;Y;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.ConditionalIfNode;131;-2837.648,677.5956;Inherit;False;False;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT4;0,0,0,0;False;3;COLOR;0,0,0,0;False;4;COLOR;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.LerpOp;208;-3043.079,-1182.953;Inherit;False;3;0;FLOAT4;0,0,0,0;False;1;FLOAT4;0,0,0,0;False;2;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.LerpOp;215;-2707.875,-1976.49;Inherit;False;3;0;FLOAT4;0,0,0,0;False;1;FLOAT4;0,0,0,0;False;2;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.ToggleSwitchNode;211;-2738.875,-1050.49;Inherit;False;Property;_GrungeDecalSwitch_1;GrungeDecalSwitch_1;33;0;Create;True;0;0;0;False;0;False;1;True;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;212;-2835.875,-2136.49;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.ColorNode;164;-2033.293,217.9527;Inherit;False;Property;_RoomTint;RoomTint;12;0;Create;True;0;0;0;False;0;False;0,0,0,0;0.319,0.2498225,0.07687899,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ConditionalIfNode;137;-2405.648,549.5956;Inherit;False;False;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT4;0,0,0,0;False;3;COLOR;0,0,0,0;False;4;COLOR;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.RangedFloatNode;176;-1166.123,421.3995;Inherit;False;Property;_FacadeSmoothness;FacadeSmoothness;9;0;Create;True;0;0;0;False;0;False;0.2;0.2;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RGBToHSVNode;214;-2402.875,-1274.49;Inherit;False;1;0;FLOAT3;0,0,0;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;218;-2146.875,-1482.49;Inherit;True;2;2;0;COLOR;0,0,0,0;False;1;FLOAT4;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleAddOpNode;166;-1749.359,251.7003;Inherit;False;2;2;0;FLOAT4;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.RangedFloatNode;213;-2354.875,-1050.49;Inherit;False;Property;_WallGlowThreshold;WallGlowThreshold;4;0;Create;True;0;0;0;False;0;False;0.95;0.95;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;216;-2130.875,-1706.49;Inherit;False;3;0;FLOAT4;0,0,0,0;False;1;FLOAT4;0,0,0,0;False;2;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.LerpOp;157;-1094.611,89.18552;Inherit;False;3;0;FLOAT4;0,0,0,0;False;1;FLOAT4;0,0,0,0;False;2;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.RangedFloatNode;219;-1666.876,-1018.49;Inherit;False;Property;_WallEmission;WallEmission;3;0;Create;True;0;0;0;False;0;False;1;0.22;0;5;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;135;-939.555,164.7102;Float;False;Property;_RoomEmission;RoomEmission;11;0;Create;True;0;0;0;False;0;False;1;3.1;0;100;0;1;FLOAT;0
Node;AmplifyShaderEditor.ToggleSwitchNode;220;-1858.876,-1578.49;Inherit;False;Property;_GrungeDecalSwitch_2;GrungeDecalSwitch_2;39;0;Create;True;0;0;0;False;0;False;1;True;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleAddOpNode;175;-868.4212,325.1996;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;173;-1019.677,-514.7686;Inherit;False;142;uv1;1;0;OBJECT;;False;1;FLOAT2;0
Node;AmplifyShaderEditor.Compare;217;-2018.876,-1114.49;Inherit;False;2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;COLOR;0,0,0,0;False;3;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;179;-816.4212,462.9996;Inherit;False;Property;_Smoothness;Smoothness;8;0;Create;True;0;0;0;False;0;False;1;1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;177;-731.9212,326.4995;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;171;-787.319,-342.5498;Inherit;False;Property;_NormalAmount;NormalAmount;6;0;Create;True;0;0;0;False;0;False;1;1;0;5;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;221;-1318.832,-1112.389;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;156;-625.4534,93.9501;Inherit;False;2;2;0;FLOAT4;0,0,0,0;False;1;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.SamplerNode;169;-806.8202,-533.6499;Inherit;True;Property;_Normals;Normals;5;2;[NoScaleOffset];[SingleLineTexture];Create;True;0;0;0;False;0;False;-1;49488522764f02748bae0cb0e5083004;4fc1fe31c90b5b145ac2170c85978ba3;True;0;True;bump;Auto;True;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.LerpOp;149;-1717.326,-138.7799;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;170;-399.9191,-416.6501;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode;174;-434.9593,140.9574;Inherit;False;Property;_Metallic;Metallic;7;0;Create;True;0;0;0;False;0;False;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.StaticSwitch;151;-1212.49,-143.2187;Float;False;Property;_ShowFacade;ShowFacade;1;0;Create;True;0;0;0;False;0;False;0;0;1;True;;Toggle;2;Key0;Key1;Create;True;True;9;1;FLOAT4;0,0,0,0;False;0;FLOAT4;0,0,0,0;False;2;FLOAT4;0,0,0,0;False;3;FLOAT4;0,0,0,0;False;4;FLOAT4;0,0,0,0;False;5;FLOAT4;0,0,0,0;False;6;FLOAT4;0,0,0,0;False;7;FLOAT4;0,0,0,0;False;8;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;178;-521.3214,309.5997;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;222;-397.9343,-99.85547;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT4;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;0,0;Float;False;True;-1;2;ASEMaterialInspector;0;0;Standard;SyntyStudios_Parallax_Full;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Back;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Opaque;0.5;True;True;0;False;Opaque;;Geometry;All;16;all;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;True;0;0;False;-1;0;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;False;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;4;0;2;0
WireConnection;4;3;3;0
WireConnection;154;0;153;0
WireConnection;154;1;4;0
WireConnection;152;0;6;0
WireConnection;7;0;154;0
WireConnection;8;0;154;0
WireConnection;12;0;152;0
WireConnection;11;0;152;0
WireConnection;10;0;154;0
WireConnection;9;1;8;0
WireConnection;9;0;7;0
WireConnection;13;1;12;0
WireConnection;13;0;11;0
WireConnection;15;0;10;0
WireConnection;15;3;9;0
WireConnection;17;0;152;0
WireConnection;17;3;13;0
WireConnection;18;0;15;0
WireConnection;21;0;17;0
WireConnection;20;0;16;0
WireConnection;20;1;19;0
WireConnection;27;0;22;0
WireConnection;25;0;23;0
WireConnection;25;1;19;0
WireConnection;26;0;20;0
WireConnection;29;0;25;0
WireConnection;29;1;26;0
WireConnection;28;0;27;0
WireConnection;28;1;24;0
WireConnection;30;0;28;0
WireConnection;30;1;25;0
WireConnection;31;0;29;0
WireConnection;33;0;30;0
WireConnection;32;1;31;0
WireConnection;34;0;33;0
WireConnection;34;1;32;0
WireConnection;36;0;34;0
WireConnection;36;1;28;0
WireConnection;37;0;36;0
WireConnection;37;1;35;0
WireConnection;39;0;37;0
WireConnection;39;1;38;0
WireConnection;40;0;39;0
WireConnection;43;0;40;0
WireConnection;45;0;44;0
WireConnection;45;1;42;0
WireConnection;47;0;45;0
WireConnection;47;1;46;0
WireConnection;49;0;47;0
WireConnection;48;0;47;0
WireConnection;51;1;48;0
WireConnection;51;0;49;0
WireConnection;52;0;51;0
WireConnection;52;1;50;0
WireConnection;53;0;52;0
WireConnection;54;0;53;0
WireConnection;54;1;53;1
WireConnection;55;0;53;1
WireConnection;55;1;53;0
WireConnection;57;0;39;0
WireConnection;63;0;57;0
WireConnection;59;1;55;0
WireConnection;59;0;54;0
WireConnection;62;1;59;0
WireConnection;62;2;58;0
WireConnection;67;0;62;0
WireConnection;69;0;39;0
WireConnection;68;0;39;0
WireConnection;65;0;60;0
WireConnection;65;1;61;0
WireConnection;82;0;69;0
WireConnection;81;0;68;0
WireConnection;71;0;65;0
WireConnection;71;1;64;0
WireConnection;73;0;67;0
WireConnection;73;1;67;1
WireConnection;73;2;67;2
WireConnection;73;3;66;0
WireConnection;78;1;73;0
WireConnection;78;0;62;0
WireConnection;77;0;71;0
WireConnection;89;0;77;0
WireConnection;89;1;76;0
WireConnection;136;0;132;0
WireConnection;136;1;129;0
WireConnection;87;0;83;0
WireConnection;87;1;80;0
WireConnection;84;0;78;0
WireConnection;90;0;74;0
WireConnection;90;1;75;0
WireConnection;96;0;89;0
WireConnection;91;0;90;0
WireConnection;91;1;85;0
WireConnection;99;0;95;0
WireConnection;142;0;136;0
WireConnection;92;0;87;0
WireConnection;92;1;86;0
WireConnection;101;0;91;0
WireConnection;93;0;88;0
WireConnection;102;0;96;0
WireConnection;102;1;96;1
WireConnection;182;0;181;0
WireConnection;182;1;180;0
WireConnection;155;1;142;0
WireConnection;105;0;92;0
WireConnection;111;0;99;0
WireConnection;184;0;180;0
WireConnection;184;1;182;0
WireConnection;107;0;105;0
WireConnection;107;1;103;0
WireConnection;109;0;101;0
WireConnection;109;1;98;0
WireConnection;106;1;102;0
WireConnection;110;0;105;0
WireConnection;110;1;100;0
WireConnection;104;0;94;0
WireConnection;104;1;93;0
WireConnection;223;0;155;1
WireConnection;108;0;97;0
WireConnection;108;1;104;0
WireConnection;117;0;106;0
WireConnection;191;0;229;0
WireConnection;191;3;184;0
WireConnection;120;1;112;0
WireConnection;115;1;107;0
WireConnection;113;1;110;0
WireConnection;119;1;109;0
WireConnection;189;0;185;0
WireConnection;189;1;183;0
WireConnection;194;0;183;0
WireConnection;194;1;189;0
WireConnection;193;0;186;0
WireConnection;193;1;191;0
WireConnection;193;2;225;0
WireConnection;118;0;108;0
WireConnection;118;2;97;0
WireConnection;118;3;94;0
WireConnection;118;4;94;0
WireConnection;196;0;191;4
WireConnection;196;1;187;0
WireConnection;124;0;113;0
WireConnection;124;1;115;0
WireConnection;124;2;120;0
WireConnection;195;0;192;0
WireConnection;195;1;191;4
WireConnection;195;2;225;0
WireConnection;126;0;119;0
WireConnection;125;0;108;0
WireConnection;125;2;88;0
WireConnection;125;3;116;0
WireConnection;125;4;116;0
WireConnection;199;0;195;0
WireConnection;199;1;187;0
WireConnection;128;0;124;0
WireConnection;200;0;228;0
WireConnection;200;3;194;0
WireConnection;123;0;118;0
WireConnection;123;1;114;0
WireConnection;197;1;142;0
WireConnection;202;0;186;0
WireConnection;202;1;193;0
WireConnection;202;2;196;0
WireConnection;209;0;205;0
WireConnection;209;1;200;0
WireConnection;209;2;224;0
WireConnection;206;0;197;0
WireConnection;206;1;202;0
WireConnection;207;0;204;0
WireConnection;207;1;200;4
WireConnection;207;2;224;0
WireConnection;210;0;200;4
WireConnection;210;1;198;0
WireConnection;127;0;123;0
WireConnection;127;2;118;0
WireConnection;127;3;114;0
WireConnection;127;4;114;0
WireConnection;131;0;123;0
WireConnection;131;2;125;0
WireConnection;131;3;121;0
WireConnection;131;4;121;0
WireConnection;208;0;197;0
WireConnection;208;1;193;0
WireConnection;208;2;199;0
WireConnection;215;0;205;0
WireConnection;215;1;209;0
WireConnection;215;2;210;0
WireConnection;211;0;208;0
WireConnection;211;1;206;0
WireConnection;212;0;207;0
WireConnection;212;1;198;0
WireConnection;137;0;133;0
WireConnection;137;1;127;0
WireConnection;137;2;131;0
WireConnection;137;3;130;0
WireConnection;137;4;130;0
WireConnection;214;0;197;0
WireConnection;218;0;211;0
WireConnection;218;1;215;0
WireConnection;166;0;137;0
WireConnection;166;1;164;0
WireConnection;216;0;211;0
WireConnection;216;1;209;0
WireConnection;216;2;212;0
WireConnection;157;0;166;0
WireConnection;157;2;155;1
WireConnection;220;0;216;0
WireConnection;220;1;218;0
WireConnection;175;0;155;3
WireConnection;175;1;176;0
WireConnection;217;0;214;3
WireConnection;217;1;213;0
WireConnection;217;2;197;0
WireConnection;177;0;175;0
WireConnection;221;0;217;0
WireConnection;221;1;219;0
WireConnection;156;0;157;0
WireConnection;156;1;135;0
WireConnection;169;1;173;0
WireConnection;149;0;166;0
WireConnection;149;1;220;0
WireConnection;149;2;155;1
WireConnection;170;0;169;0
WireConnection;170;1;171;0
WireConnection;151;1;166;0
WireConnection;151;0;149;0
WireConnection;178;0;177;0
WireConnection;178;1;179;0
WireConnection;222;0;221;0
WireConnection;222;1;156;0
WireConnection;0;0;151;0
WireConnection;0;1;170;0
WireConnection;0;2;222;0
WireConnection;0;3;174;0
WireConnection;0;4;178;0
ASEEND*/
//CHKSM=807C98A05F4FBAD12EAC14DFABA9EB8185A4FBEE