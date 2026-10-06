using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

[VolumeComponentMenu("Post-processing/PSX Pixelization")]
[VolumeRequiresRendererFeatures(typeof(PSXPixelizationFeature))]
[SupportedOnRenderPipeline(typeof(UniversalRenderPipelineAsset))]
public class PSXPixelizationVolume : VolumeComponent, IPostProcessComponent
{
    [Tooltip("Enable the PSX pixelization effect.")]
    public BoolParameter enabled = new BoolParameter(false, overrideState: true);

    [Tooltip("Target resolution height in pixels. Lower values = more pixelated. 240 gives a classic PSX look.")]
    public ClampedIntParameter targetHeight = new ClampedIntParameter(240, 120, 1080);

    [Tooltip("Reference resolution height for scale calculation. Default is 1080p.")]
    public ClampedIntParameter referenceHeight = new ClampedIntParameter(1080, 480, 2160);

    [Tooltip("Color depth in bits per channel. Lower = more PSX color banding.")]
    public ClampedIntParameter colorDepth = new ClampedIntParameter(32, 2, 32);

    public bool IsActive() => enabled.value && targetHeight.value < referenceHeight.value;
}
