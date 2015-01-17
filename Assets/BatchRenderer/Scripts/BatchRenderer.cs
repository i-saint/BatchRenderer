using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

public class BatchRenderer : MonoBehaviour
{
    public struct TR
    {
        public Vector3 translation;
        public Quaternion rotation;
    }

    public void AddInstances(Vector3[] instances, int start, int length)
    {
        if (m_instance_count >= m_max_instances) return;
        int n = Mathf.Min(length, m_max_instances - m_instance_count);
        System.Array.Copy(instances, start, m_instance_t, m_instance_count, n);
        m_instance_count += n;
    }
    public void AddInstance(ref Vector3 pos)
    {
        if (m_instance_count >= m_max_instances) return;
        m_instance_t[m_instance_count++] = pos;
    }

    public void AddInstances(TR[] tr, int start, int length)
    {
        if (m_instance_count >= m_max_instances) return;
        int n = Mathf.Min(length, m_max_instances - m_instance_count);
        System.Array.Copy(tr, start, m_instance_tr, m_instance_count, n);
        m_instance_count += n;
    }
    public void AddInstance(ref TR tr)
    {
        if (m_instance_count >= m_max_instances) return;
        m_instance_tr[m_instance_count++] = tr;
    }

    public void AddInstances(Matrix4x4[] instances, int start, int length)
    {
        if (m_instance_count >= m_max_instances) return;
        int n = Mathf.Min(length, m_max_instances - m_instance_count);
        System.Array.Copy(instances, start, m_instance_trs, m_instance_count, n);
        m_instance_count += n;
    }
    public void AddInstance(ref Matrix4x4 mat)
    {
        if (m_instance_count >= m_max_instances) return;
        m_instance_trs[m_instance_count++] = mat;
    }


    public struct DrawData
    {
        public const int size = 20;

        public int data_type;
        public int num_instances;
        public Vector3 scale;
        public Vector3 object_to_camera_direction;
    }

    public struct BatchData
    {
        public const int size = 8;

        public int begin;
        public int end;
    }

    public enum DataType
    {
        Position,
        PositionAndRotation,
        Matrix,
    }

    public int m_max_instances = 1024 * 16;
    public Mesh m_mesh;
    public Material m_material;
    public bool m_cast_shadow = false;
    public bool m_receive_shadow = false;
    public DataType m_data_type;
    public Vector3 m_scale = Vector3.one;
    public Camera m_camera;

    int m_instances_par_batch;
    int m_instance_count;
    Transform m_trans;
    Mesh m_expanded_mesh;
    ComputeBuffer m_draw_data_buffer;
    ComputeBuffer m_instance_buffer;
    DrawData[] m_draw_data = new DrawData[1];
    Vector3[] m_instance_t;
    TR[] m_instance_tr;
    Matrix4x4[] m_instance_trs;
    List<ComputeBuffer> m_batch_data_buffers;
    List<Material> m_materials;


    public ComputeBuffer GetInstanceBuffer() { return m_instance_buffer; }
    public int GetMaxInstanceCount() { return m_max_instances; }
    public int GetInstanceCount() { return m_instance_count; }
    public void SetInstanceCount(int v) { m_instance_count = v; }
    public DataType GetDataType() { return m_data_type; }

    public static Mesh CreateExpandedMesh(Mesh mesh)
    {
        Vector3[] vertices_base = mesh.vertices;
        Vector3[] normals_base = mesh.normals;
        Vector2[] uv_base = mesh.uv1;
        int[] indices_base = mesh.triangles;
        int instances_par_batch = 65536 / mesh.vertexCount;
        int num_vertices = mesh.vertexCount;
        int num_indices = indices_base.Length;

        Vector3[] vertices = new Vector3[num_vertices * instances_par_batch];
        Vector3[] normals = new Vector3[num_vertices * instances_par_batch];
        Vector2[] uv = new Vector2[num_vertices * instances_par_batch];
        Vector2[] uv2 = new Vector2[num_vertices * instances_par_batch];
        int[] indices = new int[num_indices * instances_par_batch];
        
        for(int ii=0; ii<instances_par_batch; ++ii) {
            for(int vi = 0; vi<num_vertices; ++vi) {
                int i = ii*num_vertices + vi;
                vertices[i] = vertices_base[vi];
                normals[i] = normals_base[vi];
                uv[i] = uv_base[vi];
                uv2[i] = new Vector2((float)ii, (float)vi);
            }
            for(int vi = 0; vi<num_indices; ++vi) {
                int i = ii*num_indices + vi;
                indices[i] = ii*num_vertices + indices_base[vi];
            }
        }
        Mesh ret = new Mesh();
        ret.vertices = vertices;
        ret.normals = normals;
        ret.uv = uv;
        ret.uv2 = uv2;
        ret.triangles = indices;
        return ret;
    }

