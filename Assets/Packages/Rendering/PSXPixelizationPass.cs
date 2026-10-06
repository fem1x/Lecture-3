using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering.RenderGraphModule;

public class PSXPixelizationPass : ScriptableRenderPass, System.IDisposable
{
    private const string PassName = "PSX Pixelization";

    private Material material;
    private PSXPixelizationVolume volume;

    private static readonly int RenderScaleId = Shader.PropertyToID("_RenderScale");
    private static readonly int ColorDepthId = Shader.PropertyToID("_ColorDepth");
    private static readonly int PixelatedTexId = Shader.PropertyToID("_PixelatedTex");

    public PSXPixelizationPass(Material material)
    {
        this.material = material;
        renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;
    }

    public void Setup(PSXPixelizationVolume volume)
    {
        this.volume = volume;
    }

    private class PassData
    {
        public Material material;
        public float renderScale;
        public int colorDepth;
        public TextureHandle source;
        public TextureHandle downscaled;
    }

    public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
    {
        var resourceData = frameData.Get<UniversalResourceData>();
        var cameraData = frameData.Get<UniversalCameraData>();

        if (resourceData.isActiveTargetBackBuffer)
            return;

        var source = resourceData.activeColorTexture;
        var descriptor = cameraData.cameraTargetDescriptor;

        // Calculate uniform scale based on target height
        int targetHeight = volume.targetHeight.value;
        int colorDepth = volume.colorDepth.value;
        
        // Calculate scale to achieve target height regardless of current resolution
        float scale = (float)targetHeight / descriptor.height;
        
        // Create a low-res render target
        var downDesc = descriptor;
        downDesc.width = Mathf.Max(1, (int)(descriptor.width * scale));
        downDesc.height = Mathf.Max(1, targetHeight);
        downDesc.depthBufferBits = 0;
        downDesc.msaaSamples = 1;

        var downscaled = UniversalRenderer.CreateRenderGraphTexture(
            renderGraph, downDesc, "_PSXDownscaled", false, FilterMode.Point);

        // Pass 0: Downscale + color reduction into low-res target
        using (var builder = renderGraph.AddRasterRenderPass<PassData>(PassName + " Down", out var passData))
        {
            passData.material = material;
            passData.renderScale = scale;
            passData.colorDepth = colorDepth;
            passData.source = source;
            passData.downscaled = downscaled;

            builder.UseTexture(source, AccessFlags.Read);
            builder.SetRenderAttachment(downscaled, 0, AccessFlags.Write);

            builder.SetRenderFunc((PassData data, RasterGraphContext ctx) =>
            {
                data.material.SetFloat(RenderScaleId, data.renderScale);
                data.material.SetInt(ColorDepthId, data.colorDepth);
                Blitter.BlitTexture(ctx.cmd, data.source, new Vector4(1, 1, 0, 0), data.material, 0);
            });
        }

        // Pass 1: Upscale back to full res with point filtering
        using (var builder = renderGraph.AddRasterRenderPass<PassData>(PassName + " Up", out var passData))
        {
            passData.material = material;
            passData.downscaled = downscaled;

            builder.UseTexture(downscaled, AccessFlags.Read);
            builder.SetRenderAttachment(resourceData.activeColorTexture, 0, AccessFlags.Write);

            builder.SetRenderFunc((PassData data, RasterGraphContext ctx) =>
            {
                data.material.SetTexture(PixelatedTexId, data.downscaled);
                Blitter.BlitTexture(ctx.cmd, data.downscaled, new Vector4(1, 1, 0, 0), data.material, 1);
            });
        }
    }

    public void Dispose() { }
}
