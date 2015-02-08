using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Spawner : MonoBehaviour
{
    public GameObject m_prefab;
    public float m_spawn_count_par_second = 0.1f;
    float m_local_time;

    protected Transform m_trans;

    void Awake()
    {
        m_trans = GetComponent<Transform>();
    }

    static float R()
    {
        return Random.Range(-0.5f, 0.5f);
    }

    void Update()
    {
        m_local_time += Time.deltaTime;
        float frequency = 1.0f / m_spawn_count_par_second;
        while (m_local_time >= frequency)
        {
            m_local_time -= frequency;
            Vector4 pos = m_trans.localToWorldMatrix * new Vector4(R(), R(), R(), 1.0f);
            Instantiate(m_prefab, pos, Quaternion.identity);
        }
    }


    void OnDrawGizmos()
    {
        Gizmos.color = Color.magenta;
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
    }
}
