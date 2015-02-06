

void ApplyInstanceTransform(float2 id, inout float4 vertex, inout float3 normal, inout float2 texcoord, inout float4 color, inout float4 emission)
{
    int instance_id = GetBatchBegin() + id.x;
    if(instance_id >= GetNumInstances()) {
        vertex.xyz *= 0.0;
        return;
    }

    vertex.xyz *= GetBaseScale();
    if(GetFlag_Scale()) {
        vertex.xyz *= GetInstanceScale(instance_id);
    }
    if(GetFlag_Rotation()) {
        float3x3 rot = quaternion_to_matrix33(GetInstanceRotation(instance_id));
        vertex.xyz = mul(rot, vertex.xyz);
        normal = mul(rot, normal);
    }
    vertex.xyz += GetInstanceTranslation(instance_id);

    if(GetFlag_UVOffset()) {
        float4 u = GetInstanceUVOffset(instance_id);
        texcoord = texcoord*u.xy + u.zw;
    }
    if(GetFlag_Color()) {
        color *= GetInstanceColor(instance_id);
    }
    if(GetFlag_Emission()) {
        emission += GetInstanceEmission(instance_id);
    }
}
