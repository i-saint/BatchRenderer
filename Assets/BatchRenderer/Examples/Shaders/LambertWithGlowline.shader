Shader "BatchRendererExample/LambertWithGlowline" {
Properties {
    g_base_color ("Base Color", Color) = (1,1,1,1)
    g_base_emission ("Emission", Color) = (0,0,0,0)
    _MainTex ("Base (RGB)", 2D) = "white" {}
}
SubShader {
    Tags { "RenderType"="BatchedOpaque" }
    LOD 200

CGPROGRAM
#if defined(SHADER_API_OPENGL)
    #pragma glsl
#elif defined(SHADER_API_D3D9)
    #pragma target 3.0
#endif
#pragma surface surf Lambert vertex:vert
#include "UnityCG.cginc"
#include "../../Shaders/BatchRenderer.cginc"
#include "../../Shaders/Surface.cginc"

struct Input {
    float2 uv_MainTex;
    float3 vertex_position;
    float3 vertex_normal;
    float3 instance_position;
};

float4 g_base_color;


void vert (inout appdata_full v, out Input o)
{
    UNITY_INITIALIZE_OUTPUT(Input,o);

    int iid = GetInstanceID(v.texcoord1);
    o.vertex_position = v.vertex.xyz * GetInstanceScale(iid);
    o.vertex_normal = v.normal;

    float4 color = 0.0;
    float4 emission = 0.0;
    ApplyInstanceTransform(v.texcoord1.xy, v.vertex, v.normal, v.texcoord.xy, color, emission);

    o.uv_MainTex = v.texcoord.xy;
    o.instance_position = GetInstanceTranslation(iid);
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

void surf (Input IN, inout SurfaceOutput o)
{
    float4 line_color = float4(1.0, 1.0, 2.0, 0.0);
    float2 dg = boxcell(IN.vertex_position.xyz*0.05, IN.vertex_normal.xyz);
    float pc = 1.0-clamp(1.0 - max(min(dg.x, 2.0)-1.0, 0.0)*2.0, 0.0, 1.0);
    float d = -length(IN.vertex_position+IN.instance_position.xyz)*0.15 - dg.y*0.5;
    float vg = max(0.0, frac(1.0-d*0.75-_Time.y*0.25)*3.0-2.0) * pc;
    float4 emission = line_color * vg * 1.5;

    o.Albedo = g_base_color.rgb;
    o.Alpha = 1.0;
    o.Emission = emission;
}
ENDCG
}

Fallback "BatchRenderer/BatchBase"
}
