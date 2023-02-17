Shader "Unlit/CircleBar"
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

            float Circle(float2 inPoint, float2 center, float radius)
            {
                return length(inPoint - center) - radius;
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

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 sampleAt(float2 uv)
            {
                //float sdfVal = Circle(i.uv, float2(.5f, .5f), .5f);
                float2 sc = float2(sin(_fillAmount * 3.1415f), cos(_fillAmount * 3.1415f));

                float sdfVal = sdArc(uv - float2(.5f, .5f), sc, _radius, _width);

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
