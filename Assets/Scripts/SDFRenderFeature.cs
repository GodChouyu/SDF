using System;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class SDFRenderFeature : ScriptableRendererFeature
{
    [CanBeNull] private SDFRenderPass _sdfValueRenderFeaturePass;
    private const string ValueRTHandleName = "SDFValue";
    private const string ColorRTHandleName = "SDFColor";

    [SerializeField] private SDFRenderFeatureSettings settings = new();

    public override void Create()
    {
        _sdfValueRenderFeaturePass?.Dispose();
        _sdfValueRenderFeaturePass = new SDFRenderPass(settings);
    }

    protected override void Dispose(bool disposing)
    {
        _sdfValueRenderFeaturePass?.Dispose();
        _sdfValueRenderFeaturePass = null;
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        renderer.EnqueuePass(_sdfValueRenderFeaturePass);
    }
}

[Serializable]
public class SDFRenderFeatureSettings
{
    public RenderPassEvent renderPassEvent = RenderPassEvent.AfterRenderingOpaques;
    public FilterMode filterMode = FilterMode.Point;
    public TextureWrapMode textureWrapMode = TextureWrapMode.Clamp;
}