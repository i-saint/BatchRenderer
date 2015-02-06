Shader "BatchRenderer/Lambert" {
Properties {
    g_base_color ("Base Color", Color) = (1,1,1,1)
    g_base_emission ("Emission", Color) = (0,0,0,0)
    _MainTex ("Base (RGB)", 2D) = "white" {}
}
SubShader {
    Tags { "RenderType"="BatchedOpaque" }
    LOD 200

CGPROGRAM
#pragma surface surf Lambert vertex:vert
#ifdef SHADER_API_OPENGL
    #pragma glsl
#endif
#include "UnityCG.cginc"
#include "BatchRenderer.cginc"
#include "Surface.cginc"

struct Input {
    float2 uv_MainTex;
    float4 color;
    float4 emission;
};

sampler2D _MainTex;
float4 g_base_color;
float4 g_base_emission;

void vert (inout appdata_full v, out Input o)
{
    UNITY_INITIALIZE_OUTPUT(Input,o);

    float4 color = v.color * g_base_color;
    float4 emission = g_base_emission;
    ApplyInstanceTransform(v.texcoord1, v.vertex, v.normal, v.texcoord.xy, color, emission);

    o.uv_MainTex = v.texcoord;
    o.color = color;
    o.emission = emission;
}

void surf (Input IN, inout SurfaceOutput o)
{
    fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * IN.color;
    o.Albedo = c.rgb;
    o.Alpha = c.a;
    o.Emission = IN.emission;
}
ENDCG
}

Fallback "BatchRenderer/BatchBase"
}
