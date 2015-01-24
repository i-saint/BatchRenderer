using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;


public class BatchRenderer : MonoBehaviour
{

    public void AddInstanceT(Vector3 t)
    {
        int i = Interlocked.Increment(ref m_instance_count) - 1;
        if (i < m_max_instances)
        {
            m_instance_data.translation[i] = t;
        }
    }
    public void AddInstancesT(Vector3[] t, int start = 0, int length = 0)
    {
        if (length == 0) length = t.Length;
        int reserved_index;
        int reserved_num;
        ReserveInstance(length, out reserved_index, out reserved_num);
        System.Array.Copy(t, start, m_instance_data.translation, reserved_index, reserved_num);
    }

    public void AddInstanceTR(Vector3 t, Quaternion r)
    {
        int i = Interlocked.Increment(ref m_instance_count) - 1;
        if (i < m_max_instances)
        {
            m_instance_data.translation[i] = t;
            m_instance_data.rotation[i] = r;
        }
    }
    public void AddInstancesTR(Vector3[] t, Quaternion[] r, int start = 0, int length = 0)
    {
        if (length == 0) length = t.Length;
        int reserved_index;
        int reserved_num;
        ReserveInstance(length, out reserved_index, out reserved_num);
        System.Array.Copy(t, start, m_instance_data.translation, reserved_index, reserved_num);
        System.Array.Copy(r, start, m_instance_data.rotation, reserved_index, reserved_num);
    }

    public void AddInstanceTRS(Vector3 t, Quaternion r, Vector3 s)
    {
        int i = Interlocked.Increment(ref m_instance_count) - 1;
        if (i < m_max_instances)
        {
            m_instance_data.translation[i] = t;
            m_instance_data.rotation[i] = r;
            m_instance_data.scale[i] = s;
        }
    }
    public void AddInstancesTRS(Vector3[] t, Quaternion[] r, Vector3[] s, int start = 0, int length = 0)
    {
        if (length == 0) length = t.Length;
        int reserved_index;
        int reserved_num;
        ReserveInstance(length, out reserved_index, out reserved_num);
        System.Array.Copy(t, start, m_instance_data.translation, reserved_index, reserved_num);
        System.Array.Copy(r, start, m_instance_data.rotation, reserved_index, reserved_num);
        System.Array.Copy(s, start, m_instance_data.scale, reserved_index, reserved_num);
    }

    public void AddInstanceTRSC(Vector3 t, Quaternion r, Vector3 s, Color c)
    {
        int i = Interlocked.Increment(ref m_instance_count) - 1;
        if (i < m_max_instances)
        {
            m_instance_data.translation[i] = t;
            m_instance_data.rotation[i] = r;
            m_instance_data.scale[i] = s;
            m_instance_data.color[i] = c;
        }
    }

    public void AddInstanceTRSCE(Vector3 t, Quaternion r, Vector3 s, Color c, Color e)
    {
        int i = Interlocked.Increment(ref m_instance_count) - 1;
        if (i < m_max_instances)
        {
            m_instance_data.translation[i] = t;
            m_instance_data.rotation[i] = r;
            m_instance_data.scale[i] = s;
            m_instance_data.color[i] = c;
            m_instance_data.emission[i] = e;
        }
    }

    public void AddInstanceTRC(Vector3 t, Quaternion r, Color c)
    {
        int i = Interlocked.Increment(ref m_instance_count) - 1;
        if (i < m_max_instances)
        {
            m_instance_data.translation[i] = t;
            m_instance_data.rotation[i] = r;
            m_instance_data.color[i] = c;
        }
    }

    public void AddInstanceTRU(Vector3 t, Quaternion r, Vector4 uv)
    {
        int i = Interlocked.Increment(ref m_instance_count) - 1;
        if (i < m_max_instances)
        {
            m_instance_data.translation[i] = t;
            m_instance_data.rotation[i] = r;
            m_instance_data.uv_offset[i] = uv;
        }
    }

    public void AddInstanceTRCU(Vector3 t, Quaternion r, Color c, Vector4 uv)
    {
        int i = Interlocked.Increment(ref m_instance_count) - 1;
        if (i < m_max_instances)
        {
            m_instance_data.translation[i] = t;
            m_instance_data.rotation[i] = r;
            m_instance_data.color[i] = c;
            m_instance_data.uv_offset[i] = uv;
        }
    }

