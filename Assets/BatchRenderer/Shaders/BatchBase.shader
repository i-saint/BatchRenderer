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
#pragma target 5.0
#pragma vertex vert
#pragma fragment frag
#pragma multi_compile_shadowcaster
#include "UnityCG.cginc"
#include "BatchRenderer.cginc"

struct appdata {
    float4 vertex : POSITION;
    float4 texcoord1 : TEXCOORD1;
};

struct v2f {
    V2F_SHADOW_CASTER;
    float kill : TEXCOORD5;
};

v2f vert( appdata v )
{
    int instance_id = ApplyInstanceTransform(v.vertex, v.texcoord1);

    v2f o;
    TRANSFER_SHADOW_CASTER(o)
    o.kill = instance_id >= GetNumInstances();
    return o;
}

float4 frag( v2f i ) : SV_Target
{
    if(i.kill!=0.0f) { discard; }
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
#pragma target 5.0
#pragma vertex vert
#pragma fragment frag
#pragma multi_compile_shadowcollector 

#define SHADOW_COLLECTOR_PASS
#include "UnityCG.cginc"
#include "BatchRenderer.cginc"

struct appdata {
    float4 vertex : POSITION;
    float4 texcoord1 : TEXCOORD1;
};

struct v2f { 
    V2F_SHADOW_COLLECTOR;
    float kill : TEXCOORD5;
};

v2f vert( appdata v )
{
    int instance_id = ApplyInstanceTransform(v.vertex, v.texcoord1);

    v2f o;
    TRANSFER_SHADOW_COLLECTOR(o)
    o.kill = instance_id >= GetNumInstances();
    return o;
}

fixed4 frag (v2f i) : SV_Target
{
    if(i.kill!=0.0f) { discard; }
    SHADOW_COLLECTOR_FRAGMENT(i)
}
ENDCG
    }

}
FallBack Off
}
