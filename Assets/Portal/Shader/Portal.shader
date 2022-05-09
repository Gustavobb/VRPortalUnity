Shader "Unlit/Portal"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _DisabledColor ("Disabled Color", Color) = (1, 1, 1, 0.5)
        _Enabled ("Enabled", Range(0, 1)) = 0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        Lighting Off
		Cull Off
		ZWrite On
		ZTest Less
		LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float4 screen_pos : TEXCOORD0;
            };

            sampler2D _MainTex;
            float4 _DisabledColor;
            float _Enabled;

            v2f vert (appdata v)
            {
                v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.screen_pos = ComputeScreenPos(o.vertex);
				return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float2 uv = i.screen_pos.xy / i.screen_pos.w;
				fixed4 col = tex2D(_MainTex, uv);
				return col * _Enabled + _DisabledColor * (1 - _Enabled);
            }
            ENDCG
        }
    }
}