    public void AddInstanceTRSCU(Vector3 t, Quaternion r, Vector3 s, Color c, Vector4 uv)
    {
        int i = Interlocked.Increment(ref m_instance_count) - 1;
        if (i < m_max_instances)
        {
            m_instance_data.translation[i] = t;
            m_instance_data.rotation[i] = r;
            m_instance_data.scale[i] = s;
            m_instance_data.color[i] = c;
            m_instance_data.uv_offset[i] = uv;
        }
    }


    public InstanceData ReserveInstance(int num, out int reserved_index, out int reserved_num)
    {
        reserved_index = Interlocked.Add(ref m_instance_count, num) - num;
        reserved_num = Mathf.Clamp(m_max_instances - reserved_index, 0, num);
        return m_instance_data;
    }


    public enum DataFlags
    {
        Translation = 1 << 0,
        Rotation = 1 << 1,
        Scale = 1 << 2,
        Color = 1 << 3,
        Emission = 1 << 4,
        UVOffset = 1 << 5,
    }

    [System.Serializable]
    public struct DrawData
    {
        public const int size = 24;

        public int data_flags;
        public int num_max_instances;
        public int num_instances;
        public Vector3 scale;
    }

    [System.Serializable]
    public struct BatchData
    {
        public const int size = 8;

        public int begin;
        public int end;
    }

    [System.Serializable]
    public class InstanceData
    {
        public Vector3[] translation;
        public Quaternion[] rotation;
        public Vector3[] scale;
        public Color[] color;
        public Color[] emission;
        public Vector4[] uv_offset;

        public void Resize(int size)
        {
            translation = new Vector3[size];
            rotation = new Quaternion[size];
            scale = new Vector3[size];
            color = new Color[size];
            emission = new Color[size];
            uv_offset = new Vector4[size];

            Vector3 default_scale = Vector3.one;
            Color default_color = Color.white;
            Vector4 default_uvoffset = new Vector4(1.0f, 1.0f, 0.0f, 0.0f);
            for (int i = 0; i < scale.Length; ++i) { scale[i] = default_scale; }
            for (int i = 0; i < color.Length; ++i) { color[i] = default_color; }
            for (int i = 0; i < uv_offset.Length; ++i) { uv_offset[i] = default_uvoffset; }
        }
    }

    public class InstanceBuffer
    {
        public ComputeBuffer translation;
        public ComputeBuffer rotation;
        public ComputeBuffer scale;
        public ComputeBuffer color;
        public ComputeBuffer emission;
        public ComputeBuffer uv_offset;

        public void Release()
        {
            if (translation != null){ translation.Release(); translation = null; }
            if (rotation != null)   { rotation.Release(); rotation = null; }
            if (scale != null)      { scale.Release(); scale = null; }
            if (color != null)      { color.Release(); color = null; }
            if (emission != null)   { emission.Release(); emission = null; }
            if (uv_offset != null)  { uv_offset.Release(); uv_offset = null; }
        }

        public void Allocate(int num_max_instances)
        {
            Release();
            translation = new ComputeBuffer(num_max_instances, 12);
            rotation = new ComputeBuffer(num_max_instances, 16);
            scale = new ComputeBuffer(num_max_instances, 12);
            color = new ComputeBuffer(num_max_instances, 16);
            emission = new ComputeBuffer(num_max_instances, 16);
            uv_offset = new ComputeBuffer(num_max_instances, 16);
        }
    }

    // I will need this when I make OpenGL implementation
    public class InstanceTexture
    {
        const int texture_width = 1024;

        public RenderTexture translation;
        public RenderTexture rotation;
        public RenderTexture scale;
        public RenderTexture color;
        public RenderTexture emission;
        public RenderTexture uv_offset;

        public void Release()
        {
            if (translation != null) { translation.Release(); translation = null; }
            if (rotation != null) { rotation.Release(); rotation = null; }
            if (scale != null) { scale.Release(); scale = null; }
            if (color != null) { color.Release(); color = null; }
            if (emission != null) { emission.Release(); emission = null; }
            if (uv_offset != null) { uv_offset.Release(); uv_offset = null; }
        }

