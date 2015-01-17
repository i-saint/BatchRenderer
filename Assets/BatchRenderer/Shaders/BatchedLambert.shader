Shader "BatchRenderer/Lambert" {
Properties {
    _Color ("Main Color", Color) = (1,1,1,1)
    _MainTex ("Base (RGB)", 2D) = "white" {}
}
SubShader {
    Tags { "RenderType"="BatchedOpaque" }
    LOD 200


CGPROGRAM
#pragma target 5.0
#pragma surface surf Lambert vertex:vert
#include "UnityCG.cginc"
#include "BatchRenderer.cginc"

struct Input {
    float2 uv_MainTex;
    float kill;
};

sampler2D _MainTex;
fixed4 _Color;

void vert (inout appdata_full v, out Input o)
{
    UNITY_INITIALIZE_OUTPUT(Input,o);

    int instance_id = ApplyInstanceTransform(v.vertex, v.texcoord1);

    o.uv_MainTex = v.texcoord;
    o.kill = instance_id >= GetNumInstances();
}

void surf (Input IN, inout SurfaceOutput o)
{
    if(IN.kill!=0.0f) { discard; }

    fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;
    o.Albedo = c.rgb;
    o.Alpha = c.a;
}
ENDCG


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
    //float kill : TEXCOORD5;
};

v2f vert( appdata v )
{
    int instance_id = ApplyInstanceTransform(v.vertex, v.texcoord1);

    v2f o;
    TRANSFER_SHADOW_CASTER(o)
    //o.kill = instance_id >= GetNumInstances();
    return o;
}

float4 frag( v2f i ) : SV_Target
{
    //if(i.kill!=0.0f) { discard; }
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
Fallback Off
}