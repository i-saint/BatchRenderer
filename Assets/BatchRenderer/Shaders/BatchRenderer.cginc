#ifdef SHADER_API_PSSL
#   define COLOR  SV_Target
#   define COLOR0 SV_Target0
#   define COLOR1 SV_Target1
#   define COLOR2 SV_Target2
#   define COLOR3 SV_Target3
#   define DEPTH SV_Depth
#endif

#if defined(SHADER_API_D3D11) || defined(SHADER_API_PSSL)
    #define WITH_STRUCTURED_BUFFER
#endif



// utilities


float4x4 look_matrix(float3 dir, float3 up)
{
    float3 z = dir;
    float3 x = cross(up, z);
    float3 y = cross(z, x);
    return float4x4(
        x.x, y.x, z.x, 0.0,
        x.y, y.y, z.y, 0.0,
        x.z, y.z, z.z, 0.0,
        0.0, 0.0, 0.0, 1.0 );
}

float4x4 axis_rotation_matrix(float3 axis, float angle)
{
    axis = normalize(axis);
    float s = sin(angle);
    float c = cos(angle);
    float oc = 1.0 - c;
    
    return float4x4(
        oc * axis.x * axis.x + c,           oc * axis.x * axis.y - axis.z * s,  oc * axis.z * axis.x + axis.y * s,  0.0,
        oc * axis.x * axis.y + axis.z * s,  oc * axis.y * axis.y + c,           oc * axis.y * axis.z - axis.x * s,  0.0,
        oc * axis.z * axis.x - axis.y * s,  oc * axis.y * axis.z + axis.x * s,  oc * axis.z * axis.z + c,           0.0,
        0.0,                                0.0,                                0.0,                                1.0);
}

float4x4 quaternion_to_matrix(float4 q)
{
    return float4x4(
        1.0-2.0*q.y*q.y - 2.0*q.z*q.z,  2.0*q.x*q.y - 2.0*q.z*q.w,          2.0*q.x*q.z + 2.0*q.y*q.w,          0.0,
        2.0*q.x*q.y + 2.0*q.z*q.w,      1.0 - 2.0*q.x*q.x - 2.0*q.z*q.z,    2.0*q.y*q.z - 2.0*q.x*q.w,          0.0,
        2.0*q.x*q.z - 2.0*q.y*q.w,      2.0*q.y*q.z + 2.0*q.x*q.w,          1.0 - 2.0*q.x*q.x - 2.0*q.y*q.y,    0.0,
        0.0,                            0.0,                                0.0,                                1.0 );
}

float3 extract_position(float4x4 m)
{
    return float3(m[0][3], m[1][3], m[2][3]);
}



struct Plane
{
    float3 normal;
    float distance;
};

struct Ray
{
    float3 origin;
    float3 direction;
};

float point_plane_distance(Plane plane, float3 pos)
{
    return dot(pos, plane.normal) + plane.distance;
}

float3 ray_plane_intersection(Plane plane, Ray ray)
{
    float t = (-dot(ray.origin, plane.normal) - plane.distance) / dot(plane.normal, ray.direction);
    return ray.origin + ray.direction * t;
}

float3 project_to_plane(Plane plane, float3 pos)
{
    float d = point_plane_distance(plane, pos);
    return pos - d*plane.normal;
}



#ifdef WITH_STRUCTURED_BUFFER
struct DrawData
{
    int data_flags;
    int num_instances;
    float3 scale;
    float2 uv_scale;
};

struct BatchData
{
    int begin;
    int end;
};



StructuredBuffer<DrawData>      g_draw_data;
StructuredBuffer<BatchData>     g_batch_data;
StructuredBuffer<float3>        g_instance_t;
StructuredBuffer<float4>        g_instance_r;
StructuredBuffer<float3>        g_instance_s;
StructuredBuffer<float2>        g_instance_uv;
#endif

float ApplyInstanceTransform(inout float4 vertex, inout float3 normal, inout float2 texcoord, float2 id)
{
#ifdef WITH_STRUCTURED_BUFFER
    int instance_id = g_batch_data[0].begin + id.x;
    if(instance_id >= g_draw_data[0].num_instances) {
        return 1.0;
    }

    int data_flags = g_draw_data[0].data_flags;

    vertex.xyz *= g_draw_data[0].scale;
    if(data_flags & (1<<2)) {
        vertex.xyz *= g_instance_s[instance_id];
    }
    if(data_flags & (1<<1)) {
        float4x4 rot = quaternion_to_matrix(g_instance_r[instance_id]);
        vertex = mul(rot, vertex);
        normal = mul(rot, normal);
    }
    vertex.xyz += g_instance_t[instance_id];

    texcoord *= g_draw_data[0].uv_scale;
    if(data_flags & (1<<3)) {
        texcoord += g_instance_uv[instance_id];
    }
    return 0.0;
#else
    return 1.0;
#endif
}


float ApplyBillboardTransform(inout float4 vertex, inout float3 normal, inout float2 texcoord, float2 id)
{
#ifdef WITH_STRUCTURED_BUFFER
    int instance_id = g_batch_data[0].begin + id.x;
    if(instance_id >= g_draw_data[0].num_instances) {
        return 1.0;
    }

    int data_flags = g_draw_data[0].data_flags;
    float3 up = float3(0.0, 1.0, 0.0);
    float3 camera_pos = _WorldSpaceCameraPos.xyz;
    float3 pos = g_instance_t[instance_id];
    float3 look = normalize(camera_pos-pos);

    vertex.xyz *= g_draw_data[0].scale;
    if(data_flags & (1<<2)) {
        vertex.xyz *= g_instance_s[instance_id];
    }
    vertex = mul(look_matrix(look, up), vertex);
    if(data_flags & (1<<1)) {
        float4x4 rot = quaternion_to_matrix(g_instance_r[instance_id]);
        vertex = mul(rot, vertex);
        normal = mul(rot, normal);
    }
    vertex.xyz += pos;
    vertex = mul(UNITY_MATRIX_VP, vertex);

    texcoord *= g_draw_data[0].uv_scale;
    if(data_flags & (1<<3)) {
        texcoord += g_instance_uv[instance_id];
    }
    return 0.0;
#else
    return 1.0;
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

float ApplyViewPlaneBillboardTransform(inout float4 vertex, inout float3 normal, inout float2 texcoord, float2 id)
{
#ifdef WITH_STRUCTURED_BUFFER
    int instance_id = g_batch_data[0].begin + id.x;
    if(instance_id >= g_draw_data[0].num_instances) {
        return 1.0;
    }

    int data_flags = g_draw_data[0].data_flags;
    float3 pos = g_instance_t[instance_id];
    vertex.xyz *= g_draw_data[0].scale;
    if(data_flags & (1<<2)) {
        vertex.xyz *= g_instance_s[instance_id];
    }
    if(data_flags & (1<<1)) {
        float4x4 rot = quaternion_to_matrix(g_instance_r[instance_id]);
        vertex = mul(rot, vertex);
        normal = mul(rot, normal);
    }
    ApplyViewPlaneProjection(vertex, pos);

    texcoord *= g_draw_data[0].uv_scale;
    if(data_flags & (1<<3)) {
        texcoord += g_instance_uv[instance_id];
    }
    return 0.0;
#else
    return 1.0;
#endif
}
