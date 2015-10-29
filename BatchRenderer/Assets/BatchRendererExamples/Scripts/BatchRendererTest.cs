using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Ist;

public class BatchRendererTest : MonoBehaviour
{
    public enum DataTransferMode
    {
        SingleCopy,
        ArrayCopy,
        ReserveAndWrite,
    }
    public struct Range
    {
        public int begin;
        public int end;
    }

    public int m_num_draw;
    public bool m_enable_animation = true;
    public bool m_enable_multithread = true;
    public int m_task_block_size = 2048;
    public DataTransferMode m_transfer_mode;
    BatchRenderer m_renderer;
    Vector3[] m_instance_t;
    Quaternion[] m_instance_r;
    Vector3[] m_instance_s;
    float m_time;
    int m_num_active_tasks;

    void Awake()
    {
        m_renderer = GetComponent<BatchRenderer>();

        int num = m_renderer.GetMaxInstanceCount();
        m_instance_t = new Vector3[num];
        m_instance_r = new Quaternion[num];
        m_instance_s = new Vector3[num];
        Quaternion rot = Quaternion.AngleAxis(45.0f, Vector3.forward);
        for (int i = 0; i < num; ++i)
        {
            Vector3 pos = new Vector3(
                0.1f * (i / 256) - 12.8f,
                Random.Range(-1.5f, -0.1f),
                0.1f * (i % 256) - 12.8f);
            float s = 1.0f + Mathf.Sin((float)i*0.1f) * 0.5f;
            Vector3 scale = new Vector3(s,s,s);
            m_instance_t[i] = pos;
            m_instance_r[i] = rot;
            m_instance_s[i] = scale;
        }
    }

    void Update()
    {
        m_num_draw = Mathf.Min(m_num_draw, m_instance_t.Length);
        m_time = Time.realtimeSinceStartup;
        int num = m_num_draw;
        if (m_enable_multithread)
        {
            for (int i = 0; i < num; i += m_task_block_size)
            {
                Interlocked.Increment(ref m_num_active_tasks);
                ThreadPool.QueueUserWorkItem(
                    UpdateTask,
                    new Range { begin = i, end = Mathf.Min(i + m_task_block_size, num) });
            }
            while (m_num_active_tasks != 0) { }
        }
        else
        {
            Interlocked.Increment(ref m_num_active_tasks);
            UpdateTask(new Range { begin = 0, end = num });
        }
        m_renderer.Flush();
    }

    void UpdateTask(System.Object arg)
    {
        Range r = (Range)arg;
        float time = m_time;

        if (m_enable_animation)
        {
            Vector3 axis = Vector3.forward;
            for (int i = r.begin; i < r.end; ++i)
            {
                Quaternion rot = Quaternion.AngleAxis(45.0f + time * 180.0f + (float)i * 0.01f, axis);
                float s = 1.0f + Mathf.Sin(time * 10.0f + (float)i * 0.1f) * 0.5f;
                Vector3 scale = new Vector3(s, s, s);
                m_instance_r[i] = rot;
                m_instance_s[i] = scale;
            }
        }
        switch (m_transfer_mode)
        {
            case DataTransferMode.SingleCopy:
                DataTransfer_SingleCopy(r);
                break;
            case DataTransferMode.ArrayCopy:
                DataTransfer_ArrayCopy(r);
                break;
            case DataTransferMode.ReserveAndWrite:
                DataTransfer_ReserveAndWrite(r);
                break;
        }
        Interlocked.Decrement(ref m_num_active_tasks);
    }

    void DataTransfer_SingleCopy(Range r)
    {
        if (m_renderer.m_enable_scale)
        {
            for (int i = r.begin; i < r.end; ++i)
            {
                m_renderer.AddInstanceTRS(m_instance_t[i], m_instance_r[i], m_instance_s[i]);
            }
        }
        else if (m_renderer.m_enable_rotation)
        {
            for (int i = r.begin; i < r.end; ++i)
            {
                m_renderer.AddInstanceTR(m_instance_t[i], m_instance_r[i]);
            }
        }
        else
        {
            for (int i = r.begin; i < r.end; ++i)
            {
                m_renderer.AddInstanceT(m_instance_t[i]);
            }
        }
    }

    void DataTransfer_ArrayCopy(Range r)
    {
        int num = r.end - r.begin;
        if (m_renderer.m_enable_scale)
        {
            m_renderer.AddInstancesTRS(m_instance_t, m_instance_r, m_instance_s, r.begin, num);
        }
        else if (m_renderer.m_enable_rotation)
        {
            m_renderer.AddInstancesTR(m_instance_t, m_instance_r, r.begin, num);
        }
        else
        {
            m_renderer.AddInstancesT(m_instance_t, r.begin, num);
        }
    }

    void DataTransfer_ReserveAndWrite(Range r)
    {
        int num = r.end - r.begin;
        int reserved_index;
        int reserved_num;
        BatchRenderer.InstanceData data = m_renderer.ReserveInstance(num, out reserved_index, out reserved_num);
        System.Array.Copy(m_instance_t, r.begin, data.translation, reserved_index, reserved_num);
        System.Array.Copy(m_instance_r, r.begin, data.rotation, reserved_index, reserved_num);
        System.Array.Copy(m_instance_s, r.begin, data.scale, reserved_index, reserved_num);
    }
}

