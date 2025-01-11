Shader "Lyport/WorldTextureCutout"
{
    Properties
    {
        _Color ("Color", Color) = (1, 1, 1, 1)
        [HideInInspector] _MainTex ("Main Texture", 2D) = "clear" {}
        _WorldTex ("World Texture", 2D) = "white" {}
        _PPU ("World Texture PPU", Float) = 32
        _FogColor ("Fog Color", Color) = (1, 1, 1, 1)
        _FogIntensity ("Fog Intensity", Range(0,1)) = 0.5
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
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 localTexUV : TEXCOORD0;
                float2 worldTexUV : TEXCOORD1;
            };

            half4 _Color;
            sampler2D _MainTex;
            sampler2D _WorldTex;
            float4 _WorldTex_TexelSize;
            
            float4 _FogColor;
            float _FogIntensity;


            float _PPU;
            
            v2f vert (inputdata input)
            {
                v2f lerpData;
                lerpData.pos = UnityObjectToClipPos(input.vertex);
                lerpData.localTexUV = input.uv;
                
                float2 worldPos = mul(unity_ObjectToWorld, input.vertex).xy;
                float2 rawUV = worldPos * _WorldTex_TexelSize.xy * _PPU;
                lerpData.worldTexUV.x = rawUV.x;
                lerpData.worldTexUV.y = 1. - rawUV.y;
                return lerpData;
            }

            fixed4 frag (v2f lerpData) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, lerpData.localTexUV);
                fixed4 worldTexColor = tex2D(_WorldTex, frac(lerpData.worldTexUV));
                col = lerp(worldTexColor, col, col.a) * _Color;
                col.rgb = lerp(col.rgb, _FogColor.rgb, _FogIntensity);
                return col;
            }
            ENDCG
        }
    }
}
