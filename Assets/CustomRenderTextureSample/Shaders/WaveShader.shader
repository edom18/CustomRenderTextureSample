Shader "Hidden/WaveShader"
{
    Properties
    {
        _S2("PhaseVelocity^2", Range(0.0, 0.5)) = 0.2
        _Atten("Attenuation", Range(0.0, 1.0)) = 0.999
        _DeltaUV("Delta UV", Float) = 3
    }

        SubShader
    {
        Cull Off
        ZWrite Off
        ZTest Always

        Pass
        {
            CGPROGRAM
            #pragma vertex CustomRenderTextureVertexShader
            #pragma fragment frag

            #include "UnityCustomRenderTexture.cginc"

            half _S2;
            half _Atten;
            float _DeltaUV;
            sampler2D _MainTex;

            float4 frag(v2f_customrendertexture  i) : SV_Target
            {
                float2 uv = i.globalTexcoord;

                // 1pxあたりの単位を計算する
                float du = 1.0 / _CustomRenderTextureWidth;
                float dv = 1.0 / _CustomRenderTextureHeight;
                float3 duv = float3(du, dv, 0) * _DeltaUV;

                // 現在の位置のテクセルをフェッチ
                float2 c = tex2D(_SelfTexture2D, uv);

                // ラプラシアンフィルタをかける
                // |0  1 0|
                // |1 -4 1|
                // |0  1 0|
                float k = (2.0 * c.r) - (c.g + _S2);
                float p = (k * (
                    tex2D(_SelfTexture2D, uv - duv.zy).r +
                    tex2D(_SelfTexture2D, uv + duv.zy).r +
                    tex2D(_SelfTexture2D, uv - duv.xz).r +
                    tex2D(_SelfTexture2D, uv + duv.xz).r - 4.0 * c.r
                )) * _Atten;

                return float4(p, c.r, 0, 0);
            }
            ENDCG
        }
    }
}
