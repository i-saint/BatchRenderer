

void ApplyBillboardTransform(float2 id, inout float4 vertex, inout float3 normal, inout float2 texcoord, inout float4 color)
{
#ifdef WITH_STRUCTURED_BUFFER
    int instance_id = GetBatchBegin() + id.x;
    if(instance_id >= GetNumInstances()) {
        vertex.xyz *= 0.0;
        return;
    }

    int data_flags = GetDataFlags();
    float3 up = float3(0.0, 1.0, 0.0);
    float3 camera_pos = _WorldSpaceCameraPos.xyz;
    float3 pos = GetInstanceTranslation(instance_id);
    float3 look = normalize(camera_pos-pos);

    vertex.xyz *= GetBaseScale();
    if(data_flags & DataFlag_Scale) {
        vertex.xyz *= GetInstanceScale(instance_id);
    }
    vertex = mul(look_matrix(look, up), vertex);
    if(data_flags & DataFlag_Rotation) {
        float4x4 rot = quaternion_to_matrix(GetInstanceRotation(instance_id));
        vertex = mul(rot, vertex);
        normal = mul(rot, float4(normal, 0.0)).xyz;
    }
    vertex.xyz += pos;
    vertex = mul(UNITY_MATRIX_VP, vertex);

    if(data_flags & DataFlag_UVOffset) {
        float4 u = GetInstanceUVOffset(instance_id);
        texcoord = texcoord*u.xy + u.zw;
    }
    if(data_flags & DataFlag_Color) {
        color *= GetInstanceColor(instance_id);
    }
#else
    vertex.xyz *= 0.0;
#endif
}


bool ApplyViewPlaneProjection(inout float4 vertex, float3 pos)
{
    float4 vp = mul(UNITY_MATRIX_VP, float4(pos, 1.0));
    if(vp.z<0.0) {
        vertex.xyz *= 0.0;
        return false;
    }

    float aspect = _ScreenParams.x / _ScreenParams.y;
    float3 camera_pos = _WorldSpaceCameraPos.xyz;
    float3 look = normalize(camera_pos-pos);
    Plane view_plane = {look, 1.0};
    pos = camera_pos + project_to_plane(view_plane, pos-camera_pos);
    vertex.y *= -aspect;
    vertex.xy += vp.xy / vp.w;
    vertex.zw = float2(0.0, 1.0);
    return true;
}

void ApplyViewPlaneBillboardTransform(float2 id, inout float4 vertex, inout float3 normal, inout float2 texcoord, inout float4 color)
{
#ifdef WITH_STRUCTURED_BUFFER
    int instance_id = GetBatchBegin() + id.x;
    if(instance_id >= GetNumInstances()) {
        vertex.xyz *= 0.0;
        return;
    }

    int data_flags = GetDataFlags();
    float3 pos = GetInstanceTranslation(instance_id);
    vertex.xyz *= GetBaseScale();
    if(data_flags & DataFlag_Scale) {
        vertex.xyz *= GetInstanceScale(instance_id);
    }
    if(data_flags & DataFlag_Rotation) {
        float4x4 rot = quaternion_to_matrix(GetInstanceRotation(instance_id));
        vertex = mul(rot, vertex);
        normal = mul(rot, float4(normal, 0.0)).xyz;
    }
    if(!ApplyViewPlaneProjection(vertex, pos)) {
        return;
    }

    if(data_flags & DataFlag_UVOffset) {
        float4 u = GetInstanceUVOffset(instance_id);
        texcoord = texcoord*u.xy + u.zw;
    }
    if(data_flags & DataFlag_Color) {
        color *= GetInstanceColor(instance_id);
    }
#else
    vertex.xyz *= 0.0;
#endif
}



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
};

v2f vert(appdata_t v)
{
    float4 color = 1.0;
    ApplyBillboardTransform(v.texcoord1, v.vertex, v.normal, v.texcoord, color);

    v2f o;
    o.vertex = v.vertex;
    o.texcoord = v.texcoord;
    o.color = color;
    return o;
}

v2f vert_fixed(appdata_t v)
{
    float4 color = 1.0;
    ApplyViewPlaneBillboardTransform(v.texcoord1, v.vertex, v.normal, v.texcoord, color);

    v2f o;
    o.vertex = v.vertex;
    o.texcoord = v.texcoord;
    o.color = color;
    return o;
}

float4 frag(v2f i) : SV_Target
{
    float4 color = tex2D(_MainTex, i.texcoord) * i.color;
    return color;
}
