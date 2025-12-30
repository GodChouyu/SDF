float sdf_circle(float2 p, float2 center, float radius)
{
    return length(p - center) - radius;
}

// 圆角矩形
// 参数：
// p      - 当前像素点坐标（像素空间或 UV）
// center - 矩形中心坐标
// size   - 矩形尺寸（width, height）
// radius - 圆角半径
// angle  - 旋转角度（弧度）
// 返回值：
// float  - signed distance
float sdf_round_rectangle(float2 p, float2 center, float2 size, float radius, float angle)
{
    // 1. 将点平移到矩形中心
    float2 local = p - center;

    // 2. 旋转点到矩形局部坐标系
    // 旋转矩阵: [ cosθ  sinθ ]
    //          [-sinθ  cosθ ]
    float cos_a = cos(angle);
    float sin_a = sin(angle);
    float2 rotated;
    rotated.x = cos_a * local.x + sin_a * local.y;
    rotated.y = -sin_a * local.x + cos_a * local.y;

    // 3. 计算矩形局部坐标到边界的向量
    // 使用对称性: size / 2 表示半宽半高
    float2 d = abs(rotated) - size * 0.5;

    // 4. 计算圆角 SDF
    // max(d,0) 表示外部距离
    float2 d_max = max(d, 0.0); // 每个方向大于0的部分
    float outside_dist = length(d_max); // 圆角外部距离
    float inside_dist = min(max(d.x, d.y), 0.0); // 内部距离（负值）

    // 最终距离 = 外部距离 + 内部负值距离
    float dist = outside_dist + inside_dist - radius;

    return dist;
}

// 点到线段的最短距离
// p - 点坐标
// a - 线段起点
// b - 线段终点
float point_to_line_segment_distance(float2 p, float2 a, float2 b)
{
    float2 ab = b - a; // 线段向量
    float2 ap = p - a; // 点到起点向量
    float t = dot(ap, ab) / dot(ab, ab); // 投影系数
    t = clamp(t, 0.0, 1.0); // 限制在线段上
    float2 closest = a + t * ab; // 最近点坐标
    return length(p - closest); // 点到线段距离
}

// 三角形
// p   - 当前像素点
// v0~v2 - 三角形顶点坐标
float sdf_triangle(float2 p, float2 v0, float2 v1, float2 v2)
{
    // 1. 计算点到每条边的距离
    float d0 = point_to_line_segment_distance(p, v0, v1);
    float d1 = point_to_line_segment_distance(p, v1, v2);
    float d2 = point_to_line_segment_distance(p, v2, v0);

    float dist = min(d0, min(d1, d2)); // 最近的边距离

    // 2. 判断点是否在三角形内部
    // 使用面积法（符号）
    float2 e0 = v1 - v0;
    float2 e1 = v2 - v1;
    float2 e2 = v0 - v2;

    float2 v0_p = p - v0;
    float2 v1_p = p - v1;
    float2 v2_p = p - v2;

    float c0 = e0.x * v0_p.y - e0.y * v0_p.x;
    float c1 = e1.x * v1_p.y - e1.y * v1_p.x;
    float c2 = e2.x * v2_p.y - e2.y * v2_p.x;

    // 如果三个符号相同，点在三角形内部
    if ((c0 >= 0 && c1 >= 0 && c2 >= 0) || (c0 <= 0 && c1 <= 0 && c2 <= 0))
    {
        dist = -dist; // 内部距离为负
    }

    return dist;
}

// 并集
float sdf_union(float d1, float d2)
{
    return min(d1, d2);
}

// 减去 d2
float sdf_subtract(float d1, float d2)
{
    return max(d1, -d2);
}

// 交集
float sdf_intersect(float d1, float d2)
{
    return max(d1, d2);
}

// 平滑并集
// k - 控制平滑程度，k>0，越大越尖锐，越小越柔和
float sdf_smooth_union(float d1, float d2, float k)
{
    float h = clamp(0.5 + 0.5 * (d2 - d1) / k, 0.0, 1.0);
    return lerp(d2, d1, h) - k * h * (1.0 - h);
}
