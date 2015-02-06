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
#if defined(SHADER_API_OPENGL)
    #pragma glsl
#elif defined(SHADER_API_D3D9)
    #define WITHOUT_INSTANCE_COLOR
    #define WITHOUT_INSTANCE_EMISSION
    #pragma target 3.0
#endif
#pragma surface surf Lambert vertex:vert
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

void vert(inout appdata_full v, out Input o)
{
    UNITY_INITIALIZE_OUTPUT(Input,o);

    float4 color = v.color * g_base_color;
    float4 emission = g_base_emission;
    ApplyInstanceTransform(v.texcoord1.xy, v.vertex, v.normal, v.texcoord.xy, color, emission);

    o.uv_MainTex = v.texcoord.xy;
    o.color = color;
    o.emission = emission;
}

void surf(Input IN, inout SurfaceOutput o)
{
    fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * IN.color;
    o.Albedo = c.rgb;
    o.Alpha = c.a;
    o.Emission = IN.emission.xyz;
}
ENDCG
}

Fallback "BatchRenderer/BatchBase"
}
