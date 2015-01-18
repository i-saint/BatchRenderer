using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class BatchRendererTest : MonoBehaviour
{
    public enum DataTransferMode
    {
        SingleCopy,
        ArrayCopy,
        Action,
    }
    public int m_num_draw;
    public BatchRenderer.DataType m_data_type;
    public DataTransferMode m_transfer_mode;
    BatchRenderer m_renderer;
    Vector3[] m_instance_t;
    BatchRenderer.TR[] m_instance_tr;
    BatchRenderer.TRS[] m_instance_trs;
    Matrix4x4[] m_instance_matrix;

    void Awake()
    {
        m_renderer = GetComponent<BatchRenderer>();

        int num = m_renderer.GetMaxInstanceCount();
        m_num_draw = num;
        m_instance_t = new Vector3[num];
        m_instance_tr = new BatchRenderer.TR[num];
        m_instance_trs = new BatchRenderer.TRS[num];
        m_instance_matrix = new Matrix4x4[num];
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
            m_instance_trs[i] = new BatchRenderer.TRS { translation = pos, rotation = rot, scale = scale };
            m_instance_matrix[i] = Matrix4x4.TRS(pos, rot, scale);
        }
    }

    void Update()
    {
        m_renderer.SetDataType(m_data_type);
        switch(m_transfer_mode) {
            case DataTransferMode.SingleCopy:
                DataTransfer_SingleCopy();
                break;
            case DataTransferMode.ArrayCopy:
                DataTransfer_ArrayCopy();
                break;
            case DataTransferMode.Action:
                DataTransfer_Action();
                break;
        }
    }

    void DataTransfer_SingleCopy()
    {
        int num = Mathf.Min(m_num_draw, m_instance_matrix.Length);
        switch (m_renderer.GetDataType())
        {
            case BatchRenderer.DataType.Position:
                for (int i = 0; i < num; ++i )
                {
                    m_renderer.AddInstance(m_instance_t[i]);
                }
                break;
            case BatchRenderer.DataType.PositionRotation:
                for (int i = 0; i < num; ++i)
                {
                    m_renderer.AddInstance(ref m_instance_tr[i]);
                }
                break;
            case BatchRenderer.DataType.PositionRotationScale:
                for (int i = 0; i < num; ++i)
                {
                    m_renderer.AddInstance(ref m_instance_trs[i]);
                }
                break;
            case BatchRenderer.DataType.Matrix:
                for (int i = 0; i < num; ++i)
                {
                    m_renderer.AddInstance(ref m_instance_matrix[i]);
                }
                break;
        }
    }

    void DataTransfer_ArrayCopy()
    {
        int num = Mathf.Min(m_num_draw, m_instance_matrix.Length);
        switch (m_renderer.GetDataType())
        {
            case BatchRenderer.DataType.Position:
                m_renderer.AddInstances(m_instance_t, 0, num);
                break;
            case BatchRenderer.DataType.PositionRotation:
                m_renderer.AddInstances(m_instance_tr, 0, num);
                break;
            case BatchRenderer.DataType.PositionRotationScale:
                m_renderer.AddInstances(m_instance_trs, 0, num);
                break;
            case BatchRenderer.DataType.Matrix:
                m_renderer.AddInstances(m_instance_matrix, 0, num);
                break;
        }
    }

    void DataTransfer_Action()
    {
        int num = Mathf.Min(m_num_draw, m_instance_matrix.Length);
        switch (m_renderer.GetDataType())
        {
            case BatchRenderer.DataType.Position:
                m_renderer.AddInstances(num, (Vector3[] instances, int start, int n) =>
                {
                    for (int i = 0; i < n; ++i)
                    {
                        instances[start + i] = m_instance_t[i];
                    }
                });
                break;
            case BatchRenderer.DataType.PositionRotation:
                m_renderer.AddInstances(num, (BatchRenderer.TR[] instances, int start, int n) =>
                {
                    for (int i = 0; i < n; ++i)
                    {
                        instances[start + i] = m_instance_tr[i];
                    }
                });
                break;
            case BatchRenderer.DataType.PositionRotationScale:
                m_renderer.AddInstances(num, (BatchRenderer.TRS[] instances, int start, int n) =>
                {
                    for (int i = 0; i < n; ++i)
                    {
                        instances[start + i] = m_instance_trs[i];
                    }
                });
                break;
            case BatchRenderer.DataType.Matrix:
                m_renderer.AddInstances(num, (Matrix4x4[] instances, int start, int n) =>
                {
                    for (int i = 0; i < n; ++i)
                    {
                        instances[start + i] = m_instance_matrix[i];
                    }
                });
                break;
        }
    }
}
