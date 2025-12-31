using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;

public class SDFRenderManager : MonoBehaviour
{
    public static SDFRenderManager Instance;
    private const int MaxCount = 64;
    private static readonly int GradientTexture = Shader.PropertyToID("gradient_texture");
    private static readonly int Shapes = Shader.PropertyToID("shapes");
    private static readonly int ShapeCount = Shader.PropertyToID("shape_count");
    private static readonly int FieldMin = Shader.PropertyToID("field_min");
    private static readonly int FieldMax = Shader.PropertyToID("field_max");
    private static readonly int Stride = Marshal.SizeOf(typeof(SDFShape));

    public static readonly List<SDFShapeRender> SDFShapeRenders = new();
    private static readonly int SmoothK = Shader.PropertyToID("smooth_k");

    public float fieldMin = -50f;
    public float fieldMax = 50f;
    public float smoothK = 5f;
    public Gradient fieldGradient;
    public SpriteRenderer spriteRenderer;

    private Material _material;
    private Texture2D _gradientTexture;
    private ComputeBuffer _shapeBuffer;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            enabled = false;
            Debug.LogError("There is already an instance of SDFRenderManager in the scene!", this);
        }
    }

    private void Start()
    {
        if (spriteRenderer == null)
        {
            Debug.LogError("SpriteRenderer not assigned");
            enabled = false;
            return;
        }

        _material = Instantiate(spriteRenderer.sharedMaterial);
        spriteRenderer.material = _material;
        CreateGradientTexture();
    }

    private void Update()
    {
        UpdateGradientTexture();
        UpdateShapeBuffer();
        ApplyMaterialParams();
    }

    private void OnDestroy()
    {
        _shapeBuffer?.Release();
    }

    private void CreateGradientTexture()
    {
        _gradientTexture = new Texture2D(256, 1, TextureFormat.RGBA32, false);
        _gradientTexture.wrapMode = TextureWrapMode.Clamp;
        _gradientTexture.filterMode = FilterMode.Bilinear;
    }

    private void UpdateGradientTexture()
    {
        for (var i = 0; i < 256; i++)
        {
            var t = i / 255f;
            _gradientTexture.SetPixel(i, 0, fieldGradient.Evaluate(t));
        }

        _gradientTexture.Apply(false, false);
    }

    private void UpdateShapeBuffer()
    {
        var count = SDFShapeRenders.Count;
        if (count == 0) return;

        if (count > MaxCount)
        {
            count = MaxCount;
            Debug.LogWarning(
                $"The number of shapes exceeds the maximum limit, only the first {MaxCount} shapes will be rendered.");
        }

        _shapeBuffer?.Release();
        _shapeBuffer = new ComputeBuffer(count, Stride);

        var sdfShapes = new SDFShape[count];
        for (var i = 0; i < count; i++)
        {
            sdfShapes[i] = SDFShapeRenders[i].SDFShape;
        }

        _shapeBuffer.SetData(sdfShapes);
    }

    private void ApplyMaterialParams()
    {
        _material.SetTexture(GradientTexture, _gradientTexture);
        _material.SetFloat(FieldMin, fieldMin);
        _material.SetFloat(FieldMax, fieldMax);
        _material.SetFloat(SmoothK, smoothK);

        if (_shapeBuffer != null)
        {
            _material.SetBuffer(Shapes, _shapeBuffer);
            _material.SetInt(ShapeCount, SDFShapeRenders.Count);
        }
    }
}