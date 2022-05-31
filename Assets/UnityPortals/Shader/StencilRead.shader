Shader "Custom/StencilRead"
{
    	Properties {
		_Color ("Tint", Color) = (0, 0, 0, 1)
		_MainTex ("Texture", 2D) = "white" {}
		_Smoothness ("Smoothness", Range(0, 1)) = 0
		_Metallic ("Metalness", Range(0, 1)) = 0
		[HDR] _Emission ("Emission", color) = (0,0,0)
		[IntRange] _StencilRef ("Stencil Reference Value", Range(0,255)) = 0
        [HDR]_CutoffColor("Cutoff Color", Color) = (1,0,0,0)
		sliceNormal("Normal", Vector) = (0,0,0,0)
        sliceCentre ("Centre", Vector) = (0,0,0,0)
        sliceOffsetDst("Offset", Float) = 0
		_NeedsCut ("Needs Cutting", Range(0, 1)) = 1
		[Enum(UnityEngine.Rendering.CullMode)] _Cull ("Cull", Float) = 0
	}
	SubShader {
		Tags{ "RenderType"="Opaque" "Queue"="Geometry+1"}
		Cull [_Cull]

        //stencil operation
		Stencil 
		{
			Ref [_StencilRef]
			Comp Equal
		}

		CGPROGRAM

        #pragma surface surf Standard addshadow
		#pragma target 3.0

		sampler2D _MainTex;
		fixed4 _Color;

		half _Smoothness;
		half _Metallic;
		half3 _Emission;
		float4 _CutoffColor;
		float _NeedsCut;

		// World space normal of slice, anything along this direction from centre will be invisible
        float3 sliceNormal;
        // World space centre of slice
        float3 sliceCentre;
        // Increasing makes more of the mesh visible, decreasing makes less of the mesh visible
        float sliceOffsetDst;

		struct Input {
			float2 uv_MainTex;
			float3 worldPos;
			float facing : VFACE;
		};

		void surf (Input i, inout SurfaceOutputStandard o) {
			float facing = 1.0;
			if (_NeedsCut > 0.0)
			{
				float3 adjustedCentre = sliceCentre + sliceNormal * sliceOffsetDst;
				float3 offsetToSliceCentre = adjustedCentre - i.worldPos;
				clip (dot(offsetToSliceCentre, sliceNormal));
				facing = i.facing * 0.5 + 0.5;
			}

			fixed4 col = tex2D(_MainTex, i.uv_MainTex) * _Color;
			o.Albedo = col.rgb * facing;
			o.Metallic = _Metallic * facing;
			o.Smoothness = _Smoothness * facing;
            o.Emission = lerp(_CutoffColor, _Emission, facing);
		}
		ENDCG
	}
	FallBack "Standard"
}
