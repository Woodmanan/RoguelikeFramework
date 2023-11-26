Shader "Unlit/MessageBackground"
{
    Properties
    {
        _MainTex("Texture", 2D) = "white" {}
        _fillColor("Main Fill Color", Color) = (1,1,1,1)
        _backgroundColor("Background Color", Color) = (0,0,0,1)
        _borderColor("Border Color", Color) = (0,0,0,1)
        _borderWidth("Border Width", float) = 0.05
        _radius("Radius", float) = 0.4
        _width("Width", float) = 0.1
        _fillAmount("FillAmount", float) = .5
        _AACount("AA Count", int) = 5
        _PixRad("Pixel Radius", int) = 10
        _PixOffset("Pixel offset", Vector) = (10, 10, 0, 0)
        _circleSpeed("Circle speed", Vector) = (0, 0.2, 0, 0)
        _UnionExpansion("Union Expansion", Vector) = (0, 0.05, 0, 0)
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
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

            float _borderWidth;
            float _fillAmount;
            float4 _fillColor, _borderColor, _backgroundColor;
            float _radius, _width, _aliasDist;

            sampler2D _MainTex;
            float _AACount;
            int _PixRad;
            float4 _PixOffset;
            float4 _circleSpeed;
            float4 _UnionExpansion;

            float _boxRects[200];
            int _numRects;

            float Circle(float2 inPoint, float2 center, float radius)
            {
                return length(inPoint - center) - radius;
            }

            float sdEllipse(in float2 p, in float2 ab)
            {
                p = abs(p); if (p.x > p.y) { p = p.yx; ab = ab.yx; }
                float l = ab.y * ab.y - ab.x * ab.x;
                float m = ab.x * p.x / l;      float m2 = m * m;
                float n = ab.y * p.y / l;      float n2 = n * n;
                float c = (m2 + n2 - 1.0) / 3.0; float c3 = c * c * c;
                float q = c3 + m2 * n2 * 2.0;
                float d = c3 + m2 * n2;
                float g = m + m * n2;
                float co;
                if (d < 0.0)
                {
                    float h = acos(q / c3) / 3.0;
                    float s = cos(h);
                    float t = sin(h) * sqrt(3.0);
                    float rx = sqrt(-c * (s + t + 2.0) + m2);
                    float ry = sqrt(-c * (s - t + 2.0) + m2);
                    co = (ry + sign(l) * rx + abs(g) / (rx * ry) - m) / 2.0;
                }
                else
                {
                    float h = 2.0 * m * n * sqrt(d);
                    float s = sign(q + h) * pow(abs(q + h), 1.0 / 3.0);
                    float u = sign(q - h) * pow(abs(q - h), 1.0 / 3.0);
                    float rx = -s - u - c * 4.0 + 2.0 * m2;
                    float ry = (s - u) * sqrt(3.0);
                    float rm = sqrt(rx * rx + ry * ry);
                    co = (ry / sqrt(rm - rx) + 2.0 * g / rm - m) / 2.0;
                }
                float2 r = ab * float2(co, sqrt(1.0 - co * co));
                return length(r - p) * sign(p.y - r.y);
            }

            float PixelCircle(float2 inPoint, float2 center, float radius, float2 pixSize)
            {
                return sdEllipse(inPoint - center, radius * pixSize);
            }

            float PixelCircleField(float2 inPoint, float2 c, float radius, float2 pixSize)
            {
                c = c * pixSize;
                float2 q = ((inPoint + 0.5 * c) % c) - 0.5 * c;
                return PixelCircle(q, float2(0, 0), radius, pixSize);
            }

            //Amazing code from Inigo Quilez!
            float Capsule(float2 p, float2 a, float2 b, float r)
            {
                float2 pa = p - a, ba = b - a;
                float h = clamp(dot(pa, ba) / dot(ba, ba), 0.0, 1.0);
                return (length(pa - ba * h) - r);
            }

            //Exact 2D arc from iniqo quilez
            float sdArc(in float2 p, in float2 sc, in float ra, float rb)
            {
                // sc is the sin/cos of the arc's aperture
                p.x = abs(p.x);
                return ((sc.y * p.x > sc.x * p.y) ? length(p - sc * ra) :
                    abs(length(p) - ra)) - rb;
            }

            float sdRoundedBox(in float2 p, in float2 b, in float4 r)
            {
                r.xy = (p.x > 0.0) ? r.xy : r.zw;
                r.x = (p.y > 0.0) ? r.x : r.y;
                float2 q = abs(p) - b + r.x;
                return min(max(q.x, q.y), 0.0) + length(max(q, 0.0)) - r.x;
            }

            float opSmoothUnion(float d1, float d2, float k) {
                float h = clamp(0.5 + 0.5 * (d2 - d1) / k, 0.0, 1.0);
                return lerp(d2, d1, h) - k * h * (1.0 - h);
            }

            float opSmoothIntersection(float d1, float d2, float k) {
                float h = clamp(0.5 - 0.5 * (d2 - d1) / k, 0.0, 1.0);
                return lerp(d2, d1, h) + k * h * (1.0 - h);
            }

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 sampleAt(float2 uv, float2 pixSize)
            {
                //float sdfVal = Circle(i.uv, float2(.5f, .5f), .5f);
                float2 sc = float2(sin(_fillAmount * 3.1415f), cos(_fillAmount * 3.1415f));


                float circleSDF = PixelCircleField(uv + _circleSpeed.xy * _Time.x, _PixOffset.xy * float2(2, 1), _PixRad, pixSize);
                circleSDF = min(circleSDF,
                    PixelCircleField(uv + _PixOffset.xy * pixSize + _circleSpeed.xy * _Time.x + float2(0, 1),
                        _PixOffset.xy * float2(2, 1),
                        _PixRad, pixSize));

                float sdfVal = 1000;

                for (int i = 0; i < _numRects; i++)
                {
                    int bigIndex = i * 4;
                    float unionSDF = sdRoundedBox(uv - float2(_boxRects[bigIndex], _boxRects[bigIndex + 1]) + _UnionExpansion.xy,
                        float2(_boxRects[bigIndex + 2], _boxRects[bigIndex + 3]) + _UnionExpansion.zw,
                        float4(0.01, 0.01, 0.01, 0.01));

                    float boxSDF = sdRoundedBox(uv - float2(_boxRects[bigIndex], _boxRects[bigIndex+1]), 
                        float2(_boxRects[bigIndex+2], _boxRects[bigIndex+3]), 
                        float4(0.01, 0.01, 0.01, 0.01));

                    
                    float finBoxSDF = opSmoothIntersection(circleSDF, unionSDF, 0.1);
                    finBoxSDF = opSmoothUnion(boxSDF, finBoxSDF, 0.01);
                    sdfVal = min(sdfVal, finBoxSDF);
                }


                //sdfVal = circleSDF;

                //float sdfVal = sdEllipse(uv - float2(.5f, .5f), _PixRad * pixSize);

                if (sdfVal > 0)
                {
                    return float4(0, 0, 0, 0);
                }
                else
                {
                    sdfVal = sdfVal + _borderWidth;
                    if (sdfVal > 0)
                    {
                        return _borderColor;
                    }
                    else
                    {

                        return _fillColor;

                    }
                }
            }

            fixed4 frag(v2f i) : SV_Target
            {
                
                float2 pixSize = abs(float2(ddx(i.uv.x), ddy(i.uv.y)));
                float2 offset = pixSize / (_AACount);

                fixed4 color = sampleAt(i.uv, pixSize);

                int AABound = _AACount / 2;
                for (int x = -AABound; x <= AABound; x++)
                {
                    for (int y = -AABound; y <= AABound; y++)
                    {
                        float2 coordOffset = float2(x * offset.x, y * offset.y);
                        color += sampleAt(i.uv + coordOffset, pixSize);
                    }
                }
                color = color / ((_AACount * _AACount) + 1.0);
                return color;
            }
            ENDCG
        }
    }
}
