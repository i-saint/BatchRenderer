Shader "BatchRenderer/FixedBillboard Alpha Blended" {
Properties {
    _MainTex ("Texture", 2D) = "white" {}
    g_view_plane_distance ("View Plane Distance", Float) = 10.0
}

Category {
    Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }
    Blend SrcAlpha OneMinusSrcAlpha
    AlphaTest Greater .01
    ColorMask RGB
    Cull Off Lighting Off ZWrite Off ZTest Always 

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
