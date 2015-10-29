using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Ist;


public class ExampleBulletManager : MonoBehaviour
{
    [System.Serializable]
    public struct Range
    {
        public int begin;
        public int end;
    }

    [System.Serializable]
    public struct BulletEntity
    {
        public Vector3 position;
        public Vector3 velosity;
        public Quaternion rotation;
        public float lifetime;
        public int owner_id;
    }

    struct HitTarget
    {
        public Vector3 position;
        public float radius;
        public int id;
        public int num_hits;
        public ExampleBulletCollider collider;
    }

    struct WorkData
    {
        public float delta_time;
        public int num_targets;
        public HitTarget[] targets;
    }
    struct TaskWorkData
    {
        public int num_active_entities;
        public int hit_count;
    }

    public bool m_use_multithread = true;
    public float m_radius = 0.1f;
    public int m_max_entities = 1024 * 8;
    public int m_num_active_entities = 0;
    public BulletEntity[] m_entities;
    BulletEntity[] m_entities_to_add;

    BatchRenderer m_renderer;
    int m_index;
    int m_add_index;
    int m_num_tasks;
    int m_num_active_tasks;
    WorkData m_work_data;
    TaskWorkData[] m_task_work_data;

    const int m_entities_par_task = 1024;

    public BulletEntity Shoot(Vector3 pos, Vector3 vel, float lifetime = 10.0f, int owner_id=0)
    {
        var e = new BulletEntity
        {
            position = pos,
            velosity = vel,
            rotation = Quaternion.LookRotation(vel),
            lifetime = lifetime,
            owner_id = owner_id,
        };
        m_entities_to_add[m_add_index++] = e;
        m_add_index %= m_entities_to_add.Length;
        return e;
    }

    public void Shoot(Vector3[] pos, Vector3[] vel, float lifetime = 10.0f, int owner_id = 0)
    {
        for (int i = 0; i < pos.Length; ++i )
        {
            var e = new BulletEntity
            {
                position = pos[i],
                velosity = vel[i],
                rotation = Quaternion.LookRotation(vel[i]),
                lifetime = lifetime,
            };
            m_entities_to_add[m_add_index++] = e;
            m_add_index %= m_entities_to_add.Length;
        }
    }


    void Awake()
    {
        m_renderer = GetComponent<BatchRenderer>();
        m_renderer.m_flush_on_LateUpdate = false;
        m_entities = new BulletEntity[m_max_entities];
        m_entities_to_add = new BulletEntity[m_max_entities];

        m_num_tasks = m_max_entities / m_entities_par_task + (m_max_entities % m_entities_par_task != 0 ? 1 : 0);
        m_work_data = new WorkData();
        m_work_data.targets = new HitTarget[128];
        m_task_work_data = new TaskWorkData[m_num_tasks];
    }


    void Update()
    {
        for (int i = 0; i < m_add_index; ++i )
        {
            m_entities[m_index++] = m_entities_to_add[i];
            m_index %= m_max_entities;
        }
        m_add_index = 0;

        m_work_data.delta_time = Time.deltaTime;

        // gather hit target data
        var targets = ExampleBulletCollider.GetInstances();
        if (m_work_data.targets.Length < targets.Count)
        {
            m_work_data.targets = new HitTarget[Mathf.Max(m_work_data.targets.Length * 2, targets.Count)];
        }
        m_work_data.num_targets = targets.Count;
        for (int i = 0; i < targets.Count; ++i )
        {
            ExampleBulletCollider t = targets[i];
            m_work_data.targets[i].position = t.GetTransform().position;
            m_work_data.targets[i].radius = t.m_hit_radius;
            m_work_data.targets[i].id = t.m_id;
            m_work_data.targets[i].collider = t;
        }

        m_num_active_tasks = m_num_tasks;
        if (m_use_multithread)
        {
            for (int i = 0; i < m_num_tasks; ++i)
            {
                ThreadPool.QueueUserWorkItem(UpdateTask, i);
            }
        }
        else
        {
            for (int i = 0; i < m_num_tasks; ++i)
            {
                UpdateTask(i);
            }
        }
    }

    void LateUpdate()
    {
        while (m_num_active_tasks > 0) { } // wait for tasks complete

        m_num_active_entities = 0;
        for (int i = 0; i < m_task_work_data.Length; ++i )
        {
            m_num_active_entities += m_task_work_data[i].num_active_entities;
        }
        for (int i = 0; i < m_work_data.num_targets; ++i)
        {
            if (m_work_data.targets[i].num_hits > 0)
            {
                m_work_data.targets[i].collider.m_num_hits = m_work_data.targets[i].num_hits;
                m_work_data.targets[i].num_hits = 0;
            }
        }

        m_renderer.Flush();
    }

    void UpdateTask(System.Object c)
    {
        int task_index = (int)c;
        int begin = task_index * m_entities_par_task;
        int end = Mathf.Min((task_index + 1) * m_entities_par_task, m_max_entities);

        float dt = m_work_data.delta_time;
    
        int num_active_entities = 0;
        for (int bi = begin; bi < end; ++bi)
        {
            if (m_entities[bi].lifetime <= 0.0f) { continue; }

            ++num_active_entities;
            m_entities[bi].position += m_entities[bi].velosity * dt;
            m_entities[bi].lifetime -= dt;

            Vector3 pos = m_entities[bi].position;
            int owner_id = m_entities[bi].owner_id;
            for (int ei = 0; ei < m_work_data.num_targets; ++ei )
            {
                if (m_work_data.targets[ei].id == owner_id) continue;

                Vector3 diff = m_work_data.targets[ei].position - pos;
                if (diff.magnitude <= m_work_data.targets[ei].radius)
                {
                    Interlocked.Increment(ref m_work_data.targets[ei].num_hits);
                    m_entities[bi].lifetime = 0.0f;
                }
            }

            m_renderer.AddInstanceTR(m_entities[bi].position, m_entities[bi].rotation);
        }
        m_task_work_data[task_index].num_active_entities = num_active_entities;

        Interlocked.Decrement(ref m_num_active_tasks);
    }
}
