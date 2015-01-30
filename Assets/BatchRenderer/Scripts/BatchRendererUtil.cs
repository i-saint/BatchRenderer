using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;

public static class BatchRendererUtil
{
    public static Vector4 ComputeUVOffset(Texture texture, Rect rect)
    {
        float tw = texture.width;
        float th = texture.height;
        return new Vector4(
            rect.width / tw,
            rect.height / th,
            rect.xMin / tw,
            (1.0f - rect.yMax) / th);
    }

    public static Vector4 ComputeUVOffset(int texture_width, int texture_height, Rect rect)
    {
        float tw = texture_width;
        float th = texture_height;
        return new Vector4(
            rect.width / tw,
            rect.height / th,
            rect.xMin / tw,
            (1.0f - rect.yMax) / th);
    }



    public static Mesh CreateExpandedMesh(Mesh mesh, out int instances_par_batch)
    {
        const int max_vertices = 65000; // Mesh's limitation
        Vector3[] vertices_base = mesh.vertices;
        Vector3[] normals_base = (mesh.normals == null || mesh.normals.Length == 0) ? null : mesh.normals;
        Vector2[] uv_base = (mesh.uv == null || mesh.uv.Length == 0) ? null : mesh.uv;
        Color[] colors_base = (mesh.colors == null || mesh.colors.Length == 0) ? null : mesh.colors;
        int[] indices_base = (mesh.triangles == null || mesh.triangles.Length == 0) ? null : mesh.triangles;
        instances_par_batch = max_vertices / mesh.vertexCount;

        Vector3[] vertices = new Vector3[vertices_base.Length * instances_par_batch];
        Vector2[] idata = new Vector2[vertices_base.Length * instances_par_batch];
        Vector3[] normals = normals_base == null ? null : new Vector3[normals_base.Length * instances_par_batch];
        Vector2[] uv = uv_base == null ? null : new Vector2[uv_base.Length * instances_par_batch];
        Color[] colors = colors_base == null ? null : new Color[colors_base.Length * instances_par_batch];
        int[] indices = indices_base == null ? null : new int[indices_base.Length * instances_par_batch];

        for (int ii = 0; ii < instances_par_batch; ++ii)
        {
            for (int vi = 0; vi < vertices_base.Length; ++vi)
            {
                int i = ii * vertices_base.Length + vi;
                vertices[i] = vertices_base[vi];
                idata[i] = new Vector2((float)ii, (float)vi);
            }
            if (normals != null)
            {
                for (int vi = 0; vi < normals_base.Length; ++vi)
                {
                    int i = ii * normals_base.Length + vi;
                    normals[i] = normals_base[vi];
                }
            }
            if (uv != null)
            {
                for (int vi = 0; vi < uv_base.Length; ++vi)
                {
                    int i = ii * uv_base.Length + vi;
                    uv[i] = uv_base[vi];
                }
            }
            if (colors != null)
            {
                for (int vi = 0; vi < colors_base.Length; ++vi)
                {
                    int i = ii * colors_base.Length + vi;
                    colors[i] = colors_base[vi];
                }
            }
            if (indices != null)
            {
                for (int vi = 0; vi < indices_base.Length; ++vi)
                {
                    int i = ii * indices_base.Length + vi;
                    indices[i] = ii * vertices_base.Length + indices_base[vi];
                }
            }

        }
        Mesh ret = new Mesh();
        ret.vertices = vertices;
        ret.normals = normals;
        ret.uv = uv;
        ret.colors = colors;
        ret.uv2 = idata;
        ret.triangles = indices;
        return ret;
    }
}

