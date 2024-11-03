Shader "Unlit/WorldTextureCutout"
{
    Properties
    {
        _WorldTex ("World Texture", 2D) = "white" {}
        _PPU ("World Texture PPU", Float) = 32
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            #include "UnityCG.cginc"

            struct inputdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 worldTexUV : TEXCOORD0;
            };

            sampler2D _WorldTex;
            float4 _WorldTex_TexelSize;

            float _PPU;
            
            v2f vert (inputdata v)
            {
                v2f output;
                output.pos = UnityObjectToClipPos(v.vertex);
                
                float2 worldPos = mul(unity_ObjectToWorld, v.vertex).xy;
                float2 rawUV = worldPos * _WorldTex_TexelSize.xy * _PPU;
                output.worldTexUV.x = rawUV.x;
                output.worldTexUV.y = 1. - rawUV.y;
                return output;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_WorldTex, frac(i.worldTexUV));
                return col;
            }
            ENDCG
        }
    }
}
