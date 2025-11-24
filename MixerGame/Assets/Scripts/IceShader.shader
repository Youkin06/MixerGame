Shader "Custom/URP2DIce_Rounded"
{
    Properties
    {
        [Header(Colors)]
        _EdgeColor ("Edge Color", Color) = (1, 1, 1, 1)
        _CenterColor ("Center Color", Color) = (0.5, 0.8, 1.0, 0.2)

        [Header(Settings)]
        _Power ("Transparency Power", Range(0.5, 10.0)) = 3.0
        
        // ★ここを追加：角丸の半径 (0.0 = 四角, 0.5 = 円)
        _Radius ("Corner Radius", Range(0.0, 0.5)) = 0.1 
    }

    SubShader
    {
        Tags 
        { 
            "RenderType" = "Transparent" 
            "Queue" = "Transparent" 
            "RenderPipeline" = "UniversalPipeline" 
            "IgnoreProjector" = "True"
            "PreviewType" = "Plane"
            "CanUseSpriteAtlas" = "True"
        }

        Pass
        {
            Tags { "LightMode" = "Universal2D" }

            Blend SrcAlpha OneMinusSrcAlpha
            Cull Off
            ZWrite Off

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS   : POSITION;
                float4 color        : COLOR;
                float2 uv           : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS  : SV_POSITION;
                float4 color        : COLOR;
                float2 uv           : TEXCOORD0;
            };

            CBUFFER_START(UnityPerMaterial)
                float4 _EdgeColor;
                float4 _CenterColor;
                float _Power;
                float _Radius; // 角丸用変数
            CBUFFER_END

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = IN.uv;
                OUT.color = IN.color;
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                // --- 1. 角丸（ラウンデッドボックス）の計算 ---
                
                // UVを中心に移動 (-0.5 ～ 0.5) し、絶対値をとって第1象限(右上)だけで計算する
                float2 p = abs(IN.uv - 0.5);
                
                // 角丸の中心位置を計算
                float2 q = p - (0.5 - _Radius);
                
                // 角の外側へ飛び出している距離を計算
                float dist = length(max(q, 0.0));

                // 半径(_Radius)を超えていたら透明にする（なめらかに消す）
                // 0.01はアンチエイリアス（ジャギー防止）のためのぼかし幅
                float alphaMask = 1.0 - smoothstep(_Radius - 0.01, _Radius, dist);


                // --- 2. 以前の氷グラデーション処理 ---
                
                float centerDist = distance(IN.uv, float2(0.5, 0.5));
                float t = saturate(centerDist * 2.0);
                t = pow(t, _Power);

                half4 finalColor = lerp(_CenterColor, _EdgeColor, t);
                finalColor *= IN.color;

                // --- 3. 角丸マスクを適用 ---
                // 角の外側なら alphaMask が 0 になるので消える
                finalColor.a *= alphaMask;

                return finalColor;
            }
            ENDHLSL
        }
    }
}