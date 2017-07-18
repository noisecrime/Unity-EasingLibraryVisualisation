Shader "NoiseCrimeStudios/Unlit/Blended/VertexTextureTint"
{
	// Description: AlphaBlended Texture Tinted by Vertex Colors and Color

	Properties
	{
		_Color 		("Main Color", Color) 		= (1,1,1,1)
		_MainTex 	("Base (RGBA)", 2D) 		= "white" {}	
	}
		
	SubShader
	{
		Tags 
		{ 
			"Queue"				= "Transparent" 
			"RenderType"		= "Transparent"
			"IgnoreProjector"	= "True" 
			"PreviewType"		= "Plane"
			"DisableBatching"	= "False"
		}	
		
		LOD 100
		
		ZWrite Off
		Blend SrcAlpha OneMinusSrcAlpha
		
		Pass
		{
			CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag
				#pragma target 2.0
				#pragma multi_compile_fog

				#include "UnityCG.cginc"
				
				sampler2D 	_MainTex;
				fixed4 		_Color;
				
				struct appdata_t
				{
					float4 vertex   : POSITION;
					float4 color    : COLOR;
					float2 texcoord : TEXCOORD0;
				};

				struct v2f
				{
					float4 vertex   : SV_POSITION;
					fixed4 color    : COLOR;
					half2 texcoord  : TEXCOORD0;
					UNITY_FOG_COORDS(1)
				};
								

				v2f vert(appdata_t v)
				{
					v2f o;
					o.vertex	= UnityObjectToClipPos(v.vertex);
					o.texcoord	= v.texcoord;
					o.color		= v.color * _Color;
					UNITY_TRANSFER_FOG(o, o.vertex);
					return o;
				}
				

				fixed4 frag(v2f i) : SV_Target
				{
					fixed4 col = tex2D(_MainTex, i.texcoord) * i.color;
					UNITY_APPLY_FOG(i.fogCoord, col);
					return col;
				}
			ENDCG
		}
	}
}
