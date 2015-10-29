Shader "BatchRendererExample/GlowlineSurface" {
Properties {
    g_base_color ("Base Color", Color) = (1,1,1,1)
    _Emission ("Emission", Color) = (0,0,0,0)
    _MainTex ("Base (RGB)", 2D) = "white" {}
}
SubShader {
    Tags { "RenderType"="BatchedOpaque" }
    LOD 200

CGPROGRAM
#if defined(SHADER_API_D3D9)
    #pragma target 3.0
#endif
#define WITHOUT_INSTANCE_COLOR
#define WITHOUT_INSTANCE_EMISSION

#pragma multi_compile ___ ENABLE_INSTANCE_BUFFER

#pragma enable_d3d11_debug_symbols
#pragma surface surf Standard vertex:vert
#include "UnityCG.cginc"
#include "Assets/Ist/BatchRenderer/Shaders/BatchRenderer.cginc"
#define WITHOUT_COMMON_VERT_SURF
#include "Assets/Ist/BatchRenderer/Shaders/Surface.cginc"



struct Input {
    float2 uv_MainTex;
    float3 vertex_position;
    float3 vertex_normal;
    float3 instance_position;
};

float4 g_base_color;


void vert (inout appdata_full I, out Input O)
{
    UNITY_INITIALIZE_OUTPUT(Input, O);

    int iid = GetInstanceID(I.texcoord1);
    O.vertex_position = I.vertex.xyz;
    O.vertex_normal = I.normal;

    float4 color = 0.0;
    float4 emission = 0.0;
    ApplyInstanceTransform(I.texcoord1.xy, I.vertex, I.normal, I.tangent, I.texcoord.xy, color, emission);

    O.uv_MainTex = I.texcoord.xy;
    O.instance_position = GetInstanceTranslation(iid);
}

float2 boxcell(float3 p3, float3 n)
{
    float2 p;
    float t = 0.7;
    if(abs(n.x)>t) {
        p = p3.yz;
    }
    else if(abs(n.z)>t) {
        p = p3.xy;
    }
    else {
        p = p3.xz;
    }

    p = frac(p);
    float r = 0.123;
    float v = 0.0, g = 0.0;
    r = frac(r * 9184.928);
    float cp, d;

    d = p.x;
    g += pow(clamp(1.0 - abs(d), 0.0, 1.0), 1000.0);
    d = p.y;
    g += pow(clamp(1.0 - abs(d), 0.0, 1.0), 1000.0);
    d = p.x - 1.0;
    g += pow(clamp(3.0 - abs(d), 0.0, 1.0), 1000.0);
    d = p.y - 1.0;
    g += pow(clamp(1.0 - abs(d), 0.0, 1.0), 10000.0);

    const int iter = 11;
    for(int i = 0; i < iter; i ++)
    {
        cp = 0.5 + (r - 0.5) * 0.9;
        d = p.x - cp;
        //g += pow(clamp(1.0 - abs(d), 0.0, 1.0), 200.0);
        g += clamp(1.0 - abs(d), 0.0, 1.0) > 0.999-(0.00075*i) ? 1.0 : 0.0;
        if(d > 0.0) {
            r = frac(r * 4829.013);
            p.x = (p.x - cp) / (1.0 - cp);
            v += 1.0;
        }
        else {
            r = frac(r * 1239.528);
            p.x = p.x / cp;
        }
        p = p.yx;
    }
    v /= float(iter);
    return float2(g, v);
}

void surf (Input I, inout SurfaceOutputStandard O)
{
    float4 line_color = float4(0.5, 0.5, 1.0, 0.0);
    float2 dg = boxcell(I.vertex_position.xyz*0.05, I.vertex_normal.xyz);
    float pc = 1.0-clamp(1.0 - max(min(dg.x, 2.0)-1.0, 0.0)*2.0, 0.0, 1.0);
    float d = -length(I.vertex_position + I.instance_position.xyz)*0.15 - dg.y*0.5;
    float vg = max(0.0, frac(1.0-d*0.75-_Time.y*0.25)*3.0-2.0) * pc;
    float4 emission = line_color * vg * 1.5;

    O.Albedo = g_base_color.rgb;
    O.Alpha = 1.0;
    O.Emission = emission;
}
ENDCG
}

Fallback "BatchRenderer/BatchBase"
}
