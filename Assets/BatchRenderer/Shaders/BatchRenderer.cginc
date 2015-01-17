
#if defined(SHADER_API_D3D11) || defined(SHADER_API_PSSL)
    #define WITH_STRUCTURED_BUFFER
#endif

#ifdef WITH_STRUCTURED_BUFFER
struct MetaData
{
    int num_instances;
    int begin;
    int end;
    int pad;
};
struct EntityData
{
    float4x4 trans;
};
StructuredBuffer<MetaData> g_metadata;
StructuredBuffer<EntityData> g_entities;
#endif

int ApplyInstanceTransform(inout float4 vertex, float2 id)
{
#ifdef WITH_STRUCTURED_BUFFER
    int instance_id = g_metadata[0].begin + id.x;
    float4x4 trans = g_entities[instance_id].trans;
    vertex = mul(trans, vertex);
    return instance_id;
#else
    return 0;
#endif
}

int GetNumInstances()
{
#ifdef WITH_STRUCTURED_BUFFER
    return g_metadata[0].num_instances;
#else
    return 0;
#endif
}
