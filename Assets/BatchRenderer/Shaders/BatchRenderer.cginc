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

#ifdef WITH_STRUCTURED_BUFFER
struct BatchData
{
    int num_instances;
    int begin;
    int end;
    int data_type;
};
StructuredBuffer<BatchData> g_batch_data;
StructuredBuffer<float3> g_instance_positions;
StructuredBuffer<float4x4> g_instance_matrices;
#endif

int ApplyInstanceTransform(inout float4 vertex, float2 id)
{
#ifdef WITH_STRUCTURED_BUFFER
    int instance_id = g_batch_data[0].begin + id.x;
    int data_type = g_batch_data[0].data_type;
    if(data_type==0) {
        vertex = mul(g_instance_matrices[instance_id], vertex);
    }
    else {
        vertex.xyz += g_instance_positions[instance_id];
    }
    return instance_id;
#else
    return 0;
#endif
}

int GetNumInstances()
{
#ifdef WITH_STRUCTURED_BUFFER
    return g_batch_data[0].num_instances;
#else
    return 0;
#endif
}
