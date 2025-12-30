using System;
using System.Collections.Generic;
using UnityEngine;

public class SDFShapeRender : MonoBehaviour
{
    private static readonly List<SDFShapeRender> ShapeList = new();
    public static IReadOnlyList<SDFShapeRender> Shapes => ShapeList;

    public SDFShape sdfShape;

    private void Start()
    {
        ShapeList.Add(this);
    }

    private void OnDestroy()
    {
        ShapeList.Remove(this);
    }
}

[Serializable]
public struct SDFShape
{
    public Vector2 center; // 圆、圆角矩形顶点 三角形顶点
    public Vector2 size; // 圆角矩形长款
    public float radius; // 圆、圆角矩形圆半径
    public float angle; // 圆角矩形旋转
    public Vector2 v1; // 三角形顶点
    public Vector2 v2; // 三角形顶点
    public ShapeType shapeType;
    public Operation operation;
}

public enum ShapeType
{
    Circle = 0, // 圆
    RoundedRectangle = 1, // 圆角矩形
    Triangle = 2 // 三角形
}

public enum Operation
{
    Union = 0,
    Subtract = 1,
    Intersect = 2,
    SmoothUnion = 3,
}