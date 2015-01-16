Shader "BatchRenderer/Lambert" {
    Properties {
        _MainTex ("Base (RGB)", 2D) = "white" {}
    }
    SubShader {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGINCLUDE
        struct Input {
            float2 uv_MainTex;
            float kill;
        };

        sampler2D _MainTex;

#if defined(SHADER_API_D3D11) || defined(SHADER_API_PSSL)
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
#if defined(SHADER_API_D3D11) || defined(SHADER_API_PSSL)
            int instance_id = g_metadata[0].begin + id.x;
            float4x4 trans = g_entities[instance_id].trans;
            vertex = mul(trans, vertex);
            return instance_id;
#else
            return 0;
#endif
        }
        ENDCG


        CGPROGRAM
        #pragma surface surf Lambert vertex:vert
        #pragma target 5.0

        void vert (inout appdata_full v, out Input data)
        {
            UNITY_INITIALIZE_OUTPUT(Input,data);

            int instance_id = ApplyInstanceTransform(v.vertex, v.texcoord1);

            data.uv_MainTex = v.texcoord;
#if defined(SHADER_API_D3D11) || defined(SHADER_API_PSSL)
            data.kill = instance_id > g_metadata[0].num_instances;
#endif
        }

        void surf (Input IN, inout SurfaceOutput o)
        {
            if(IN.kill!=0.0f) { discard; }

            half4 c = tex2D (_MainTex, IN.uv_MainTex);
            o.Albedo = c.rgb;
            o.Alpha = c.a;
        }
        ENDCG


        Pass {
            Name "ShadowCaster"
            Tags { "LightMode" = "ShadowCaster" }
        
            Fog {Mode Off}
            ZWrite On
            ZTest LEqual
            Cull Off
            Offset 1, 1

        CGPROGRAM
// Upgrade NOTE: excluded shader from DX11 and Xbox360; has structs without semantics (struct v2f members kill)
#pragma exclude_renderers d3d11 xbox360
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 5.0
            #pragma multi_compile_shadowcaster
            #include "UnityCG.cginc"

            struct v2f { 
                V2F_SHADOW_CASTER;
                float kill;
            };

            v2f vert( appdata_full v )
            {
                int instance_id = ApplyInstanceTransform(v.vertex, v.texcoord2);

                v2f o;
                TRANSFER_SHADOW_CASTER(o)
#if defined(SHADER_API_D3D11) || defined(SHADER_API_PSSL)
                o.kill = instance_id > g_metadata[0].num_instances;
#endif
                return o;
            }

            float4 frag( v2f i ) : SV_Target
            {
                if(i.kill!=0.0f) { discard; }
                SHADOW_CASTER_FRAGMENT(i)
            }
        ENDCG
        }

        Pass {
            Name "ShadowCollector"
            Tags { "LightMode" = "ShadowCollector" }
        
            Fog {Mode Off}
            ZWrite On
            ZTest LEqual

        CGPROGRAM
// Upgrade NOTE: excluded shader from DX11 and Xbox360; has structs without semantics (struct v2f members kill)
#pragma exclude_renderers d3d11 xbox360
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 5.0
            #pragma multi_compile_shadowcollector
            #define SHADOW_COLLECTOR_PASS
            #include "UnityCG.cginc"

            struct v2f { 
                V2F_SHADOW_COLLECTOR;
                float kill;
            };

            v2f vert( appdata_full v )
            {
                int instance_id = ApplyInstanceTransform(v.vertex, v.texcoord2);
            
                v2f o;
                TRANSFER_SHADOW_COLLECTOR(o)
#if defined(SHADER_API_D3D11) || defined(SHADER_API_PSSL)
                o.kill = instance_id > g_metadata[0].num_instances;
#endif
                return o;
            }
        
            fixed4 frag (v2f i) : SV_Target
            {
                if(i.kill!=0.0f) { discard; }
                SHADOW_COLLECTOR_FRAGMENT(i)
            }
        ENDCG
        }
    }
    FallBack "Diffuse"
}