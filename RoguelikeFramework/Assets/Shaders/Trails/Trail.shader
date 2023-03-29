Shader "Unlit/Trail"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _ColorOne("First Color", Color) = (1, 1, 1, 1)
        _ColorTwo("Second Color", Color) = (1, 1, 1, 1)
        _MainSpeed("Maintex Speed", float) = 1
        _NoiseSpeed("Noise Speed", float) = 1
        _Brighten("Brighten Amount", float) = 0.4
        _Scale("Scale", float) = 1

        _UVGridSize("Grid Size", Vector) = (0.05, 0.05, 0, 0)
        __AACount("AA Count", int) = 5
    }
    SubShader
    {
        Tags { "RenderType" = "Transparent" "Queue" = "Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha

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
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _Scale;
            float4 _ColorOne;
            float4 _ColorTwo;
            float _MainSpeed;
            float _NoiseSpeed;
            float _Brighten;
            float2 _UVGridSize;

            int _AACount;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            float3 mod289(float3 x)
            {
                return x - floor(x / 289.0) * 289.0;
            }

            float4 mod289(float4 x)
            {
                return x - floor(x / 289.0) * 289.0;
            }

            float4 permute(float4 x)
            {
                return mod289((x * 34.0 + 1.0) * x);
            }

            float4 taylorInvSqrt(float4 r)
            {
                return 1.79284291400159 - r * 0.85373472095314;
            }

            //Simplex noise
            float SimplexNoise(float3 v)
            {
                const float2 C = float2(1.0 / 6.0, 1.0 / 3.0);

                // First corner
                float3 i = floor(v + dot(v, C.yyy));
                float3 x0 = v - i + dot(i, C.xxx);

                // Other corners
                float3 g = step(x0.yzx, x0.xyz);
                float3 l = 1.0 - g;
                float3 i1 = min(g.xyz, l.zxy);
                float3 i2 = max(g.xyz, l.zxy);

                // x1 = x0 - i1  + 1.0 * C.xxx;
                // x2 = x0 - i2  + 2.0 * C.xxx;
                // x3 = x0 - 1.0 + 3.0 * C.xxx;
                float3 x1 = x0 - i1 + C.xxx;
                float3 x2 = x0 - i2 + C.yyy;
                float3 x3 = x0 - 0.5;

                // Permutations
                i = mod289(i); // Avoid truncation effects in permutation
                float4 p =
                    permute(permute(permute(i.z + float4(0.0, i1.z, i2.z, 1.0))
                        + i.y + float4(0.0, i1.y, i2.y, 1.0))
                        + i.x + float4(0.0, i1.x, i2.x, 1.0));

                // Gradients: 7x7 points over a square, mapped onto an octahedron.
                // The ring size 17*17 = 289 is close to a multiple of 49 (49*6 = 294)
                float4 j = p - 49.0 * floor(p / 49.0);  // mod(p,7*7)

                float4 x_ = floor(j / 7.0);
                float4 y_ = floor(j - 7.0 * x_);  // mod(j,N)

                float4 x = (x_ * 2.0 + 0.5) / 7.0 - 1.0;
                float4 y = (y_ * 2.0 + 0.5) / 7.0 - 1.0;

                float4 h = 1.0 - abs(x) - abs(y);

                float4 b0 = float4(x.xy, y.xy);
                float4 b1 = float4(x.zw, y.zw);

                //float4 s0 = float4(lessThan(b0, 0.0)) * 2.0 - 1.0;
                //float4 s1 = float4(lessThan(b1, 0.0)) * 2.0 - 1.0;
                float4 s0 = floor(b0) * 2.0 + 1.0;
                float4 s1 = floor(b1) * 2.0 + 1.0;
                float4 sh = -step(h, 0.0);

                float4 a0 = b0.xzyw + s0.xzyw * sh.xxyy;
                float4 a1 = b1.xzyw + s1.xzyw * sh.zzww;

                float3 g0 = float3(a0.xy, h.x);
                float3 g1 = float3(a0.zw, h.y);
                float3 g2 = float3(a1.xy, h.z);
                float3 g3 = float3(a1.zw, h.w);

                // Normalise gradients
                float4 norm = taylorInvSqrt(float4(dot(g0, g0), dot(g1, g1), dot(g2, g2), dot(g3, g3)));
                g0 *= norm.x;
                g1 *= norm.y;
                g2 *= norm.z;
                g3 *= norm.w;

                // Mix final noise value
                float4 m = max(0.6 - float4(dot(x0, x0), dot(x1, x1), dot(x2, x2), dot(x3, x3)), 0.0);
                m = m * m;
                m = m * m;

                float4 px = float4(dot(x0, g0), dot(x1, g1), dot(x2, g2), dot(x3, g3));
                return 42.0 * dot(m, px);
            }

            //Simplex noise with octaves
            float SimplexNoise_Octaves(float3 inCoord, float scale, float3 speed, uint octaveNumber, float octaveScale, float octaveAttenuation, float time) {

                float output = 0.0f;
                float weight = 1.0f;

                for (uint i = 0; i < octaveNumber; i++)
                {
                    float3 coord = inCoord * scale + time * speed;

                    output += SimplexNoise(coord) * weight;

                    scale *= octaveScale;
                    weight *= 1.0f - octaveAttenuation;
                }

                return output;
            }

            //Simplex noise gradient
            float4 SimplexNoiseGradient(float3 v)
            {
                const float2 C = float2(1.0 / 6.0, 1.0 / 3.0);

                // First corner
                float3 i = floor(v + dot(v, C.yyy));
                float3 x0 = v - i + dot(i, C.xxx);

                // Other corners
                float3 g = step(x0.yzx, x0.xyz);
                float3 l = 1.0 - g;
                float3 i1 = min(g.xyz, l.zxy);
                float3 i2 = max(g.xyz, l.zxy);

                // x1 = x0 - i1  + 1.0 * C.xxx;
                // x2 = x0 - i2  + 2.0 * C.xxx;
                // x3 = x0 - 1.0 + 3.0 * C.xxx;
                float3 x1 = x0 - i1 + C.xxx;
                float3 x2 = x0 - i2 + C.yyy;
                float3 x3 = x0 - 0.5;

                // Permutations
                i = mod289(i); // Avoid truncation effects in permutation
                float4 p =
                    permute(permute(permute(i.z + float4(0.0, i1.z, i2.z, 1.0))
                        + i.y + float4(0.0, i1.y, i2.y, 1.0))
                        + i.x + float4(0.0, i1.x, i2.x, 1.0));

                // Gradients: 7x7 points over a square, mapped onto an octahedron.
                // The ring size 17*17 = 289 is close to a multiple of 49 (49*6 = 294)
                float4 j = p - 49.0 * floor(p / 49.0);  // mod(p,7*7)

                float4 x_ = floor(j / 7.0);
                float4 y_ = floor(j - 7.0 * x_);  // mod(j,N)

                float4 x = (x_ * 2.0 + 0.5) / 7.0 - 1.0;
                float4 y = (y_ * 2.0 + 0.5) / 7.0 - 1.0;

                float4 h = 1.0 - abs(x) - abs(y);

                float4 b0 = float4(x.xy, y.xy);
                float4 b1 = float4(x.zw, y.zw);

                //float4 s0 = float4(lessThan(b0, 0.0)) * 2.0 - 1.0;
                //float4 s1 = float4(lessThan(b1, 0.0)) * 2.0 - 1.0;
                float4 s0 = floor(b0) * 2.0 + 1.0;
                float4 s1 = floor(b1) * 2.0 + 1.0;
                float4 sh = -step(h, 0.0);

                float4 a0 = b0.xzyw + s0.xzyw * sh.xxyy;
                float4 a1 = b1.xzyw + s1.xzyw * sh.zzww;

                float3 g0 = float3(a0.xy, h.x);
                float3 g1 = float3(a0.zw, h.y);
                float3 g2 = float3(a1.xy, h.z);
                float3 g3 = float3(a1.zw, h.w);

                // Normalise gradients
                float4 norm = taylorInvSqrt(float4(dot(g0, g0), dot(g1, g1), dot(g2, g2), dot(g3, g3)));
                g0 *= norm.x;
                g1 *= norm.y;
                g2 *= norm.z;
                g3 *= norm.w;

                // Compute noise and gradient at P
                float4 m = max(0.6 - float4(dot(x0, x0), dot(x1, x1), dot(x2, x2), dot(x3, x3)), 0.0);
                float4 m2 = m * m;
                float4 m3 = m2 * m;
                float4 m4 = m2 * m2;
                float3 grad =
                    -6.0 * m3.x * x0 * dot(x0, g0) + m4.x * g0 +
                    -6.0 * m3.y * x1 * dot(x1, g1) + m4.y * g1 +
                    -6.0 * m3.z * x2 * dot(x2, g2) + m4.z * g2 +
                    -6.0 * m3.w * x3 * dot(x3, g3) + m4.w * g3;
                float4 px = float4(dot(x0, g0), dot(x1, g1), dot(x2, g2), dot(x3, g3));
                return 42.0 * float4(grad, dot(m4, px));
            }

            //Simplex noise gradient with octaves
            float4 SimplexNoiseGradient_Octaves(float3 inCoord, float scale, float3 speed, uint octaveNumber, float octaveScale, float octaveAttenuation, float time) {

                float4 output = 0.0f;
                float weight = 1.0f;

                for (uint i = 0; i < octaveNumber; i++)
                {
                    float3 coord = inCoord * scale + time * speed;

                    output += SimplexNoiseGradient(coord) * weight;

                    scale *= octaveScale;
                    weight *= 1.0f - octaveAttenuation;
                }

                return output;
            }

            fixed4 SampleAtUV(float2 uv)
            {
                float alpha = ((2 * (SimplexNoise(float3(uv - float2(_NoiseSpeed, 0) * _Time.y, 0) * _Scale) + 1)) * (1 - uv.x)) + _Brighten;
                alpha *= tex2D(_MainTex, fmod((uv - float2(_MainSpeed, 0) * _Time.y) + float2(1000, 1000), 1));
                fixed4 col = lerp(_ColorOne, _ColorTwo, alpha);
                col.a = clamp(alpha, 0, 1);
                return col;
            }

            fixed4 GetAt(float2 uv)
            {
                fixed4 color = fixed4(0, 0, 0, 0);
                uv = uv - fmod(uv, _UVGridSize);
                
                
                return SampleAtUV(uv);
            }

            fixed4 frag(v2f i) : SV_Target
            {
                // sample the texture
                fixed4 col = GetAt(i.uv);

                //col = fixed4(alpha, alpha, alpha, alpha);

                
                // apply fog
                //UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
    }
}
