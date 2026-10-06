using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class PSXPixelizationFeature : ScriptableRendererFeature
{
    [SerializeField] private Shader shader;

    private Material material;
    private PSXPixelizationPass pass;

    public override void Create()
    {
        if (shader == null)
            shader = Shader.Find("Hidden/PSXPixelization");

        if (shader == null)
            return;

        material = CoreUtils.CreateEngineMaterial(shader);
        pass = new PSXPixelizationPass(material);
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (material == null || pass == null)
            return;

        // Skip scene view and preview cameras
        if (renderingData.cameraData.cameraType != CameraType.Game)
            return;

        var volume = VolumeManager.instance.stack.GetComponent<PSXPixelizationVolume>();
        if (volume == null || !volume.IsActive())
            return;

        pass.Setup(volume);
        renderer.EnqueuePass(pass);
    }

    protected override void Dispose(bool disposing)
    {
        if (material != null)
            CoreUtils.Destroy(material);

        pass?.Dispose();
    }
}
