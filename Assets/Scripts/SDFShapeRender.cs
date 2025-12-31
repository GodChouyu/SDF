using UnityEngine;

public class SDFShapeRender : MonoBehaviour
{
    public Vector2 size; // 圆角矩形长款
    public float radius; // 圆、圆角矩形圆半径
    public float angle; // 圆角矩形旋转
    public Vector2 v1; // 三角形顶点
    public Vector2 v2; // 三角形顶点
    public ShapeType shapeType;
    public SDFOperation sdfOperation;

    public int renderOrder;

    public SDFShape SDFShape;


    private void OnEnable()
    {
        SDFRenderManager.SDFShapeRenders.Add(this);
        SDFRenderManager.SDFShapeRenders.Sort((left, right) => left.renderOrder.CompareTo(right.renderOrder));
    }

    private void Update()
    {
        SDFShape = new SDFShape
        {
            center = transform.position,
            size = size,
            radius = radius,
            angle = angle,
            v1 = (Vector2)transform.position + v1,
            v2 = (Vector2)transform.position + v2,
            shapeType = shapeType,
            SDFOperation = sdfOperation
        };
    }

    private void OnDisable()
    {
        SDFRenderManager.SDFShapeRenders.Remove(this);
    }
}