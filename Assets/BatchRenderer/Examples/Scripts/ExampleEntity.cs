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
    public float m_hit_radius = 0.5f;
    public float m_delta_damage = 0;

    protected Transform m_trans;


    public Transform GetTransform() { return m_trans; }
    public virtual void AddDamage(float dd)
    {
        m_delta_damage += dd;
    }


    public virtual void OnEnable()
    {
        GetInstances().Add(this);
        m_trans = GetComponent<Transform>();
    }

    public virtual void OnDisable()
    {
        GetInstances().Remove(this);
    }

    public virtual void Update()
    {
        if (m_life > 0.0f)
        {
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
