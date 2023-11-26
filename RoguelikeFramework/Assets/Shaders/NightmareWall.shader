Shader "Unlit/NightmareWall"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _SecondTex ("Texture", 2D) = "white" {}
        _Scale ("Scale", float) = 1
        _Lerp ("Lerp", float) = 0.25
        _Closeness ("Closeness", float) = 1
        _Intensity ("Intensity", float) = 1
        _Speed ("Speed", float) = 1
    }
    SubShader
    {
        Tags{"Queue" = "Transparent"}
        LOD 100

        Pass
        {
            ZWrite Off
            Blend SrcAlpha OneMinusSrcAlpha

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                fixed4 color : COLOR;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
                float3 worldPos : TEXCOORD1;
                fixed4 color : COLOR;
            };

            sampler2D _MainTex;
            sampler2D _SecondTex;
            float4 _MainTex_ST;
            float _Scale;
            float _Lerp;
            float _Closeness;
            float _Intensity;
            float _Speed;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex);
                o.color = v.color;
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                // sample the texture

                fixed4 col = tex2D(_MainTex, i.uv);

                float mapX = (((i.worldPos.x / _Closeness + _Intensity * sin(_Time.y * _Speed + i.worldPos.y / _Closeness)) * _Scale)) % 1.0;
                float mapY = (((i.worldPos.y / _Closeness + _Intensity * sin(_Time.y * _Speed + i.worldPos.x / _Closeness)) * _Scale)) % 1.0;
                float2 offset = float2(mapX, mapY);

                //float2 offset = float2(sin((i.worldPos.x % 2) *_WorldScale * _Time.y), cos((i.worldPos.y % 2) * _WorldScale * _Time.y)) * _Scale;
                
                col = lerp(col, tex2D(_SecondTex, offset), _Lerp) * i.color;

                return col;
            }
            ENDCG
        }
    }
}
