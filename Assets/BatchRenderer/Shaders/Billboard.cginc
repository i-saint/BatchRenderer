


sampler2D _MainTex;

struct appdata_t {
    float4 vertex : POSITION;
    float2 texcoord : TEXCOORD0;
    float2 texcoord1 : TEXCOORD1;
};

struct v2f {
    float4 vertex : SV_POSITION;
    float2 texcoord : TEXCOORD0;
    float kill : TEXCOORD1;
};

v2f vert(appdata_t v)
{
    float k = ApplyBillboardTransform(v.vertex, v.texcoord, v.texcoord1);

    v2f o;
    o.vertex = v.vertex;
    o.texcoord = v.texcoord;
    o.kill = k;
    return o;
}

v2f vert_fixed(appdata_t v)
{
    float k = ApplyViewPlaneBillboardTransform(v.vertex, v.texcoord, v.texcoord1);

    v2f o;
    o.vertex = v.vertex;
    o.texcoord = v.texcoord;
    o.kill = k;
    return o;
}

float4 frag(v2f i) : SV_Target
{
    if(i.kill!=0.0f) { discard; }
    float4 color = tex2D(_MainTex, i.texcoord);
    return color;
}
