Shader "Unlit/BeamLightShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Color", Color) = (1,1,1,1)
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        LOD 100

        Pass
        {
            Blend DstColor One
            ZWrite Off
            Cull Off
            Lighting Off
            Fog { Mode Off }

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            fixed4 _Color;

            struct VertexData
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct FragmentData
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            FragmentData vert (VertexData v)
            {
                FragmentData o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (FragmentData i) : SV_Target
            {
                fixed4 texColor = tex2D(_MainTex, i.uv);
                // if (texColor.a > 0.5f)
                // {    texColor.a = 0.0f;}
                // else if (texColor.a < 0.5f)
                //     texColor = fixed4(1, 1, 1, 1);
                
                fixed4 col = texColor * _Color;
                return col;
            }
            ENDCG
        }
    }
}
