Shader "BatchRenderer/FixedBillboard Add Multiply" {
Properties {
    _MainTex ("Texture", 2D) = "white" {}
    g_view_plane_distance ("View Plane Distance", Float) = 10.0
}

Category {
    Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }
    Blend One OneMinusSrcAlpha
    ColorMask RGB
    Cull Off Lighting Off ZWrite Off ZTest Always Fog { Color (0,0,0,1) }

    SubShader {
        Pass {
CGPROGRAM
#pragma target 5.0
#pragma vertex vert_fixed
#pragma fragment frag

#include "UnityCG.cginc"
#include "BatchRenderer.cginc"
#include "Billboard.cginc"
ENDCG
        }
    }
}
}
