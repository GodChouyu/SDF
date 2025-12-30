using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;

public class SDFRenderPass : ScriptableRenderPass
{
    public RTHandle ValueRTHandle;
    public RTHandle ColorRTHandle;
    public List<SDFShape> SDFShapes;
    public Material Material;

    private RTHandle _valueRTHandle;
    private RTHandle _colorRTHandle;
    
    private static readonly int Size = System.Runtime.InteropServices.Marshal.SizeOf<SDFShape>();

    public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
    {
        var resourceData = frameData.Get<UniversalResourceData>();
        var cameraData = frameData.Get<UniversalCameraData>();
        var cameraColorTextureHandle = resourceData.cameraColor;
        if (!cameraColorTextureHandle.IsValid())
        {
            return;
        }

        RenderingUtils.ReAllocateHandleIfNeeded(ref alueRTHandle, descriptor, FilterMode.Point,
            settings.textureWrapMode, name: ValueRTHandleName);
        RenderingUtils.ReAllocateHandleIfNeeded(ref _colorRTHandle, descriptor, FilterMode.Point,
            settings.textureWrapMode, name: ColorRTHandleName);

        var sdfValueCommandBuffer = CommandBufferPool.Get("SDFRenderer");
        var shapeBuffer = new ComputeBuffer(SDFShapes.Count, Size);
        shapeBuffer.SetData(SDFShapes);
        sdfValueCommandBuffer.SetRenderTarget(ValueRTHandle);
        sdfValueCommandBuffer.ClearRenderTarget(true, true, Color.clear);
        Blitter.BlitTexture(sdfValueCommandBuffer, cameraColorTextureHandle, ValueRTHandle, Material, 0);
        sdfValueCommandBuffer.Clear();
        CommandBufferPool.Release(sdfValueCommandBuffer);
    }
}