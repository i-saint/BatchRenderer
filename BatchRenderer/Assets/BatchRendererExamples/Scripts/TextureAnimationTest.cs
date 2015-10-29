using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Ist;


public class TextureAnimationTest : MonoBehaviour
{
    public struct Range
    {
        public int begin;
        public int end;
    }

    public int m_num_draw = 1024;
    BatchRenderer m_renderer;
    Vector3[] m_instance_t;
    Color[] m_instance_color;
    Vector4[] m_instance_uv;
    float[] m_instance_time;
    float m_time;
    int m_num_active_tasks;


    public float R(float v = 1.0f)
    {
        return Random.Range(-v, v);
    }

    void Awake()
    {
        m_renderer = GetComponent<BatchRenderer>();

        int num = m_renderer.GetMaxInstanceCount();
        m_instance_t = new Vector3[num];
        m_instance_color = new Color[num];
        m_instance_uv = new Vector4[num];
        m_instance_time = new float[num];
        for (int i = 0; i < num; ++i)
        {
            m_instance_t[i] = new Vector3(R(10.0f), R(10.0f), R(10.0f));
            m_instance_time[i] = Random.Range(0.0f, 10.0f);
        }
    }

    void Update()
    {
        m_num_draw = Mathf.Min(m_num_draw, m_instance_t.Length);
        m_time = Time.realtimeSinceStartup;
        int num = m_num_draw;
        {
            Interlocked.Increment(ref m_num_active_tasks);
            UpdateTask(new Range { begin = 0, end = num });
        }
        m_renderer.Flush();
    }


    void UpdateTask(System.Object arg)
    {
        Range r = (Range)arg;
        int num = r.end - r.begin;

        Texture tex = m_renderer.m_material.mainTexture;

        for (int i = r.begin; i < r.end; ++i)
        {
            float time = m_instance_time[i] + m_time;
            float e = 0.75f + Mathf.Sin(time * 10.0f) * 0.25f;
            int a = (int)(time * 5.0f) % 16;
            m_instance_color[i] = new Color(1.0f, 1.0f, 1.0f, e);
            m_instance_uv[i] = BatchRendererUtil.ComputeUVOffset(tex, new Rect(32 * (a % 4), 32 * (a / 4), 32, 32));
        }
        {
            int reserved_index;
            int reserved_num;
            BatchRenderer.InstanceData data = m_renderer.ReserveInstance(num, out reserved_index, out reserved_num);
            System.Array.Copy(m_instance_t, r.begin, data.translation, reserved_index, reserved_num);
            System.Array.Copy(m_instance_color, r.begin, data.color, reserved_index, reserved_num);
            System.Array.Copy(m_instance_uv, r.begin, data.uv_offset, reserved_index, reserved_num);
        }
        Interlocked.Decrement(ref m_num_active_tasks);
    }


}