        RenderTexture CreateDataTexture(int num_max_instances)
        {
            int width = texture_width;
            int height = num_max_instances / texture_width + (num_max_instances % texture_width != 0 ? 1 : 0);
            var r = new RenderTexture(width, height, 0, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Default);
            r.filterMode = FilterMode.Point;
            r.Create();
            return r;
        }

        public void Allocate(int num_max_instances)
        {
            Release();
            translation = CreateDataTexture(num_max_instances);
            rotation = CreateDataTexture(num_max_instances);
            scale = CreateDataTexture(num_max_instances);
            color = CreateDataTexture(num_max_instances);
            emission = CreateDataTexture(num_max_instances);
            uv_offset = CreateDataTexture(num_max_instances);
        }
    }



    public bool m_enable_rotation;
    public bool m_enable_scale;
    public bool m_enable_color;
    public bool m_enable_emission;
    public bool m_enable_uv_offset;

    public int m_max_instances = 1024 * 16;
    public Mesh m_mesh;
    public Material m_material;
    public LayerMask m_layer_selector = 1;
    public bool m_cast_shadow = false;
    public bool m_receive_shadow = false;
    public Vector3 m_scale = Vector3.one;
    public Camera m_camera;
    public bool m_flush_on_LateUpdate = true;
    public bool m_update_buffers_externally = false;

    int m_instances_par_batch;
    int m_instance_count;
    int m_layer;
    Transform m_trans;
    Mesh m_expanded_mesh;
    ComputeBuffer m_draw_data_buffer;
    DrawData[] m_draw_data = new DrawData[1];
    List<ComputeBuffer> m_batch_data_buffers;
    List<Material> m_materials;

    InstanceData m_instance_data;
    InstanceBuffer m_instance_buffer;
    InstanceTexture m_instance_texture;


    public InstanceBuffer GetInstanceBuffer() { return m_instance_buffer; }
    public InstanceTexture GetInstanceTexture() { return m_instance_texture; }
    public int GetMaxInstanceCount() { return m_max_instances; }
    public int GetInstanceCount() { return m_instance_count; }
    public void SetInstanceCount(int v) { m_instance_count = v; }

    public void Flush()
    {
        if (m_mesh == null || m_instance_count==0)
        {
            m_instance_count = 0;
            return;
        }

        m_expanded_mesh.bounds = new Bounds(m_trans.position, m_trans.localScale);
        m_instance_count = m_update_buffers_externally ? m_max_instances : Mathf.Min(m_instance_count, m_max_instances);
        int num_batches = (m_max_instances / m_instances_par_batch) + (m_max_instances % m_instances_par_batch != 0 ? 1 : 0);

        if (!m_update_buffers_externally)
        {
            UpdateBuffers();
        }

        while (m_batch_data_buffers.Count < num_batches)
        {
            int i = m_batch_data_buffers.Count;
            ComputeBuffer batch_data_buffer = new ComputeBuffer(1, BatchData.size);
            BatchData[] batch_data = new BatchData[1];
            batch_data[0].begin = i * m_instances_par_batch;
            batch_data[0].end = (i + 1) * m_instances_par_batch;
            batch_data_buffer.SetData(batch_data);

            Material m = new Material(m_material);
            m.SetBuffer("g_draw_data", m_draw_data_buffer);
            m.SetBuffer("g_batch_data", batch_data_buffer);
            m.SetBuffer("g_instance_buffer_t", m_instance_buffer.translation);
            m.SetBuffer("g_instance_buffer_r", m_instance_buffer.rotation);
            m.SetBuffer("g_instance_buffer_s", m_instance_buffer.scale);
            m.SetBuffer("g_instance_buffer_color", m_instance_buffer.color);
            m.SetBuffer("g_instance_buffer_emission", m_instance_buffer.emission);
            m.SetBuffer("g_instance_buffer_uv", m_instance_buffer.uv_offset);
            // fix rendering order for transparent objects
            if (m.renderQueue >= 3000)
            {
                m.renderQueue = m.renderQueue + (i + 1);
            }
            m_materials.Add(m);
            m_batch_data_buffers.Add(batch_data_buffer);
        }

        Matrix4x4 identity = Matrix4x4.identity;
        for (int i = 0; i * m_instances_par_batch < m_instance_count; ++i)
        {
            Graphics.DrawMesh(m_expanded_mesh, identity, m_materials[i], m_layer, m_camera, 0, null, m_cast_shadow, m_receive_shadow);
        }
        m_instance_count = 0;
    }


