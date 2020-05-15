Shader "Custom/Billboard" 
{
	Properties
	{ 
		[PerRendererData]
		_MainTex("Color (RGB) Alpha (A)", 2D) = "white" {}
		_Color("Color (RGBA)", Color) = (1, 1, 1, 1) 
	}

	SubShader
	{
		Tags 
		{ 
			"Queue" = "Transparent"
			"DisableBatching" = "True"
			"IgnoreProjector" = "True"
			"RenderType" = "Transparent"
			"PreviewType" = "Plane"
			"CanUseSpriteAtlas" = "True"
		}

		Blend SrcAlpha OneMinusSrcAlpha
		Pass
		{

		CGPROGRAM
#pragma vertex vert
#pragma fragment frag   
#include "UnityCG.cginc"

			struct appdata_t
			{
				float4 vertex   : POSITION;
				float4 color    : COLOR;
				float2 texcoord : TEXCOORD0;
			};

			struct v2f
			{
				float4 pos		: SV_POSITION;
				float2 uv		: TEXCOORD0;
				float4 color	: COLOR;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
			float4 _Color;

			v2f vert(appdata_t v)
			{
				// The world position of the center of the object
				float3 worldPos = mul(unity_ObjectToWorld, float4(0, 0, 0, 1)).xyz;

				v2f o;
				//if (worldPos.z > 0)
				{
					o.pos = mul(UNITY_MATRIX_P,
						float4(UnityObjectToViewPos(float3(0.0, 0.0, 0.0)), 1.0) + float4(v.vertex.x, v.vertex.y, 0, 0));
				}
				//else
				//{
				//	o.pos = UnityObjectToClipPos(v.vertex);
				//}

				o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);
				o.color = v.color * _Color;
				return o;
			}

			fixed4 frag(v2f i) : SV_Target
			{
				fixed4 col = tex2D(_MainTex, i.uv) * i.color;
				return col;
			}

		ENDCG
		}
	}
}