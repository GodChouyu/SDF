Shader "Custom/SDF"
{
    Properties
    {
        _MainTex ("Sprite Texture", 2D) = "white" {}
        gradient_texture ("Gradient Texture", 2D) = "white" {}

        field_min ("Field Min", Float) = -50
        field_max ("Field Max", Float) = 50
        smooth_k ("Smooth K", Float) = 5
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "RenderType"="Transparent"
            "IgnoreProjector"="True"
            "RenderPipeline"="UniversalPipeline"
        }

        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Off

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 4.5

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Assets/Shaders/SDFLib.hlsl"

            struct attributes
            {
                float4 position_os : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct varyings
            {
                float4 position_hcs : SV_POSITION;
                float2 uv : TEXCOORD0;
                float2 world_pos : TEXCOORD1;
            };

            struct sdf_shape
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

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            TEXTURE2D(gradient_texture);
            SAMPLER(sampler_gradient_texture);

            StructuredBuffer<sdf_shape> shapes;
            int shape_count;

            float field_min;
            float field_max;
            float smooth_k;

            varyings vert(attributes v)
            {
                varyings o;
                VertexPositionInputs pos = GetVertexPositionInputs(v.position_os.xyz);
                o.position_hcs = pos.positionCS;
                o.uv = v.uv;
                o.world_pos = pos.positionWS.xy;
                return o;
            }

            float eval_shape(float2 p, sdf_shape s)
            {
                if (s.shape_type == 0) // Circle
                {
                    return sdf_circle(p, s.center, s.radius);
                }
                if (s.shape_type == 1) // RoundedRect
                {
                    return sdf_round_rectangle(
                        p,
                        s.center,
                        s.size,
                        s.radius,
                        s.angle
                    );
                }
                if (s.shape_type == 2) // Triangle
                {
                    return sdf_triangle(p, s.center, s.v1, s.v2);
                }

                return 1e5;
            }

            float apply_op(float d, float new_d, int op)
            {
                if (op == 0) return sdf_union(d, new_d);
                if (op == 1) return sdf_subtract(d, new_d);
                if (op == 2) return sdf_intersect(d, new_d);
                if (op == 3) return sdf_smooth_union(d, new_d, smooth_k);

                return new_d;
            }

            half4 frag(varyings i) : SV_Target
            {
                // Sprite alpha（可选）
                half4 sprite_col = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv);
                if (sprite_col.a <= 0.001)
                    discard;

                float2 p = i.world_pos;

                float d = 1e5;

                for (int idx = 0; idx < shape_count; idx++)
                {
                    float sd = eval_shape(p, shapes[idx]);

                    if (idx == 0)
                        d = sd;
                    else
                        d = apply_op(d, sd, shapes[idx].operation);
                }

                // 场值映射到 0~1
                float t = saturate((d - field_min) / (field_max - field_min));

                half4 col = SAMPLE_TEXTURE2D(
                    gradient_texture,
                    sampler_gradient_texture,
                    float2(t, 0.5)
                );

                col.a *= sprite_col.a;
                return col;
            }
            ENDHLSL
        }
    }
}