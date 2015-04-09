using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;


public abstract class BatchRendererBase : MonoBehaviour
{
    public int m_max_instances = 1024 * 4;
    public Mesh m_mesh;
    public Material m_material;
    public LayerMask m_layer_selector = 1;
    public bool m_cast_shadow = false;
    public bool m_receive_shadow = false;
    public Vector3 m_scale = Vector3.one;
    public Camera m_camera;
    public bool m_flush_on_LateUpdate = true;
    public Vector3 m_bounds_size = Vector3.one;

    protected int m_instances_par_batch;
    protected int m_instance_count;
    protected int m_batch_count;
    protected int m_layer;
    protected Transform m_trans;
    protected Mesh m_expanded_mesh;
    protected List<Material> m_materials;

    public int GetMaxInstanceCount() { return m_max_instances; }
    public int GetInstanceCount() { return m_instance_count; }
    public void SetInstanceCount(int v) { m_instance_count = v; }



    public abstract Material CloneMaterial(int nth);
    public abstract void UpdateGPUResources();


    public virtual void Flush()
    {
        if (m_expanded_mesh == null || m_instance_count == 0)
        {
            m_instance_count = 0;
            return;
        }

        Vector3 scale = m_trans.localScale;
        m_expanded_mesh.bounds = new Bounds(m_trans.position,
            new Vector3(m_bounds_size.x * scale.x, m_bounds_size.y * scale.y, m_bounds_size.y * scale.y));
        m_instance_count = Mathf.Min(m_instance_count, m_max_instances);
        m_batch_count = BatchRendererUtil.ceildiv(m_instance_count, m_instances_par_batch);

        while (m_materials.Count < m_batch_count)
        {
            Material m = CloneMaterial(m_materials.Count);
            m_materials.Add(m);
        }
        UpdateGPUResources();

        Matrix4x4 matrix = Matrix4x4.identity;
        for (int i = 0; i < m_batch_count; ++i)
        {
            Graphics.DrawMesh(m_expanded_mesh, matrix, m_materials[i], m_layer, m_camera, 0, null, m_cast_shadow, m_receive_shadow);
        }
        m_instance_count = m_batch_count = 0;
    }



    public virtual void OnEnable()
    {
        m_trans = GetComponent<Transform>();
        m_materials = new List<Material>();

        if (m_expanded_mesh == null && m_mesh != null)
        {
            m_expanded_mesh = BatchRendererUtil.CreateExpandedMesh(m_mesh, out m_instances_par_batch);
            m_expanded_mesh.UploadMeshData(true);
        }

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

    }

    public virtual void OnDisable()
    {
    }

    public virtual void LateUpdate()
    {
        if (m_flush_on_LateUpdate)
        {
            Flush();
        }
    }

    public virtual void OnDrawGizmos()
    {
        Transform t = GetComponent<Transform>();
        Vector3 s = t.localScale;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(t.position, new Vector3(m_bounds_size.x * s.x, m_bounds_size.y * s.y, m_bounds_size.z * s.z));
    }
}
