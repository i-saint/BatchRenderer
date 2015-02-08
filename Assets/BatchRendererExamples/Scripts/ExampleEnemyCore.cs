using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ExampleEnemyCore : ExampleEntity
{
    ExampleBulletManager m_bullets;

    void Start()
    {
        m_bullets = ExampleGame.GetBulletManager();
        GetComponent<BatchRendererInstance>().m_renderer = ExampleGame.GetSphereRenderer();
    }

    public override void Update()
    {
        float spread = 0.4f;
        Vector3 pos = m_trans.position + m_trans.forward*1.0f;
        for (int i = 0; i < 24; ++i)
        {
            Vector3 dir = (m_trans.forward + (new Vector3(R(1.0f), R(1.0f), 0.0f)).normalized * spread).normalized;
            float speed = 6.0f + R(0.25f);
            m_bullets.Shoot(pos, dir * speed, 4.0f, m_bcol.m_id);
        }
        base.Update();
    }
}
