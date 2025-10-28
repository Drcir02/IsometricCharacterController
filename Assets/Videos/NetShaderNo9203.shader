Shader "Hidden/Aubergine/NetShaderNo9203" {
	Properties {
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_MaskCol ("Mask Color", Color)  = (1.0, 0.0, 0.0, 1.0)
	}

	SubShader {
		CGINCLUDE
			#include "UnityCG.cginc"
			
			half3 NormalizeColor (half3 color) {
				//return color / max(dot(color, half3(1.0f/3.0f)), 0.0001);
				return color / dot(color, fixed3(0.22, 0.707, 0.071));
			}
			
			half4 MaskColor (half3 mCol, half3 cCol) {
				half4 d = distance(NormalizeColor(mCol.rgb), NormalizeColor(cCol.rgb));
				return (d > 1f) ? half4(0.0) : half4(1.0);
			}
		ENDCG

		Pass {
			ZTest Always Cull Off ZWrite Off Lighting Off Fog { Mode off }
			
			CGPROGRAM
				#pragma vertex vert_img
				#pragma fragment frag
				#pragma fragmentoption ARB_precision_hint_fastest

				uniform sampler2D _MainTex;
				uniform float4 _MaskCol;

				half4 frag (v2f_img i) : COLOR {
					half4 col = tex2D(_MainTex, i.uv);
					half4 mask = MaskColor(col.rgb, _MaskCol.rgb);
					//return col * mask;
					return mask;
				}
			ENDCG
		}
	}
	Fallback off
}