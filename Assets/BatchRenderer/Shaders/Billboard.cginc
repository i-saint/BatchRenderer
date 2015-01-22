


sampler2D _MainTex;

struct appdata_t {
    float4 vertex : POSITION;
    float3 normal : NORMAL;
    float2 texcoord : TEXCOORD0;
    float2 texcoord1 : TEXCOORD1;
};

struct v2f {
    float4 vertex : SV_POSITION;
    float2 texcoord : TEXCOORD0;
    float4 color : TEXCOORD1;
    float kill : TEXCOORD2;
};

v2f vert(appdata_t v)
{
    float4 color = 1.0;
    float k = ApplyBillboardTransform(v.texcoord1, v.vertex, v.normal, v.texcoord, color);

    v2f o;
    o.vertex = v.vertex;
    o.texcoord = v.texcoord;
    o.color = color;
    o.kill = k;
    return o;
}

v2f vert_fixed(appdata_t v)
{
    float4 color = 1.0;
    float k = ApplyViewPlaneBillboardTransform(v.texcoord1, v.vertex, v.normal, v.texcoord, color);

    v2f o;
    o.vertex = v.vertex;
    o.texcoord = v.texcoord;
    o.color = color;
    o.kill = k;
    return o;
}

float4 frag(v2f i) : SV_Target
{
    if(i.kill!=0.0f) { discard; }
    float4 color = tex2D(_MainTex, i.texcoord) * i.color;
    return color;
}
