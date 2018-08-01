Shader "Unlit/HightMapNormal"
{
    Properties
    {
        _MainTex("Texture", 2D) = "white" {}
        _ParallaxMap("Parallax Map", 2D) = "gray" {}
    }
        SubShader
    {
        Tags { "RenderType" = "Opaque" }
        LOD 100

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
                float3 normal : NORMAL;
            };

            sampler2D _MainTex;
            sampler2D _ParallaxMap;
            float4 _MainTex_ST;

            float2 _ParallaxMap_TexelSize;

            v2f vert(appdata v)
            {
                float2 shiftX = float2(_ParallaxMap_TexelSize.x, 0);
                float2 shiftZ = float2(0, _ParallaxMap_TexelSize.y);

                float3 texX = tex2Dlod(_ParallaxMap, float4(v.uv.xy + shiftX, 0, 0)) * 2.0 - 1;
                float3 texx = tex2Dlod(_ParallaxMap, float4(v.uv.xy - shiftX, 0, 0)) * 2.0 - 1;
                float3 texZ = tex2Dlod(_ParallaxMap, float4(v.uv.xy + shiftZ, 0, 0)) * 2.0 - 1;
                float3 texz = tex2Dlod(_ParallaxMap, float4(v.uv.xy - shiftZ, 0, 0)) * 2.0 - 1;

                float3 du = float3(1, 0, (texX.x - texx.x));
                float3 dv = float3(1, 0, (texZ.x - texz.x));

                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.normal = normalize(cross(du, dv)) * 0.5 + 0.5;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                //fixed4 col = tex2D(_MainTex, i.uv);
                fixed4 col = float4(i.normal, 1);
                return col;
            }
            ENDCG
        }
    }
}