    const int max_vertices = 65000;

    public static Mesh CreateExpandedMesh(Mesh mesh)
    {
        Vector3[] vertices_base = mesh.vertices;
        Vector3[] normals_base = (mesh.normals == null || mesh.normals.Length == 0) ? null : mesh.normals;
        Vector2[] uv_base = (mesh.uv == null || mesh.uv.Length == 0) ? null : mesh.uv;
        Color[] colors_base = (mesh.colors == null || mesh.colors.Length == 0) ? null : mesh.colors;
        int[] indices_base = (mesh.triangles==null || mesh.triangles.Length==0) ? null : mesh.triangles;
        int instances_par_batch = max_vertices / mesh.vertexCount;

        Vector3[] vertices = new Vector3[vertices_base.Length * instances_par_batch];
        Vector2[] idata = new Vector2[vertices_base.Length * instances_par_batch];
        Vector3[] normals = normals_base == null ? null : new Vector3[normals_base.Length * instances_par_batch];
        Vector2[] uv = uv_base == null ? null : new Vector2[uv_base.Length * instances_par_batch];
        Color[] colors = colors_base == null ? null : new Color[colors_base.Length * instances_par_batch];
        int[] indices = indices_base == null ? null : new int[indices_base.Length * instances_par_batch];

        for(int ii=0; ii<instances_par_batch; ++ii) {
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

    void ReleaseBuffers()
    {
        if (m_draw_data_buffer != null) { m_draw_data_buffer.Release(); m_draw_data_buffer = null; }
        m_instance_buffer.Release();
        m_batch_data_buffers.ForEach((e) => { e.Release(); });
        m_batch_data_buffers.Clear();
        m_materials.Clear();
    }

    void ResetBuffers()
    {
        ReleaseBuffers();

        m_instance_data.Resize(m_max_instances);
        m_draw_data_buffer = new ComputeBuffer(1, DrawData.size);
        m_instance_buffer.Allocate(m_max_instances);

        // set default values
        UpdateBuffers();
    }

    public void UpdateBuffers()
    {
        int data_flags = (int)DataFlags.Translation;
        m_instance_buffer.translation.SetData(m_instance_data.translation);
        if (m_enable_rotation)
        {
            data_flags |= (int)DataFlags.Rotation;
            m_instance_buffer.rotation.SetData(m_instance_data.rotation);
        }
        if (m_enable_scale)
        {
            data_flags |= (int)DataFlags.Scale;
            m_instance_buffer.scale.SetData(m_instance_data.scale);
        }
        if (m_enable_color)
        {
            data_flags |= (int)DataFlags.Color;
            m_instance_buffer.color.SetData(m_instance_data.color);
        }
        if (m_enable_emission)
        {
            data_flags |= (int)DataFlags.Emission;
            m_instance_buffer.emission.SetData(m_instance_data.emission);
        }
        if (m_enable_uv_offset)
        {
            data_flags |= (int)DataFlags.UVOffset;
            m_instance_buffer.uv_offset.SetData(m_instance_data.uv_offset);
        }

        m_draw_data[0].data_flags = data_flags;
        m_draw_data[0].num_instances = m_instance_count;
        m_draw_data[0].scale = m_scale;
        m_draw_data_buffer.SetData(m_draw_data);
    }


    void OnEnable()
    {
        if (m_mesh == null) return;

        m_trans = GetComponent<Transform>();
        m_batch_data_buffers = new List<ComputeBuffer>();
        m_materials = new List<Material>();
        m_instance_data = new InstanceData();
        m_instance_buffer = new InstanceBuffer();
        m_instance_texture = null;

        m_instances_par_batch = max_vertices / m_mesh.vertexCount;
        m_expanded_mesh = CreateExpandedMesh(m_mesh);
        m_expanded_mesh.UploadMeshData(true);

        int layer_mask = m_layer_selector.value;
        for (int i = 0; i < 32; ++i )
        {
            if ((layer_mask & (1<<i)) != 0)
            {
                m_layer = i;
                m_layer_selector.value = 1 << i;
                break;
            }
        }

        ResetBuffers();
    }

    void OnDisable()
    {
        ReleaseBuffers();
    }

    void LateUpdate()
    {
        if (m_flush_on_LateUpdate)
        {
            Flush();
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(transform.position, transform.localScale);
    }
}
