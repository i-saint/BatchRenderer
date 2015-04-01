Shader "BatchRenderer/BatchStandard" {
Properties {
    _Color ("Color", Color) = (1,1,1,1)
    _MainTex ("Albedo (RGB)", 2D) = "white" {}
    _Glossiness ("Smoothness", Range(0,1)) = 0.5
    _Metallic ("Metallic", Range(0,1)) = 0.0
}
SubShader {
    Tags { "RenderType"="Opaque" }
    LOD 200
        
    CGPROGRAM
#if defined(SHADER_API_OPENGL)
    #pragma glsl
#elif defined(SHADER_API_D3D9)
    #pragma target 3.0
    #define WITHOUT_INSTANCE_COLOR
    #define WITHOUT_INSTANCE_EMISSION
#else
    #pragma target 4.0
#endif
    #pragma surface stdsurf Standard fullforwardshadows vertex:vert
#include "UnityCG.cginc"
#include "BatchRenderer.cginc"
#include "Surface.cginc"

    half _Glossiness;
    half _Metallic;
    fixed4 _Color;

    void stdsurf(Input IN, inout SurfaceOutputStandard o)
    {
        fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;
        o.Albedo = c.rgb;
        o.Metallic = _Metallic;
        o.Smoothness = _Glossiness;
        o.Alpha = c.a;
    }
    ENDCG
} 
FallBack "Diffuse"
}
