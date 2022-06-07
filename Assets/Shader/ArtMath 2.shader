Shader "Unlit/ArtMath 2"
{
    Properties
    {
        [IntRange] _StencilRef ("Stencil Reference Value", Range(0,255)) = 0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Stencil 
		{
			Ref [_StencilRef]
			Comp Equal
		}

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float2 uv = (7.0 * i.vertex - _ScreenParams.xy*4.0) / min(_ScreenParams.x, _ScreenParams.y);
    
                for (float k = 2.0; k < 4.0; k++)
                {
                    uv.x += 0.4 / k * cos(k * 2.0 * uv.y + _Time.y) * cos(k * 1.5 * uv.y + _Time.y);
                    uv.y += 0.4 / k * cos(k * 2.0 * uv.x + _Time.y);
                }
                
                float3 col = cos(_Time.y / 4.0 - uv.xyx);
                if (col.r > 0.0) col.r = 1.0;
                if (col.g > 0.0) col.g = 1.0;
                if (col.b > 0.0) col.b = 1.0;
                
                if (col.r > 0.99 && col.g > 0.99 && col.b > 0.99) 
                    col = float3(_Time.y, _Time.y, _Time.y);
                
                return float4(col, 1.0);
            }
            ENDCG
        }
    }
}
