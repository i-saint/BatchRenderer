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



#ifdef WITH_STRUCTURED_BUFFER
struct DrawData
{
    int data_type;
    int num_instances;
    float3 scale;
};

struct BatchData
{
    int begin;
    int end;
};

struct TR
{
    float3 translation;
    float4 rotation; // quaternion
};

StructuredBuffer<DrawData>  g_draw_data;
StructuredBuffer<BatchData> g_batch_data;
StructuredBuffer<float3>    g_instance_t;
StructuredBuffer<TR>        g_instance_tr;
StructuredBuffer<float4x4>  g_instance_trs;
#endif

int ApplyInstanceTransform(inout float4 vertex, float2 id)
{
#ifdef WITH_STRUCTURED_BUFFER
    int data_type = g_draw_data[0].data_type;
    int instance_id = g_batch_data[0].begin + id.x;
    vertex.xyz *= g_draw_data[0].scale;
    if(data_type==0) {
        vertex.xyz += g_instance_t[instance_id];
    }
    else if(data_type==1) {
        vertex = mul(quaternion_to_matrix(g_instance_tr[instance_id].rotation), vertex);
        vertex.xyz += g_instance_tr[instance_id].translation;
    }
    else if(data_type==2) {
        vertex = mul(g_instance_trs[instance_id], vertex);
    }
    return instance_id;
#else
    return 0;
#endif
}

int GetNumInstances()
{
#ifdef WITH_STRUCTURED_BUFFER
    return g_draw_data[0].num_instances;
#else
    return 0;
#endif
}
