Shader "ThicknessDepth/DebugDepth"
{
    Properties
    {
        _DebugNear("Near", float) = 0
        _DebugFar("Far", float) = 5
    }
    SubShader
    {
        Tags
        {
            "RenderPipeline"="UniversalPipeline"
            "RenderType"="Opaque"
        }

        Pass
        {
            Name "DebugThicknessDepth"
            ZWrite Off
            ZTest Always
            Cull Off

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            TEXTURE2D(_ThicknessDepthTexture);
            SAMPLER(sampler_ThicknessDepthTexture);

            float _DebugNear;
            float _DebugFar;

            Varyings vert (Attributes v)
            {
                Varyings o;
                o.positionCS = TransformObjectToHClip(v.positionOS.xyz);
                o.uv = v.uv;
                return o;
            }

            float4 frag (Varyings i) : SV_Target
            {
                float depth = SAMPLE_TEXTURE2D(
                    _ThicknessDepthTexture,
                    sampler_ThicknessDepthTexture,
                    i.uv
                ).r;

                float t = saturate((depth - _DebugNear) / (_DebugFar - _DebugNear));
                return float4(t, t, t, 1);

                //return float4(depth, depth, depth, 1);
            }
            ENDHLSL
        }
    }
}
