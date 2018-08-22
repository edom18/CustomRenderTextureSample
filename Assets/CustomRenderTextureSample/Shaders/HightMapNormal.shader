Shader "Unlit/HightMapNormal"
{
    Properties
    {
        _Color("Tint color", Color) = (1, 1, 1, 1)
        _MainTex("Texture", 2D) = "white" {}
        _ParallaxMap("Parallax Map", 2D) = "gray" {}
    }

    SubShader
    {
        Tags { "RenderType" = "Transparent" "Queue" = "Transparent" }
        LOD 100

        GrabPass { }

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float4 uvgrab : TEXCOORD1;
            };

            sampler2D _MainTex;
            sampler2D _ParallaxMap;
            sampler2D _GrabTexture;
            float4 _MainTex_ST;
            fixed4 _Color;

            float2 _ParallaxMap_TexelSize;

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);

#if UNITY_UV_STARTS_AT_TOP
                float scale = -1.0;
#else
                float scale = 1.0;
#endif
                o.uvgrab.xy = (float2(o.vertex.x, o.vertex.y * scale) + o.vertex.w) * 0.5;
                o.uvgrab.zw = o.vertex.zw;

                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float2 shiftX = float2(_ParallaxMap_TexelSize.x, 0);
                float2 shiftZ = float2(0, _ParallaxMap_TexelSize.y);

                float3 texX = tex2D(_ParallaxMap, float4(i.uv.xy + shiftX, 0, 0)) * 2.0 - 1;
                float3 texx = tex2D(_ParallaxMap, float4(i.uv.xy - shiftX, 0, 0)) * 2.0 - 1;
                float3 texZ = tex2D(_ParallaxMap, float4(i.uv.xy + shiftZ, 0, 0)) * 2.0 - 1;
                float3 texz = tex2D(_ParallaxMap, float4(i.uv.xy - shiftZ, 0, 0)) * 2.0 - 1;

                float3 du = float3(1, 0, (texX.x - texx.x));
                float3 dv = float3(0, (texZ.x - texz.x), 1);

                float3 n = normalize(cross(dv, du));

                i.uvgrab.xy += n * i.uvgrab.z;

                fixed4 col = tex2Dproj(_GrabTexture, UNITY_PROJ_COORD(i.uvgrab)) * _Color;

                return col;
            }
            ENDCG
        }
    }
}
