Shader "Unlit/FillBar"
{
    Properties
    {
        _fillColor("Main Fill Color", Color) = (1,1,1,1)
        _backgroundColor("Background Color", Color) = (0,0,0,1)
        _borderColor("Border Color", Color) = (0,0,0,1)
        _borderWidth("Border Width", float) = 0.05
        _fillAmount("FillAmount", float) = .5
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

            sampler2D _MainTex;
            float _borderWidth;
            float _fillAmount;
            float4 _fillColor, _borderColor, _backgroundColor;

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

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float rad = .1f;
                //float sdfVal = Circle(i.uv, float2(.5f, .5f), .5f);
                float sdfVal = Capsule(i.uv, float2(.5f, rad), float2(.5f, 1 - rad), rad);

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
                        if (i.uv.y < _fillAmount)
                        {
                            return _fillColor;
                        }
                        else
                        {
                            return _backgroundColor;
                        }
                    }
                }
            }
            ENDCG
        }
    }
}
