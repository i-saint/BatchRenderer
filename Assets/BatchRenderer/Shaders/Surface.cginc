

void ApplyInstanceTransform(float2 id, inout float4 vertex, inout float3 normal, inout float4 tangent, inout float2 texcoord, inout float4 color, inout float4 emission)
{
    int instance_id = GetBatchBegin() + id.x;
    if(instance_id >= GetNumInstances()) {
        vertex.xyz *= 0.0;
        return;
    }

    vertex.xyz *= GetBaseScale();
#ifndef WITHOUT_INSTANCE_SCALE
    if(GetFlag_Scale()) {
        vertex.xyz *= GetInstanceScale(instance_id);
    }
#endif
#ifndef WITHOUT_INSTANCE_ROTATION
    if(GetFlag_Rotation()) {
        float3x3 rot = quaternion_to_matrix33(GetInstanceRotation(instance_id));
        vertex.xyz = mul(rot, vertex.xyz);
        normal.xyz = mul(rot, normal.xyz);
        tangent.xyz = mul(rot, tangent.xyz);
    }
#endif
    vertex.xyz += GetInstanceTranslation(instance_id);

#ifndef WITHOUT_INSTANCE_UVOFFSET
    if(GetFlag_UVOffset()) {
        float4 u = GetInstanceUVOffset(instance_id);
        texcoord = texcoord*u.xy + u.zw;
    }
#endif
#ifndef WITHOUT_INSTANCE_COLOR
    if(GetFlag_Color()) {
        color *= GetInstanceColor(instance_id);
    }
#endif
#ifndef WITHOUT_INSTANCE_EMISSION
    if(GetFlag_Emission()) {
        emission += GetInstanceEmission(instance_id);
    }
#endif
}


#ifndef WITHOUT_COMMON_VERT_SURF
sampler2D _MainTex;
sampler2D _NormalMap;
sampler2D _EmissionMap;
sampler2D _SpecularMap;
sampler2D _GrossMap;
float4 g_base_color;
float4 g_base_emission;

struct Input {
    float2 uv_MainTex;
    float4 color;
    float4 emission;
};

void vert(inout appdata_full v, out Input o)
{
    UNITY_INITIALIZE_OUTPUT(Input,o);

    float4 color = v.color * g_base_color;
    float4 emission = g_base_emission;
    ApplyInstanceTransform(v.texcoord1.xy, v.vertex, v.normal, v.tangent, v.texcoord.xy, color, emission);

    o.uv_MainTex = v.texcoord.xy;
    o.color = color;
    o.emission = emission;
}

void surf(Input IN, inout SurfaceOutput o)
{
    fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * IN.color;
    o.Albedo = c.rgb;
    o.Alpha = c.a;
    o.Emission = IN.emission.xyz;
}

void surf_detailed(Input IN, inout SurfaceOutput o)
{
    fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * IN.color;
    o.Albedo = c.rgb;
    o.Alpha = c.a;
    o.Normal = tex2D(_NormalMap, IN.uv_MainTex).xyz;
    o.Emission = g_base_emission + tex2D(_EmissionMap, IN.uv_MainTex).xyz;
    o.Specular *= tex2D(_SpecularMap, IN.uv_MainTex).x;
    o.Gloss *= tex2D(_GrossMap, IN.uv_MainTex).x;
}
#endif // WITHOUT_COMMON_VERT_SURF