    public void Flush()
    {
        if (m_mesh == null)
        {
            m_instance_count = 0;
            return;
        }

        Camera cam = m_camera != null ? m_camera : Camera.current;
        m_expanded_mesh.bounds = new Bounds(m_trans.position, m_trans.localScale);
        int num_instances = m_instance_count;
        int num_batches = (num_instances / m_instances_par_batch) + (num_instances % m_instances_par_batch != 0 ? 1 : 0);

        m_draw_data[0].data_type = (int)m_data_type;
        m_draw_data[0].num_instances = num_instances;
        m_draw_data[0].scale = m_scale;
        if (cam != null)
        {
            m_draw_data[0].object_to_camera_direction = cam.GetComponent<Transform>().forward;
        }
        m_draw_data_buffer.SetData(m_draw_data);

        while (m_batch_data_buffers.Count < num_batches)
        {
            int i = m_batch_data_buffers.Count;
            ComputeBuffer mb = new ComputeBuffer(1, BatchData.size);
            int begin = i * m_instances_par_batch;
            int end = (i + 1) * m_instances_par_batch;
            BatchData[] batch_data = new BatchData[1];
            batch_data[0].begin = begin;
            batch_data[0].end = end;
            mb.SetData(batch_data);

            Material m = new Material(m_material);
            m.SetBuffer("g_draw_data", m_draw_data_buffer);
            m.SetBuffer("g_batch_data", mb);
            m.SetBuffer("g_instance_t", m_instance_buffer);
            m.SetBuffer("g_instance_tr", m_instance_buffer);
            m.SetBuffer("g_instance_trs", m_instance_buffer);
            m_batch_data_buffers.Add(mb);
            m_materials.Add(m);
        }

        switch (m_data_type)
        {
            case DataType.Position:
                m_instance_buffer.SetData(m_instance_t);
                break;
            case DataType.PositionAndRotation:
                m_instance_buffer.SetData(m_instance_tr);
                break;
            case DataType.Matrix:
                m_instance_buffer.SetData(m_instance_trs);
                break;
        }
        Matrix4x4 identity = Matrix4x4.identity;
        for (int i = 0; i * m_instances_par_batch < num_instances; ++i)
        {
            Graphics.DrawMesh(m_expanded_mesh, identity, m_materials[i], 0, m_camera, 0, null, m_cast_shadow, m_receive_shadow);
        }
        m_instance_count = 0;
    }


    void Start()
    {
        if (m_mesh == null) return;

        m_trans = GetComponent<Transform>();
        m_batch_data_buffers = new List<ComputeBuffer>();
        m_draw_data_buffer = new ComputeBuffer(1, DrawData.size);
        switch(m_data_type) {
            case DataType.Position:
                m_instance_t = new Vector3[m_max_instances];
                m_instance_buffer = new ComputeBuffer(m_max_instances, 12);
                break;
            case DataType.PositionAndRotation:
                m_instance_tr = new TR[m_max_instances];
                m_instance_buffer = new ComputeBuffer(m_max_instances, 28);
                break;
            case DataType.Matrix:
                m_instance_trs = new Matrix4x4[m_max_instances];
                m_instance_buffer = new ComputeBuffer(m_max_instances, 64);
                break;
        }
        m_materials = new List<Material>();

        m_instances_par_batch = 65536 / m_mesh.vertexCount;
        m_expanded_mesh = CreateExpandedMesh(m_mesh);
        m_expanded_mesh.UploadMeshData(true);
    }

    void OnDestroy()
    {
        if (m_batch_data_buffers != null)
        {
            m_batch_data_buffers.ForEach((e) => { e.Release(); });
            m_instance_buffer.Release();
            m_draw_data_buffer.Release();
        }
    }

    void LateUpdate()
    {
        Flush();
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(transform.position, transform.localScale);
    }
}
