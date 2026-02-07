using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RendererUtils;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;

public class ThicknessRenderFeature : ScriptableRendererFeature
{
    [SerializeField] ThicknessRenderFeatureSettings settings = new();

    ThicknessRenderFeaturePass m_Pass;

    public override void Create()
    {
        m_Pass = new ThicknessRenderFeaturePass(settings)
        {
            renderPassEvent = RenderPassEvent.AfterRenderingPrePasses
        };

        m_Pass.ConfigureInput(ScriptableRenderPassInput.Depth);
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        renderer.EnqueuePass(m_Pass);
    }

    [Serializable]
    public class ThicknessRenderFeatureSettings
    {
        public LayerMask layerMask = 0;
        public Material backFaceDeptMaterial;
        public string globalTextureName = "_ThicknessDepthTexture";
        public RenderTextureFormat textureFormat = RenderTextureFormat.R16;
    }

    class ThicknessRenderFeaturePass : ScriptableRenderPass
    {
        readonly ThicknessRenderFeatureSettings settings;

        public ThicknessRenderFeaturePass(ThicknessRenderFeatureSettings settings)
        {
            this.settings = settings;
        }

        class PassData
        {
            public TextureHandle backDepthTexture;
            public RendererListHandle rendererList;
        }

        static void ExecutePass(PassData data, RasterGraphContext context)
        {
            context.cmd.DrawRendererList(data.rendererList);

            context.cmd.SetGlobalTexture(
                Shader.PropertyToID("_ThicknessDepthTexture"),
                data.backDepthTexture
            );
        }

        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            if (settings.backFaceDeptMaterial == null) return;

            UniversalCameraData cameraData = frameData.Get<UniversalCameraData>();
            UniversalRenderingData renderingData = frameData.Get<UniversalRenderingData>();
            UniversalResourceData resourceData = frameData.Get<UniversalResourceData>();

            using IRasterRenderGraphBuilder builder = renderGraph.AddRasterRenderPass<PassData>(
                "Thickness Depth Pass",
                out PassData passData
            );

            //builder.AllowPassCulling(false);

            RenderTextureDescriptor desc = cameraData.cameraTargetDescriptor;
            desc.colorFormat = settings.textureFormat;
            desc.depthBufferBits = 0;
            desc.msaaSamples = 1;

            passData.backDepthTexture = renderGraph.CreateTexture(
                new TextureDesc(desc)
                {
                    name = settings.globalTextureName,
                    clearBuffer = true,
                    clearColor = Color.black
                }
            );

            builder.SetRenderAttachment(passData.backDepthTexture, 0);

            RendererListDesc rendererListDesc = new RendererListDesc(
                new ShaderTagId("UniversalForward"),
                renderingData.cullResults,
                cameraData.camera
            )
            {
                renderQueueRange = RenderQueueRange.opaque,
                sortingCriteria = SortingCriteria.CommonOpaque,
                layerMask = settings.layerMask,
                overrideMaterial = settings.backFaceDeptMaterial
            };

            passData.rendererList = renderGraph.CreateRendererList(rendererListDesc);
            builder.UseRendererList(passData.rendererList);

            builder.UseTexture(resourceData.cameraDepthTexture);

            builder.SetRenderFunc((PassData data, RasterGraphContext context) =>
                ExecutePass(data, context)
            );
        }
    }
}
