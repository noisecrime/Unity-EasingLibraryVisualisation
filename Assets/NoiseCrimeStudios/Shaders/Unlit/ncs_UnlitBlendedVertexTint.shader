Shader "NoiseCrimeStudios/Unlit/Blended/VertexTint"
{
	// Description: AlphaBlended Vertex Colors tinted by Color

	Properties
	{
		_Color	("Main Color", Color) = (1,1,1,1)
	}

	SubShader
	{
		Tags
		{ 
			"Queue"				= "Transparent" 
			"IgnoreProjector"	= "True" 
			"RenderType"		= "Transparent"
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

			fixed4		_Color;
	
			struct appdata_t 
			{
				float4 vertex : POSITION;
				float4 color    : COLOR;

			};

			struct v2f
			{
				float4 vertex : SV_POSITION;
				fixed4 color : COLOR;
				UNITY_FOG_COORDS(1)
			};

			v2f vert(appdata_t v)
			{
				v2f o;
				o.vertex	= UnityObjectToClipPos(v.vertex);
				o.color		= v.color * _Color;
				UNITY_TRANSFER_FOG(o,o.vertex);
				return o;
			}

			fixed4 frag(v2f i) : SV_Target
			{
				fixed4 col = i.color;
				UNITY_APPLY_FOG(i.fogCoord, col);
				return col;
			}
			ENDCG
		}
	}

}
