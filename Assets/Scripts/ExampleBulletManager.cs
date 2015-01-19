using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Threading;



public class ExampleBulletManager : MonoBehaviour
{
    [System.Serializable]
    public struct BulletEntity
    {
        public Vector3 position;
        public Vector3 velosity;
        public Quaternion rotation;
        public float lifetime;
        public bool is_dead;
    }

    public bool m_use_multithread = true;
    public int m_max_entities = 1024 * 8;
    public BulletEntity[] m_entities;
    BulletEntity[] m_entities_to_add;

    BatchRenderer m_renderer;
    int m_index;
    int m_add_index;
    float m_delta_time;
    int m_num_tasks;
    int m_num_active_tasks;

    const int entities_par_task = 256;

    public BulletEntity Shoot(Vector3 pos, Vector3 vel, float lifetime = 10.0f)
    {
        var e = new BulletEntity {
            position = pos,
            velosity = vel,
            rotation = Quaternion.LookRotation(vel),
            lifetime = lifetime,
            is_dead = false
        };
        m_entities_to_add[m_add_index++] = e;
        m_add_index %= m_entities_to_add.Length;
        return e;
    }


    void Awake()
    {
        m_renderer = GetComponent<BatchRenderer>();
        m_renderer.m_flush_on_LateUpdate = false;
        m_entities = new BulletEntity[m_max_entities];
        m_entities_to_add = new BulletEntity[m_max_entities];
        m_num_tasks = m_max_entities / entities_par_task;
    }

    void Update()
    {
        for (int i = 0; i < m_add_index; ++i )
        {
            m_entities[m_index++] = m_entities_to_add[i];
            m_index %= m_max_entities;
        }
        m_add_index = 0;


        m_delta_time = Time.deltaTime;
        m_num_active_tasks = m_num_tasks;

        if (m_use_multithread)
        {
            for (int i = 0; i < m_num_tasks; ++i)
            {
                ThreadPool.QueueUserWorkItem(Task, i);
            }
        }
        else
        {
            for (int i = 0; i < m_num_tasks; ++i)
            {
                Task(i);
            }
        }
    }

    void LateUpdate()
    {
        while (m_num_active_tasks > 0) { }

        m_renderer.Flush();
    }

    void Task(System.Object c)
    {
        int nth = (int)c;
        float dt = m_delta_time;
        for (int i = 0; i < entities_par_task; ++i )
        {
            int ei = nth*entities_par_task + i;
            if (m_entities[ei].is_dead) { continue; }

            m_entities[ei].position += m_entities[ei].velosity * dt;
            m_entities[ei].lifetime -= dt;
            if (m_entities[ei].lifetime <= 0.0f)
            {
                m_entities[ei].is_dead = true;
            }
            if (!m_entities[ei].is_dead)
            {
                m_renderer.AddInstanceTR(m_entities[ei].position, m_entities[ei].rotation);
            }
        }
        Interlocked.Decrement(ref m_num_active_tasks);
    }
}
