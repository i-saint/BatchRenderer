
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

v2f vert (appdata_t v)
{
    int instance_id = ApplyInstanceTransformBillboard(v.vertex, v.texcoord1);

    v2f o;
    o.vertex = mul(UNITY_MATRIX_VP, v.vertex);
    o.texcoord = v.texcoord;
    o.kill = instance_id >= GetNumInstances();
    return o;
}

fixed4 frag(v2f i) : SV_Target
{
    if(i.kill!=0.0f) { discard; }
    return tex2D(_MainTex, i.texcoord);
}
