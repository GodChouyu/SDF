using System.Runtime.InteropServices;
using UnityEngine;

[StructLayout(LayoutKind.Sequential)]
public struct SDFShape
{
    public Vector2 center; // 圆、圆角矩形顶点 三角形顶点
    public Vector2 size; // 圆角矩形长款
    public float radius; // 圆、圆角矩形圆半径
    public float angle; // 圆角矩形旋转
    public Vector2 v1; // 三角形顶点
    public Vector2 v2; // 三角形顶点
    public ShapeType shapeType;
    public SDFOperation SDFOperation;
}