using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BatchRenderer : MonoBehaviour
{
    public void AddInstance(Transform trans)
    {
        if (m_num_instances >= m_max_instances) return;
        m_entities[m_num_instances++] = new EntityData { trans = trans.localToWorldMatrix };
    }

    public void AddInstance(Vector3 position, Quaternion rotation, Vector3 scale)
    {
        if (m_num_instances >= m_max_instances) return;
        //m_entities[m_num_instances++] = new EntityData { trans = Matrix4x4.identity };
        m_entities[m_num_instances++] = new EntityData { trans = Matrix4x4.TRS(position, rotation, scale) };
    }


    public struct MetaData
    {
        public const int size = 16;

        public int num_entities;
        public int begin;
        public int end;
        int pad2;
    }
    
    public struct EntityData
    {
        public const int size = 64;

        public Matrix4x4 trans;
    }

    public int m_max_instances = 1024 * 16;
    public Mesh m_mesh;
    public Material m_material;
    public Camera m_camera;
    int m_instances_par_batch;
    int m_num_instances;
    MetaData[] m_metadata = new MetaData[1];
    Mesh m_expanded_mesh;
    ComputeBuffer m_entity_buffer;
    EntityData[] m_entities;
    List<ComputeBuffer> m_metadata_buffers;
    List<Material> m_materials;


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
                //uv[i] = uv2[i];
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

    void Start()
    {
        if (m_mesh == null) return;

        m_entities = new EntityData[m_max_instances];
        m_metadata_buffers = new List<ComputeBuffer>();
        m_entity_buffer = new ComputeBuffer(m_max_instances, EntityData.size);
        m_materials = new List<Material>();

        m_instances_par_batch = 65536 / m_mesh.vertexCount;
        m_expanded_mesh = CreateExpandedMesh(m_mesh);
        m_expanded_mesh.bounds = new Bounds(Vector3.zero, Vector3.one * 9999999.0f); // force disable frustum culling
        m_expanded_mesh.UploadMeshData(true);
    }

    void OnDestroy()
    {
        if (m_metadata_buffers != null)
        {
            m_metadata_buffers.ForEach((e) => { e.Release(); });
            m_entity_buffer.Release();
        }
    }

    void Update()
    {
        for (int i = 0; i < 65536; ++i )
        {
            Vector3 pos = new Vector3(0.25f * (i/256)-5.0f, -0.5f, 0.25f * (i%256)-5.0f);
            m_entities[m_num_instances++] = new EntityData { trans = Matrix4x4.TRS(pos, Quaternion.identity, Vector3.one * 0.2f) };
            //AddInstance(pos, Quaternion.identity, Vector3.one * 0.2f);
        }
    }

    void LateUpdate()
    {
        if (m_mesh == null)
        {
            m_num_instances = 0;
            return;
        }

        int num_entities = m_num_instances;
        int num_batches = num_entities / m_instances_par_batch + 1;

        while (m_metadata_buffers.Count < num_batches)
        {
            ComputeBuffer mb = new ComputeBuffer(1, MetaData.size);
            Material m = new Material(m_material);
            m.SetBuffer("g_metadata", mb);
            m.SetBuffer("g_entities", m_entity_buffer);
            m_metadata_buffers.Add(mb);
            m_materials.Add(m);
        }

        m_entity_buffer.SetData(m_entities);
        Matrix4x4 identity = Matrix4x4.identity;
        for (int i = 0; i * m_instances_par_batch < num_entities; ++i)
        {
            int begin = i*m_instances_par_batch;
            int end = Mathf.Min((i + 1) * m_instances_par_batch, m_num_instances);
            m_metadata[0].num_entities = num_entities;
            m_metadata[0].begin = begin;
            m_metadata[0].end = end;
            m_metadata_buffers[i].SetData(m_metadata);
            Graphics.DrawMesh(m_expanded_mesh, identity, m_materials[i], 0, m_camera);
       }
        m_num_instances = 0;
    }
}
