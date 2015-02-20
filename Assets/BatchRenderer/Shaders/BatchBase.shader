Shader "BatchRenderer/BatchBase" {
SubShader {
    Tags { "RenderType"="Opaque" }
    LOD 200
        

    Pass {
        Name "ShadowCaster"
        Tags { "LightMode" = "ShadowCaster" }

        Fog {Mode Off}
        ZWrite On ZTest LEqual Cull Off
        Offset 1, 1

CGPROGRAM
#if defined(SHADER_API_OPENGL)
    #pragma glsl
#elif defined(SHADER_API_D3D9)
    #pragma target 3.0
    #define WITHOUT_INSTANCE_COLOR
    #define WITHOUT_INSTANCE_EMISSION
#endif
#pragma vertex vert
#pragma fragment frag
#pragma multi_compile_shadowcaster
#include "UnityCG.cginc"
#include "BatchRenderer.cginc"
#define WITHOUT_COMMON_VERT_SURF
#include "Surface.cginc"

struct appdata {
    float4 vertex : POSITION;
    float3 normal : NORMAL;
    float4 tangent : TANGENT;
    float2 texcoord : TEXCOORD0;
    float2 texcoord1 : TEXCOORD1;
};

struct v2f {
    V2F_SHADOW_CASTER;
};

v2f vert( appdata v )
{
    float4 color_dummy = 0.0;
    float4 emission_dummy = 0.0;
    ApplyInstanceTransform(v.texcoord1.xy, v.vertex, v.normal, v.tangent, v.texcoord, color_dummy, emission_dummy);

    v2f o;
    TRANSFER_SHADOW_CASTER(o)
    return o;
}

float4 frag( v2f i ) : SV_Target
{
    SHADOW_CASTER_FRAGMENT(i)
}
ENDCG
    }


    Pass {
        Name "ShadowCollector"
        Tags { "LightMode" = "ShadowCollector" }

        Fog {Mode Off}
        ZWrite On ZTest LEqual

CGPROGRAM
#if defined(SHADER_API_OPENGL)
    #pragma glsl
#elif defined(SHADER_API_D3D9)
    #pragma target 3.0
    #define WITHOUT_INSTANCE_COLOR
    #define WITHOUT_INSTANCE_EMISSION
#endif
#pragma vertex vert
#pragma fragment frag
#pragma multi_compile_shadowcollector 

#define SHADOW_COLLECTOR_PASS
#include "UnityCG.cginc"
#include "BatchRenderer.cginc"
#define WITHOUT_COMMON_VERT_SURF
#include "Surface.cginc"

struct appdata {
    float4 vertex : POSITION;
    float3 normal : NORMAL;
    float4 tangent : TANGENT;
    float2 texcoord : TEXCOORD0;
    float2 texcoord1 : TEXCOORD1;
};

struct v2f { 
    V2F_SHADOW_COLLECTOR;
};

v2f vert( appdata v )
{
    float4 color_dummy = 0.0;
    float4 emission_dummy = 0.0;
    ApplyInstanceTransform(v.texcoord1.xy, v.vertex, v.normal, v.tangent, v.texcoord, color_dummy, emission_dummy);

    v2f o;
    TRANSFER_SHADOW_COLLECTOR(o)
    return o;
}

fixed4 frag (v2f i) : SV_Target
{
    SHADOW_COLLECTOR_FRAGMENT(i)
}
ENDCG
    }

}
FallBack Off
}
