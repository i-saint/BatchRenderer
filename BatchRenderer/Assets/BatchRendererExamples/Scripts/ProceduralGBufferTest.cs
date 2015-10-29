using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Ist;

public class ProceduralGBufferTest : MonoBehaviour
{
    public GameObject m_prefab;
    public int m_max_instance = 1000;
    public int m_count;
    ProceduralGBuffer m_pgb;

    void Start()
    {
        m_pgb = GetComponent<ProceduralGBuffer>();
    }

    void Update()
    {
        if (m_count < m_max_instance)
        {
            for (int i = 0; i < 6; ++i )
            {
                ++m_count;
                float r = 5.0f;
                var pos = new Vector3(Random.Range(-r, r), Random.Range(-r, r) + 15.0f, Random.Range(-r, r));
                var axis = new Vector3(Random.Range(-r, r), Random.Range(-r, r), Random.Range(-r, r)).normalized;
                GameObject go = (GameObject)Instantiate(m_prefab, pos, Quaternion.AngleAxis(Random.Range(-Mathf.PI, Mathf.PI), axis));
                go.GetComponent<ProceduralGBufferInstance>().m_renderer = m_pgb;

            }
        }
    }

}

