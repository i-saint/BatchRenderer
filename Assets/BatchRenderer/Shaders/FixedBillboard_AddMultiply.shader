Shader "BatchRenderer/FixedBillboard Add Multiply" {
Properties {
    _MainTex ("Texture", 2D) = "white" {}
}

Category {
    Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }
    Blend One OneMinusSrcAlpha
    ColorMask RGB
    Cull Off Lighting Off ZWrite Off ZTest Always Fog { Color (0,0,0,1) }

    SubShader {
        Pass {
CGPROGRAM
#pragma vertex vert_fixed
#pragma fragment frag
#ifdef SHADER_API_OPENGL
    #pragma glsl
#endif

#include "UnityCG.cginc"
#include "BatchRenderer.cginc"
#include "Billboard.cginc"
ENDCG
        }
    }
}
}
