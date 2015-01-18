

int ApplyBillboardTransform(inout float4 vertex, float2 id)
{
#ifdef WITH_STRUCTURED_BUFFER
    int data_type = g_draw_data[0].data_type;
    int instance_id = g_batch_data[0].begin + id.x;
    vertex.xyz *= g_draw_data[0].scale;

    float3 up = float3(0.0, 1.0, 0.0);
    float3 camera_pos = _WorldSpaceCameraPos.xyz;
    if(data_type==0) {
        float3 pos = g_instance_t[instance_id].translation;
        float3 look = normalize(camera_pos-pos);
        vertex = mul(look_matrix(look, up), vertex);
        vertex.xyz += pos;
    }
    else if(data_type==1) {
        float3 pos = g_instance_tr[instance_id].translation;
        float3 look = normalize(camera_pos-pos);
        vertex = mul(look_matrix(look, up), vertex);
        vertex = mul(quaternion_to_matrix(g_instance_tr[instance_id].rotation), vertex);
        vertex.xyz += pos;
    }
    else if(data_type==2) {
        float3 pos = g_instance_trs[instance_id].translation;
        float3 look = normalize(camera_pos-pos);
        vertex = mul(look_matrix(look, up), vertex);
        vertex.xyz *= g_instance_trs[instance_id].scale;
        vertex = mul(quaternion_to_matrix(g_instance_trs[instance_id].rotation), vertex);
        vertex.xyz += pos;
    }
    else if(data_type==3) {
        float3 pos = extract_position(g_instance_matrix[instance_id]);
        float3 look = normalize(camera_pos-pos);
        vertex = mul(look_matrix(look, up), vertex);
        vertex = mul(g_instance_matrix[instance_id], vertex);
    }
    vertex = mul(UNITY_MATRIX_VP, vertex);
    return instance_id;
#else
    return 0;
#endif
}


float g_view_plane_distance;

void ApplyViewPlaneProjection(inout float4 vertex, float3 pos)
{
    float aspect = _ScreenParams.x / _ScreenParams.y;
    float3 camera_pos = _WorldSpaceCameraPos.xyz;
    float3 look = normalize(camera_pos-pos);
    Plane view_plane = {look, g_view_plane_distance};
    pos = camera_pos + project_to_plane(view_plane, pos-camera_pos);
    float4 vp = mul(UNITY_MATRIX_VP, float4(pos, 1.0));
    vertex.x /= aspect;
    vertex.y *= -1.0;
    vertex.xy += vp.xy;
    vertex.zw = vp.zw;
}

int ApplyViewPlaneBillboardTransform(inout float4 vertex, float2 id)
{
#ifdef WITH_STRUCTURED_BUFFER
    int data_type = g_draw_data[0].data_type;
    int instance_id = g_batch_data[0].begin + id.x;
    vertex.xyz *= g_draw_data[0].scale;

    if(data_type==0) {
        float3 pos = g_instance_t[instance_id].translation;
        ApplyViewPlaneProjection(vertex, pos);
    }
    else if(data_type==1) {
        float3 pos = g_instance_tr[instance_id].translation;
        vertex = mul(quaternion_to_matrix(g_instance_tr[instance_id].rotation), vertex);
        ApplyViewPlaneProjection(vertex, pos);
    }
    else if(data_type==2) {
        float3 pos = g_instance_trs[instance_id].translation;
        vertex.xyz *= g_instance_trs[instance_id].scale;
        vertex = mul(quaternion_to_matrix(g_instance_trs[instance_id].rotation), vertex);
        ApplyViewPlaneProjection(vertex, pos);
    }
    else if(data_type==3) {
        float3 pos = extract_position(g_instance_matrix[instance_id]);
        vertex = mul(g_instance_matrix[instance_id], vertex);
        vertex.xyz -= pos;
        ApplyViewPlaneProjection(vertex, pos);
    }
    return instance_id;
#else
    return 0;
#endif
}



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
    int instance_id = ApplyBillboardTransform(v.vertex, v.texcoord1);

    v2f o;
    o.vertex = v.vertex;
    o.texcoord = v.texcoord;
    o.kill = instance_id >= GetNumInstances();
    return o;
}

v2f vert_fixed(appdata_t v)
{
    int instance_id = ApplyViewPlaneBillboardTransform(v.vertex, v.texcoord1);

    v2f o;
    o.vertex = v.vertex;
    o.texcoord = v.texcoord;
    o.kill = instance_id >= GetNumInstances();
    return o;
}

float4 frag(v2f i) : SV_Target
{
    if(i.kill!=0.0f) { discard; }
    float4 color = tex2D(_MainTex, i.texcoord);
    return color;
}
