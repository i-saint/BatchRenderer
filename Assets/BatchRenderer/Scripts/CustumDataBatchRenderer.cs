using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;


public class CustumDataBatchRenderer<InstanceData> : BatchRendererBase
    where InstanceData : struct
{

    public void AddInstance(ref InstanceData v)
    {
        int i = Interlocked.Increment(ref m_instance_count) - 1;
        if (i < m_max_instances)
        {
            m_instance_data[i] = v;
        }
    }

    public InstanceData[] ReserveInstance(int num, out int reserved_index, out int reserved_num)
    {
        reserved_index = Interlocked.Add(ref m_instance_count, num) - num;
        reserved_num = Mathf.Clamp(m_max_instances - reserved_index, 0, num);
        return m_instance_data;
    }


    [System.Serializable]
    public struct DrawData
    {
        public const int size = 20;

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


    public int m_sizeof_instance_data;

    protected ComputeBuffer m_draw_data_buffer;
    protected DrawData[] m_draw_data = new DrawData[1];
    protected List<ComputeBuffer> m_batch_data_buffers;
    protected InstanceData[] m_instance_data;
    protected ComputeBuffer m_instance_buffer;
    protected RenderTexture m_instance_texture;


    public ComputeBuffer GetInstanceBuffer() { return m_instance_buffer; }
    public RenderTexture GetInstanceTexture() { return m_instance_texture; }

    public override Material CloneMaterial(int nth)
    {
        Material m = new Material(m_material);
        m.SetBuffer("g_draw_data", m_draw_data_buffer);
        m.SetBuffer("g_instance_buffer", m_instance_buffer);


        ComputeBuffer batch_data_buffer = new ComputeBuffer(1, BatchData.size);
        BatchData[] batch_data = new BatchData[1];
        batch_data[0].begin = nth * m_instances_par_batch;
        batch_data[0].end = (nth + 1) * m_instances_par_batch;
        batch_data_buffer.SetData(batch_data);
        m.SetBuffer("g_batch_data", batch_data_buffer);
        m_batch_data_buffers.Add(batch_data_buffer);

        // fix rendering order for transparent objects
        if (m.renderQueue >= 3000)
        {
            m.renderQueue = m.renderQueue + (nth + 1);
        }
        return m;
    }


    public virtual void ReleaseBuffers()
    {
        if (m_draw_data_buffer != null) { m_draw_data_buffer.Release(); m_draw_data_buffer = null; }
        m_instance_buffer.Release();
        m_batch_data_buffers.ForEach((e) => { e.Release(); });
        m_batch_data_buffers.Clear();
        m_materials.Clear();
    }

    public virtual void ResetBuffers()
    {
        ReleaseBuffers();

        m_instance_data = new InstanceData[m_max_instances];
        m_draw_data_buffer = new ComputeBuffer(1, DrawData.size);
        m_instance_buffer = new ComputeBuffer(m_max_instances, m_sizeof_instance_data);

        UploadInstanceData();
    }

    public override void UploadInstanceData()
    {
        m_draw_data[0].num_instances = m_instance_count;
        m_draw_data[0].scale = m_scale;
        m_draw_data_buffer.SetData(m_draw_data);
    }


    public override void OnEnable()
    {
        base.OnEnable();
        if (m_mesh == null) return;

        ResetBuffers();
    }

    public override void OnDisable()
    {
        base.OnDisable();
        ReleaseBuffers();
    }
}
