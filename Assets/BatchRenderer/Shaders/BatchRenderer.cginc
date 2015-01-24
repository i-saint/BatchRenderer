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

float3x3 look_matrix33(float3 dir, float3 up)
{
    float3 z = dir;
    float3 x = cross(up, z);
    float3 y = cross(z, x);
    return float3x3(
        x.x, y.x, z.x,
        x.y, y.y, z.y,
        x.z, y.z, z.z );
}

float4x4 look_matrix44(float3 dir, float3 up)
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

float3x3 axis_rotation_matrix33(float3 axis, float angle)
{
    axis = normalize(axis);
    float s = sin(angle);
    float c = cos(angle);
    float oc = 1.0 - c;
    return float3x3(
        oc * axis.x * axis.x + c,           oc * axis.x * axis.y - axis.z * s,  oc * axis.z * axis.x + axis.y * s,
        oc * axis.x * axis.y + axis.z * s,  oc * axis.y * axis.y + c,           oc * axis.y * axis.z - axis.x * s,
        oc * axis.z * axis.x - axis.y * s,  oc * axis.y * axis.z + axis.x * s,  oc * axis.z * axis.z + c          );
}
float4x4 axis_rotation_matrix44(float3 axis, float angle)
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

float3x3 quaternion_to_matrix33(float4 q)
{
    return float3x3(
        1.0-2.0*q.y*q.y - 2.0*q.z*q.z,  2.0*q.x*q.y - 2.0*q.z*q.w,          2.0*q.x*q.z + 2.0*q.y*q.w,      
        2.0*q.x*q.y + 2.0*q.z*q.w,      1.0 - 2.0*q.x*q.x - 2.0*q.z*q.z,    2.0*q.y*q.z - 2.0*q.x*q.w,      
        2.0*q.x*q.z - 2.0*q.y*q.w,      2.0*q.y*q.z + 2.0*q.x*q.w,          1.0 - 2.0*q.x*q.x - 2.0*q.y*q.y );
}
float4x4 quaternion_to_matrix44(float4 q)
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



#define DataFlag_Translation (1 << 0)
#define DataFlag_Rotation    (1 << 1)
#define DataFlag_Scale       (1 << 2)
#define DataFlag_Color       (1 << 3)
#define DataFlag_Emission    (1 << 4)
#define DataFlag_UVOffset    (1 << 5)

#ifdef WITH_STRUCTURED_BUFFER
struct DrawData
{
    int data_flags;
    int num_max_instances;
    int num_instances;
    float3 scale;
};

struct BatchData
{
    int begin;
    int end;
};

StructuredBuffer<DrawData>      g_draw_data;
StructuredBuffer<BatchData>     g_batch_data;
StructuredBuffer<float3>        g_instance_buffer_t;
StructuredBuffer<float4>        g_instance_buffer_r;
StructuredBuffer<float3>        g_instance_buffer_s;
StructuredBuffer<float4>        g_instance_buffer_color;
StructuredBuffer<float4>        g_instance_buffer_emission;
StructuredBuffer<float4>        g_instance_buffer_uv;

int     GetDataFlags()          { return g_draw_data[0].data_flags; }
int     GetNumMaxInstances()    { return g_draw_data[0].num_max_instances; }
int     GetNumInstances()       { return g_draw_data[0].num_instances; }
float3  GetBaseScale()          { return g_draw_data[0].scale; }
int     GetBatchBegin()         { return g_batch_data[0].begin; }
int     GetBatchEnd()           { return g_batch_data[0].end; }
int     GetInstanceID(float2 i) { return i.x + GetBatchBegin(); }
float3  GetInstanceTranslation(int i)   { return g_instance_buffer_t[i]; }
float4  GetInstanceRotation(int i)      { return g_instance_buffer_r[i]; }
float3  GetInstanceScale(int i)         { return g_instance_buffer_s[i]; }
float4  GetInstanceColor(int i)         { return g_instance_buffer_color[i]; }
float4  GetInstanceEmission(int i)      { return g_instance_buffer_emission[i]; }
float4  GetInstanceUVOffset(int i)      { return g_instance_buffer_uv[i]; }

#else // WITH_STRUCTURED_BUFFER

sampler2D g_instance_texture_t;
sampler2D g_instance_texture_r;
sampler2D g_instance_texture_s;
sampler2D g_instance_texture_color;
sampler2D g_instance_texture_emission;
sampler2D g_instance_texture_uv;

/* todo
int     GetDataFlags()          {  }
int     GetNumMaxInstances()    {  }
int     GetNumInstances()       {  }
float3  GetBaseScale()          {  }
int     GetBatchBegin()         {  }
int     GetBatchEnd()           {  }
float3  GetInstanceTranslation(int i)   {  }
float4  GetInstanceRotation(int i)      {  }
float3  GetInstanceScale(int i)         {  }
float4  GetInstanceColor(int i)         {  }
float4  GetInstanceEmission(int i)      {  }
float4  GetInstanceUVOffset(int i)      {  }
*/

#endif // WITH_STRUCTURED_BUFFER
