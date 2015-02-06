Shader "BatchRenderer/FixedBillboard Alpha Blended" {
Properties {
    _MainTex ("Texture", 2D) = "white" {}
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
