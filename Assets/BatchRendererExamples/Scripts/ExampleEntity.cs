using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ExampleEntity : MonoBehaviour
{
    static List<ExampleEntity> s_all;
    public static List<ExampleEntity> GetInstances()
    {
        if (s_all == null)
        {
            s_all = new List<ExampleEntity>();
        }
        return s_all;
    }

    public float m_life = 100.0f;
    public float m_delta_damage = 0;

    protected Transform m_trans;
    protected Rigidbody m_rigid;
    protected ExampleBulletCollider m_bcol;


    public Transform GetTransform() { return m_trans; }
    public virtual void AddDamage(float dd)
    {
        m_delta_damage += dd;
    }

    public float R(float v=1.0f)
    {
        return Random.Range(-v, v);
    }


    public virtual void OnEnable()
    {
        GetInstances().Add(this);
        m_trans = GetComponent<Transform>();
        m_rigid = GetComponent<Rigidbody>();
        m_bcol = GetComponent<ExampleBulletCollider>();
    }

    public virtual void OnDisable()
    {
        GetInstances().Remove(this);
    }

    public virtual void Update()
    {
        {
            Vector3 pos = m_trans.position;
            pos.z = 0.0f;
            m_trans.position = pos;
        }
        if (m_rigid != null && !m_rigid.isKinematic)
        {
            Vector3 vel = m_rigid.velocity;
            vel.z = 0.0f;
            m_rigid.velocity = vel;
        }

        if (m_life > 0.0f)
        {
            if (m_bcol != null)
            {
                m_delta_damage += (float)m_bcol.m_num_hits;
                m_bcol.m_num_hits = 0;
            }

            m_life -= m_delta_damage;
            m_delta_damage = 0;
            if (m_life <= 0.0f)
            {
                OnLifeZero();
            }
        }
    }

    public virtual void OnLifeZero()
    {
        // override me!
    }
}
