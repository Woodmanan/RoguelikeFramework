Shader "Unlit/AbilityBlock"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _fillColor("Main Fill Color", Color) = (1,1,1,1)
        _cornerRadius("Corner Radius", float) = .05
        
        _outlineThickness("Outline Thickness", float) = 0.03
        _outlineColor("Outline Color", Color) = (1,1,1,1)

        _blendAmount("Blend Amount", Range(0, 1)) = 0
        _AACount("AA Count", int) = 5
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
            float4 _fillColor, _outlineColor;
            float _AACount;
            float _cornerRadius;
            float _outlineThickness, _blendAmount;

            //Exact sdf functions from iniqo quilez - god bless that man
            float sdRoundedBox(in float2 p, in float2 b, in float4 r)
            {
                r.xy = (p.x > 0.0) ? r.xy : r.zw;
                r.x = (p.y > 0.0) ? r.x : r.y;
                float2 q = abs(p) - b + r.x;
                return min(max(q.x, q.y), 0.0) + length(max(q, 0.0)) - r.x;
            }

            float sdCircle(in float2 p, float r)
            {
                return length(p) - r;
            }

            float sdArc(in float2 p, in float2 sc, in float ra, float rb)
            {
                // sc is the sin/cos of the arc's aperture
                p.x = abs(p.x);
                return ((sc.y * p.x > sc.x * p.y) ? length(p - sc * ra) :
                    abs(length(p) - ra)) - rb;
            }

            float sdCapsule(float2 p, float2 a, float2 b, float r)
            {
                float2 pa = p - a, ba = b - a;
                float h = clamp(dot(pa, ba) / dot(ba, ba), 0.0, 1.0);
                return (length(pa - ba * h) - r);
            }
           

            float sdTunnel(in float2 p, in float2 wh)
            {
                float2 q = abs(p);
                q.x -= wh.x;

                if (p.y >= 0.0)
                {
                    q.x = max(q.x, 0.0);
                    q.y += wh.y;
                    return -min(wh.x - length(p), length(q));
                }
                else
                {
                    q.y -= wh.y;
                    float f = max(q.x, q.y);
                    return (f < 0.0) ? f : length(max(q, 0.0));
                }
            }

            float opUnion(float d1, float d2) { return min(d1, d2); }

            float opSubtraction(float d1, float d2) { return max(-d1, d2); }

            float sdLock(in float2 p)
            {
                float main = sdTunnel(p, float2(.4, .5));

                float minor = sdTunnel(p, float2(.2, .1));

                float circle = sdCircle(p - float2(0, -.25), .07f);

                float capsule = sdCapsule(p, float2(0, -.25), float2(0, -.4), .03);

                main = opSubtraction(minor, main);
                main = opSubtraction(circle, main);
                main = opSubtraction(capsule, main);
                return main;
            }

            float sdBlendedSample(in float2 p)
            {
                float2 uv = p + float2(-0.5, -0.5);
                float2 hw = float2(.5, .5);
                float4 corners = _cornerRadius * float4(1.0, 1.0, 1.0, 1.0);

                return lerp(sdRoundedBox(uv, hw, corners), sdLock(uv), _blendAmount);
            }

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 sampleAt(float2 uv)
            {
                float dist = sdBlendedSample(uv);
                bool isIn = sdBlendedSample(uv) <= 0;

                float inOutMask = float(dist <= 0);
                float outlineMask = float(dist >= -_outlineThickness);
                
                float4 col = _fillColor * tex2D(_MainTex, uv);
                col = outlineMask * _outlineColor + (1 - outlineMask) * col;

                col.a = inOutMask;

                return col;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 color = sampleAt(i.uv);
                
                float2 offset = abs(float2(ddx(i.uv.x), ddy(i.uv.y))) / (_AACount);

                int AABound = _AACount / 2;
                for (int x = -AABound; x <= AABound; x++)
                {
                    for (int y = -AABound; y <= AABound; y++)
                    {
                        float2 coordOffset = float2(x * offset.x, y * offset.y);
                        color += sampleAt(i.uv + coordOffset);
                    }
                }
                color = color / ((_AACount * _AACount) + 1.0);
                return color;
            }
            ENDCG
        }
    }
}
