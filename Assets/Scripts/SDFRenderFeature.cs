using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class SDFRenderFeature : ScriptableRendererFeature
{
    private readonly SDFRenderPass _sdfValueRenderFeaturePass = new();
    private const string ValueRTHandleName = "SDFValue";
    private const string ColorRTHandleName = "SDFColor";

    [SerializeField] private SDFRenderFeatureSettings settings = new();
    private Material _material;
    private RTHandle _valueRTHandle;
    private RTHandle _colorRTHandle;
    private readonly List<SDFShape> _sdfShapes = new();

    public override void Create()
    {
        Debug.LogWarning("============================Create");
        var shader = Shader.Find("Custom/SDF");
        CoreUtils.Destroy(_material);
        _material = CoreUtils.CreateEngineMaterial(shader);
    }

    protected override void Dispose(bool disposing)
    {
        Debug.LogWarning("============================Dispose");
        CoreUtils.Destroy(_material);
        _valueRTHandle?.Release();
        _colorRTHandle?.Release();
        base.Dispose(disposing);
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        var descriptor = renderingData.cameraData.cameraTargetDescriptor;
        descriptor.colorFormat = RenderTextureFormat.RFloat;
        RenderingUtils.ReAllocateHandleIfNeeded(ref _valueRTHandle, descriptor, FilterMode.Point,
            settings.textureWrapMode, name: ValueRTHandleName);
        RenderingUtils.ReAllocateHandleIfNeeded(ref _colorRTHandle, descriptor, FilterMode.Point,
            settings.textureWrapMode, name: ColorRTHandleName);
        _sdfValueRenderFeaturePass.ValueRTHandle = _valueRTHandle;
        _sdfValueRenderFeaturePass.ColorRTHandle = _colorRTHandle;
        _sdfShapes.Clear();
        foreach (var shape in SDFShapeRender.Shapes)
        {
            _sdfShapes.Add(shape.sdfShape);
        }

        _sdfValueRenderFeaturePass.SDFShapes = _sdfShapes;
        _sdfValueRenderFeaturePass.Material = _material;

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