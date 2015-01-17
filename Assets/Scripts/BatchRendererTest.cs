using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class BatchRendererTest : MonoBehaviour
{
    public int m_num_draw;
    BatchRenderer m_renderer;
    Vector3[] m_instance_t;
    BatchRenderer.TR[] m_instance_tr;
    Matrix4x4[] m_instance_trs;

    void Awake()
    {
        m_renderer = GetComponent<BatchRenderer>();

        int num = m_renderer.GetMaxInstanceCount();
        m_num_draw = num;
        m_instance_t = new Vector3[num];
        m_instance_tr = new BatchRenderer.TR[num];
        m_instance_trs = new Matrix4x4[num];
        Vector3 scale = Vector3.one * 0.9f;
        Quaternion rot = Quaternion.AngleAxis(45.0f, Vector3.up);
        for (int i = 0; i < num; ++i)
        {
            Vector3 pos = new Vector3(
                0.25f * (i / 256) - 16.0f,
                Random.Range(-1.5f, -0.1f),
                0.25f * (i % 256) - 16.0f);
            m_instance_t[i] = pos;
            m_instance_tr[i] = new BatchRenderer.TR { translation = pos, rotation = rot };
            m_instance_trs[i] = Matrix4x4.TRS(pos, rot, scale);
        }
    }

    void Update()
    {
        int num = Mathf.Min(m_num_draw, m_instance_trs.Length);
        switch (m_renderer.GetDataType())
        {
            case BatchRenderer.DataType.Position:
                m_renderer.AddInstances(m_instance_t, 0, num);
                break;
            case BatchRenderer.DataType.PositionAndRotation:
                m_renderer.AddInstances(m_instance_tr, 0, num);
                break;
            case BatchRenderer.DataType.Matrix:
                m_renderer.AddInstances(m_instance_trs, 0, num);
                break;
        }
    }
}
