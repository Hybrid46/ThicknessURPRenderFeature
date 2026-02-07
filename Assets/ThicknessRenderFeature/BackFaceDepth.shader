Shader "ThicknessDepth/BackFaceDepth"
{
    SubShader
    {
        Tags { 
            "RenderPipeline"="UniversalPipeline" 
            "RenderType"="Opaque" 
            "LightMode" = "UniversalForward"
        }

        Pass
        {
            Name "BackFaceDepth"
            Cull Front
            ZWrite Off
            ZTest LEqual // GEqual for overlap fix?

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
            };

            Varyings vert (Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                return OUT;
            }

            float frag (Varyings IN) : SV_Target
            {
                float depth = IN.positionHCS.z / IN.positionHCS.w;
                return LinearEyeDepth(depth, _ZBufferParams);
            }
            ENDHLSL
        }
    }
}
