using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class BatchRendererTest : MonoBehaviour
{
    public enum DataTransferMode
    {
        SingleCopy,
        ArrayCopy,
        ReserveAndWrite,
    }
    public int m_num_draw;
    public DataTransferMode m_transfer_mode;
    BatchRenderer m_renderer;
    Vector3[] m_instance_t;
    Quaternion[] m_instance_r;
    Vector3[] m_instance_s;

    void Awake()
    {
        m_renderer = GetComponent<BatchRenderer>();

        int num = m_renderer.GetMaxInstanceCount();
        m_num_draw = num;
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
        switch(m_transfer_mode) {
            case DataTransferMode.SingleCopy:
                DataTransfer_SingleCopy();
                break;
            case DataTransferMode.ArrayCopy:
                DataTransfer_ArrayCopy();
                break;
            case DataTransferMode.ReserveAndWrite:
                DataTransfer_ReserveAndWrite();
                break;
        }
    }

    void DataTransfer_SingleCopy()
    {
        //int num = Mathf.Min(m_num_draw, m_instance_matrix.Length);

    }

    void DataTransfer_ArrayCopy()
    {
        int num = Mathf.Min(m_num_draw, m_instance_t.Length);
        int reserved_index;
        int reserved_num;
        BatchRenderer.InstanceData data = m_renderer.ReserveInstance(num, out reserved_index, out reserved_num);
        System.Array.Copy(data.translation, 0, m_instance_t, reserved_index, reserved_num);
        if (m_renderer.m_enable_rotation) System.Array.Copy(data.rotation, 0, m_instance_r, reserved_index, reserved_num);
        if (m_renderer.m_enable_scale) System.Array.Copy(data.scale, 0, m_instance_s, reserved_index, reserved_num);
    }

    void DataTransfer_ReserveAndWrite()
    {
        int num = Mathf.Min(m_num_draw, m_instance_t.Length);
        int reserved_index;
        int reserved_num;
        BatchRenderer.InstanceData data = m_renderer.ReserveInstance(num, out reserved_index, out reserved_num);
        for (int i = 0; i < num; ++i)
        {
            data.translation[reserved_index + i] = m_instance_t[i];
            data.rotation[reserved_index + i] = m_instance_r[i];
            data.scale[reserved_index + i] = m_instance_s[i];
        }
    }
}
