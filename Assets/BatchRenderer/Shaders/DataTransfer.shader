Shader "BatchRenderer/DataTransfer" {

SubShader {
    ZTest Always
    ZWrite Off
    Cull Off

CGINCLUDE

struct ia_out
{
    float4 vertex : POSITION;
    float3 data3 : NORMAL;
    float4 data4 : TANGENT;
};

struct vs_out
{
    float4 vertex : SV_POSITION;
    float4 data : TEXCOORD0;
};

struct ps_out
{
    float4 color : COLOR0;
};

int g_begin;
float4 g_texel;

float2 InstanceIDToScreenPosition(float id)
{
    id += g_begin;
    float xi = fmod(id, g_texel.z);
    float yi = floor(id / g_texel.z);
    float2 pixel_size = g_texel.xy * 2.0;
    float2 pos = pixel_size * (float2(xi, yi) + 0.5) - 1.0;
    pos.y *= -1.0;
    return pos;
}

vs_out vert1(ia_out io)
{
    vs_out o;
    o.vertex = float4(InstanceIDToScreenPosition(io.vertex.x), 0.0, 1.0);
    o.data = io.data3.xyzz;
    return o;
}

vs_out vert2(ia_out io)
{
    vs_out o;
    o.vertex = float4(InstanceIDToScreenPosition(io.vertex.x), 0.0, 1.0);
    o.data = io.data4;
    return o;
}

ps_out frag(vs_out vo)
{
    ps_out po = { vo.data };
    return po;
}
ENDCG

    Pass {
        CGPROGRAM
        #pragma vertex vert1
        #pragma fragment frag
        #ifdef SHADER_API_OPENGL 
            #pragma glsl
        #endif
        ENDCG
    }

    Pass {
        CGPROGRAM
        #pragma vertex vert2
        #pragma fragment frag
        #ifdef SHADER_API_OPENGL 
            #pragma glsl
        #endif
        ENDCG
    }
}

}
