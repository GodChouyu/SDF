Shader "Custom/SDF"
{
    SubShader
    {
        Tags
        {
            "RenderType"="Opaque" "Queue"="Background"
        }
        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Assets/Shaders/SDFLib.hlsl"

            struct attributes
            {
                float4 position_os : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct varyings
            {
                float4 position_cs : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            // 顶点函数
            varyings vert(attributes v)
            {
                varyings o;
                o.position_cs = TransformObjectToHClip(v.position_os.xyz);
                o.uv = v.uv;
                return o;
            }

            // Shape 数据结构
            struct shape
            {
                float2 center;
                float2 size;
                float radius;
                float angle;
                float2 v1;
                float2 v2;
                int shape_type;
                int operation;
            };

            // 最大支持形状数量
            #define MAX_SHAPES 64
            StructuredBuffer<shape> shapes;
            int shape_count;

            float compute_shape_sdf(float2 uv, shape shape)
            {
                float d = 100000.0; // 初始大值
                if (shape.shape_type == 0) d = sdf_circle(uv, shape.center, shape.radius);
                else if (shape.shape_type == 1)
                    d = sdf_round_rectangle(uv, shape.center, shape.size, shape.radius,
                                            shape.angle);
                else if (shape.shape_type == 2) d = sdf_triangle(uv, shape.center, shape.v1, shape.v2);
                return d;
            }

            float CombineSDF(float d1, float d2, int op)
            {
                if (op == 0) return sdf_union(d1, d2);
                if (op == 1) return sdf_subtract(d1, d2);
                if (op == 2) return sdf_intersect(d1, d2);
                if (op == 3) return sdf_smooth_union(d1, d2, 10.0);
                // k=10,可调
                return d2;
            }

            // 片段函数
            half4 frag(varyings i) : SV_Target
            {
                float2 uv = i.uv; // 屏幕空间
                float dist = 100000.0; // 初始场值

                for (int s = 0; s < shape_count; s++)
                {
                    float d = compute_shape_sdf(uv, shapes[s]);
                    dist = CombineSDF(dist, d, shapes[s].operation);
                }

                return float4(dist, 0, 0, 1); // 输出 RFloat，G/B/A 填充无意义
            }
            ENDHLSL
        }
    }
    FallBack Off
}