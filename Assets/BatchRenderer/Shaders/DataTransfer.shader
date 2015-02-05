Shader "BatchRenderer/DataTransfer" {

SubShader {
    ZTest Always
    ZWrite Off
    Cull Off

CGINCLUDE

struct ia_out
{
    float4 vertex : POSITION;
    float4 data : COLOR;
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

int base_index;

vs_out vert(ia_out io)
{
    float2 pixel_size = (_ScreenParams.zw - 1.0) * 2.0;
    int i = base_index + (int)io.vertex.x;
    int xi = i % (int)_ScreenParams.x;
    int yi = i / (int)_ScreenParams.y;
    float2 pos = pixel_size * (float2(xi, yi) + 0.5) - 1.0;

    vs_out o;
    o.vertex = float4(pos, 0.0, 1.0);
    o.data = io.data;
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
        #pragma vertex vert
        #pragma fragment frag
        #ifdef SHADER_API_OPENGL 
            #pragma glsl
        #endif
        ENDCG
    }
}

}
