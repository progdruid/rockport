Shader "Unlit/FoliageWindShader"
{
    Properties
    {
        _MainTex ("Sprite Texture", 2D) = "white" {}
        _WindStrength ("Wind Strength", Float) = 0.03
        _WindSpeed ("Wind Speed", Float) = 2.0
        _WindFrequency ("Wind Frequency", Float) = 2.0
        _Cutoff ("Alpha Cutoff", Range(0,1)) = 0.5
        [HideInInspector] _UVRect ("UV Rect", Vector) = (0, 0, 1, 1)
        [Toggle] _FlipX ("Flip X", Float) = 0.0
    }
    SubShader
    {
        Tags { "RenderType"="AlphaTest" "Queue"="AlphaTest" }
        LOD 100
        Cull Off
        
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct VertexData
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct FragmentData
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float3 worldPos : TEXCOORD1; 
            };

            sampler2D _MainTex;
            float4 _UVRect;
            float _WindStrength;
            float _WindSpeed;
            float _WindFrequency;
            float _Cutoff;
            float _FlipX;

            FragmentData vert (VertexData v)
            {
                FragmentData o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                if (_FlipX > 0.5f)
                    v.uv.x = 1.0 - v.uv.x;
                o.uv = v.uv;
                return o;
            }

            // Simple 2D value noise in HLSL (returns [0,1] range)
            float noise(float2 uv)
            {
                //return tex2D(_NoiseTex, frac(uv * 0.05f)).r;
                float2 i = floor(uv);
                float2 f = frac(uv);

                // Quintic interpolation for smooth transitions
                float2 u = f * f * (3.0 - 2.0 * f);

                // Random values at corners
                float n = dot(i, float2(1.0, 57.0));
                float a = frac(sin(n) * 43758.5453);
                float b = frac(sin(n + 1.0) * 43758.5453);
                float c = frac(sin(n + 57.0) * 43758.5453);
                float d = frac(sin(n + 58.0) * 43758.5453);

                // Bilinear interpolation
                float k0 = lerp(a, b, u.x);
                float k1 = lerp(c, d, u.x);
                float value = lerp(k0, k1, u.y);

                return value;
            }

            float wind(float x)
            {
                float val = sin(sin(x) - 2.3f * cos(x));
                return val * 0.5f + 0.5f;
            }
            
            fixed4 frag (FragmentData i) : SV_Target
            {
                _WindFrequency = 0.3f;
                _WindSpeed = 10.f;
                _WindStrength = 0.004f;
                
                float lookupX = (i.worldPos.x + _Time.y * _WindSpeed) * _WindFrequency;
                float windFactor = wind(lookupX);
                float noiseFactor = noise(float2(lookupX/2, i.worldPos.y / 10.f));
                float wind = (0.0f * windFactor + 1.0f * noiseFactor);
                
                float2 localUV = (i.uv - _UVRect.xy) / _UVRect.zw;

                float add = wind * _WindStrength * lerp(0.5f, 1.f, localUV.y);
                float2 shiftedUV = i.uv;
                if (_FlipX > 0.5f)
                    shiftedUV.x -= add;
                else
                    shiftedUV.x += add;
                
                if (shiftedUV.x < _UVRect.x || shiftedUV.x > _UVRect.x + _UVRect.z ||
                    shiftedUV.y < _UVRect.y || shiftedUV.y > _UVRect.y + _UVRect.w)
                    discard;

                fixed4 col = tex2D(_MainTex, shiftedUV);
                if (col.a < _Cutoff) discard;
                return col;
            }
            ENDCG
        }
    }
}
