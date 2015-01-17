Shader "BatchRenderer/Lambert" {
Properties {
    _Color ("Main Color", Color) = (1,1,1,1)
    _MainTex ("Base (RGB)", 2D) = "white" {}
}
SubShader {
    Tags { "RenderType"="BatchedOpaque" }
    LOD 200


CGPROGRAM
#pragma target 5.0
#pragma surface surf Lambert vertex:vert
#include "UnityCG.cginc"
#include "BatchRenderer.cginc"

struct Input {
    float2 uv_MainTex;
    float kill;
};

sampler2D _MainTex;
fixed4 _Color;

void vert (inout appdata_full v, out Input o)
{
    UNITY_INITIALIZE_OUTPUT(Input,o);

    int instance_id = ApplyInstanceTransform(v.vertex, v.texcoord1);

    o.uv_MainTex = v.texcoord;
    o.kill = instance_id >= GetNumInstances();
}

void surf (Input IN, inout SurfaceOutput o)
{
    if(IN.kill!=0.0f) { discard; }

    fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;
    o.Albedo = c.rgb;
    o.Alpha = c.a;
}
ENDCG
}

Fallback "BatchRenderer/BatchBase"
}
