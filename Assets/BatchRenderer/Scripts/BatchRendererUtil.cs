using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;

public static class BatchRendererUtil
{
    public static Vector4 ComputeUVOsset(Texture texture, Rect rect)
    {
        float tw = texture.width;
        float th = texture.height;
        return new Vector4(
            rect.width / tw,
            rect.height / th,
            rect.xMin / tw,
            (1.0f - rect.yMax) / th);
    }

    public static Vector4 ComputeUVOsset(int texture_width, int texture_height, Rect rect)
    {
        float tw = texture_width;
        float th = texture_height;
        return new Vector4(
            rect.width / tw,
            rect.height / th,
            rect.xMin / tw,
            (1.0f - rect.yMax) / th);
    }
}

