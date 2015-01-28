using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ExampleBulletCollider : MonoBehaviour
{
    static List<ExampleBulletCollider> s_all;
    static int s_id_seed;
    public static List<ExampleBulletCollider> GetInstances()
    {
        if (s_all == null)
        {
            s_all = new List<ExampleBulletCollider>();
        }
        return s_all;
    }

    public int m_id;
    public float m_hit_radius = 0.5f;
    public int m_num_hits;
    protected Transform m_trans;


    public Transform GetTransform() { return m_trans; }

    void Awake()
    {
        m_id = ++s_id_seed;
    }

    void OnEnable()
    {
        GetInstances().Add(this);
        m_trans = GetComponent<Transform>();
    }

    void OnDisable()
    {
        GetInstances().Remove(this);
    }

    void OnDrawGizmos()
    {
        Transform t = GetComponent<Transform>();
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(t.position, m_hit_radius);
    }
}
