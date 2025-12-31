using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;

public class SDFRenderPass : ScriptableRenderPass
{
    private const string SDFRenderPassName = "SDFRenderPass";
    private const string ValueTextureName = "SDFValue";
    private const string ColorTextureName = "SDFColor";

    private readonly SDFRenderFeatureSettings _settings;
    private static readonly int Size = Marshal.SizeOf<SDFShape>();
    private static readonly int ShapeCountId = Shader.PropertyToID("shape_count");
    private static readonly int ShapesId = Shader.PropertyToID("shapes");
    private readonly Material _material;

    private RTHandle _valueRTHandle;
    private RTHandle _colorRTHandle;

    public SDFRenderPass(SDFRenderFeatureSettings settings)
    {
        _settings = settings;
        var sdfShader = Shader.Find("Custom/SDF");
        _material = CoreUtils.CreateEngineMaterial(sdfShader);
    }

    public void Dispose()
    {
        CoreUtils.Destroy(_material);
        _valueRTHandle?.Release();
        _colorRTHandle?.Release();
    }

    private class SDFPassData
    {
        public Material Material;
        public RTHandle ValueRTHandle;
        public RTHandle ColorRTHandle;
        public Vector4[] Shapes;
    }

    private static void ExecutePass(SDFPassData data, RasterGraphContext context)
    {
        var cmd = context.cmd;
        cmd.ClearRenderTarget(true, true, Color.white);

        cmd.SetGlobalInteger(ShapeCountId, Size);
        cmd.SetGlobalVectorArray(ShapesId, data.Shapes);

        cmd.DrawProcedural(Matrix4x4.identity, data.Material, 0, MeshTopology.Triangles, 3);
    }

    public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
    {
        var resourceData = frameData.Get<UniversalResourceData>();
        var cameraData = frameData.Get<UniversalCameraData>();

        var descriptor = cameraData.cameraTargetDescriptor;
        RenderingUtils.ReAllocateHandleIfNeeded(ref _valueRTHandle, descriptor, _settings.filterMode,
            _settings.textureWrapMode, name: ValueTextureName);
        RenderingUtils.ReAllocateHandleIfNeeded(ref _colorRTHandle, descriptor, _settings.filterMode,
            _settings.textureWrapMode, name: ColorTextureName);

        var valueTextureHandle = renderGraph.ImportTexture(_valueRTHandle);
        var colorTextureHandle = renderGraph.ImportTexture(_colorRTHandle);
        if (!valueTextureHandle.IsValid() || !colorTextureHandle.IsValid()) return;

        // var shapeData = new Vector4[SDFShapeRender.ShapesRenderers.Count * 4];
        // for (var i = 0; i < shapeData.Length; i++)
        // {
        //     var baseIndex = i * 4;
        //     var shape = SDFShapeRender.ShapesRenderers[i].sdfShape;
        //     shapeData[baseIndex + 0] = new Vector4(shape.center.x, shape.center.y, shape.size.x, shape.size.y);
        //     shapeData[baseIndex + 1] = new Vector4(shape.radius, shape.angle, shape.v1.x, shape.v1.y);
        //     shapeData[baseIndex + 2] = new Vector4(shape.v2.x, shape.v2.y, (int)shape.shapeType, (int)shape.operation);
        //     shapeData[baseIndex + 3] = Vector4.zero;
        // }
        //
        // using var builder = renderGraph.AddRasterRenderPass<SDFPassData>(SDFRenderPassName, out var passData);
        // builder.SetRenderAttachment(valueTextureHandle, 0, AccessFlags.ReadWrite);
        //
        // passData.Material = _material;
        // passData.ValueRTHandle = _valueRTHandle;
        // passData.ColorRTHandle = _colorRTHandle;
        // passData.Shapes = shapeData;
        //
        // builder.SetRenderFunc((SDFPassData data, RasterGraphContext context) => ExecutePass(data, context));
    }
}