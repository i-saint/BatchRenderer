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

    public InstanceData ReserveInstance(int num, out int reserved_index, out int reserved_num)
    {
        reserved_index = Interlocked.Add(ref m_instance_count, num) - num;
        reserved_num = Mathf.Clamp(m_max_instances - reserved_index, 0, num);
        return m_instance_data;
    }


    [System.Serializable]
    public struct DrawData
    {
        public const int size = 28;

        public int data_flags;
        public int num_instances;
        public Vector3 scale;
        public Vector2 uv_scale;
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
        public Vector2[] uv_scroll;

        public void Resize(int size)
        {
            translation = new Vector3[size];
            rotation = new Quaternion[size];
            scale = new Vector3[size];
            uv_scroll = new Vector2[size];
        }
    }



    public bool m_enable_rotation;
    public bool m_enable_scale;
    public bool m_enable_uv_scroll;

    [SerializeField] int m_max_instances = 1024 * 16;
    [SerializeField] Mesh m_mesh;
    [SerializeField] Material m_material;
    public LayerMask m_layer = 1;
    public bool m_cast_shadow = false;
    public bool m_receive_shadow = false;
    public Vector3 m_scale = Vector3.one;
    public Vector2 m_uv_scale = Vector2.one;
    public Camera m_camera;
    public bool m_flush_on_LateUpdate = true;

    int m_instances_par_batch;
    int m_instance_count;
    Transform m_trans;
    Mesh m_expanded_mesh;
    ComputeBuffer m_draw_data_buffer;
    ComputeBuffer m_instance_t_buffer;
    ComputeBuffer m_instance_r_buffer;
    ComputeBuffer m_instance_s_buffer;
    ComputeBuffer m_instance_uv_buffer;
    DrawData[] m_draw_data = new DrawData[1];
    List<ComputeBuffer> m_batch_data_buffers;
    List<Material> m_materials;

    InstanceData m_instance_data;


    public ComputeBuffer GetInstanceTBuffer() { return m_instance_t_buffer; }
    public ComputeBuffer GetInstanceRBuffer() { return m_instance_r_buffer; }
    public ComputeBuffer GetInstanceSBuffer() { return m_instance_s_buffer; }
    public ComputeBuffer GetInstanceUVBuffer() { return m_instance_uv_buffer; }
    public int GetMaxInstanceCount() { return m_max_instances; }
    public int GetInstanceCount() { return m_instance_count; }
    public void SetInstanceCount(int v) { m_instance_count = v; }

    public void Flush()
    {
        if (m_mesh == null)
        {
            m_instance_count = 0;
            return;
        }

        m_expanded_mesh.bounds = new Bounds(m_trans.position, m_trans.localScale);
        int num_instances = Mathf.Min(m_instance_count, m_max_instances);
        int num_batches = (num_instances / m_instances_par_batch) + (num_instances % m_instances_par_batch != 0 ? 1 : 0);

        int data_flags = 1;
        if (m_enable_rotation) data_flags |= 1 << 1;
        if (m_enable_scale) data_flags |= 1 << 2;
        if (m_enable_uv_scroll) data_flags |= 1 << 3;
        m_draw_data[0].data_flags = data_flags;
        m_draw_data[0].num_instances = num_instances;
        m_draw_data[0].scale = m_scale;
        m_draw_data[0].uv_scale = m_uv_scale;
        m_draw_data_buffer.SetData(m_draw_data);

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
            m.SetBuffer("g_instance_t", m_instance_t_buffer);
            m.SetBuffer("g_instance_r", m_instance_r_buffer);
            m.SetBuffer("g_instance_s", m_instance_s_buffer);
            m.SetBuffer("g_instance_uv", m_instance_uv_buffer);
            m_materials.Add(m);
            m_batch_data_buffers.Add(batch_data_buffer);
        }

        m_instance_t_buffer.SetData(m_instance_data.translation);
        if (m_enable_rotation)  { m_instance_r_buffer.SetData(m_instance_data.rotation); }
        if (m_enable_scale)     { m_instance_s_buffer.SetData(m_instance_data.scale); }
        if (m_enable_uv_scroll) { m_instance_uv_buffer.SetData(m_instance_data.uv_scroll); }

        Matrix4x4 identity = Matrix4x4.identity;
        for (int i = 0; i * m_instances_par_batch < num_instances; ++i)
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
        if (m_instance_t_buffer != null) { m_instance_t_buffer.Release(); m_instance_t_buffer = null; }
        if (m_instance_r_buffer != null) { m_instance_r_buffer.Release(); m_instance_r_buffer = null; }
        if (m_instance_s_buffer != null) { m_instance_s_buffer.Release(); m_instance_s_buffer = null; }
        if (m_instance_uv_buffer != null) { m_instance_uv_buffer.Release(); m_instance_uv_buffer = null; }
        m_batch_data_buffers.ForEach((e) => { e.Release(); });
        m_batch_data_buffers.Clear();
        m_materials.Clear();
    }

    void ResetBuffers()
    {
        ReleaseBuffers();

        m_instance_data.Resize(m_max_instances);
        m_draw_data_buffer = new ComputeBuffer(1, DrawData.size);
        m_instance_t_buffer = new ComputeBuffer(m_max_instances, 12);
        m_instance_r_buffer = new ComputeBuffer(m_max_instances, 16);
        m_instance_s_buffer = new ComputeBuffer(m_max_instances, 12);
        m_instance_uv_buffer = new ComputeBuffer(m_max_instances, 8);
    }


    void OnEnable()
    {
        if (m_mesh == null) return;

        m_trans = GetComponent<Transform>();
        m_batch_data_buffers = new List<ComputeBuffer>();
        m_materials = new List<Material>();
        m_instance_data = new InstanceData();

        m_instances_par_batch = max_vertices / m_mesh.vertexCount;
        m_expanded_mesh = CreateExpandedMesh(m_mesh);
        m_expanded_mesh.UploadMeshData(true);

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
