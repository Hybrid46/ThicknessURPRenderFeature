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
            ZTest LEqual

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
                float4 positionCS : SV_POSITION;
                float3 positionVS : TEXCOORD0;
            };

            Varyings vert(Attributes IN)
            {
                Varyings OUT;

                OUT.positionCS = TransformObjectToHClip(IN.positionOS.xyz);

                float3 positionWS = TransformObjectToWorld(IN.positionOS.xyz);
                float3 positionVS = TransformWorldToView(positionWS);
                
                OUT.positionVS = positionVS;

                return OUT;
            }

            float4 frag(Varyings IN) : SV_Target
            {
                float fragmentEyeDepth = -IN.positionVS.z;
                return fragmentEyeDepth;
            }
            ENDHLSL
        }
    }
}
