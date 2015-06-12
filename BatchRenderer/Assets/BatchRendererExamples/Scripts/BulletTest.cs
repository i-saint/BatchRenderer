using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BulletTest : MonoBehaviour
{
    ExampleBulletManager m_bullets;

    void Awake()
    {
        m_bullets = GetComponent<ExampleBulletManager>();
    }

    void OnGUI()
    {
        if (m_bullets != null)
        {
            GUI.Label(new Rect(5.0f, 25.0f, 105.0f, 20.0f), "bullets: " + m_bullets.m_num_active_entities);
        }
    }
}
