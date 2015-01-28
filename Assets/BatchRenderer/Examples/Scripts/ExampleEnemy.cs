using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ExampleEnemy : ExampleEntity
{
    public float m_lifetime = 60.0f;
    public Vector3 m_accel;

    void Start()
    {
        m_rigid.velocity = m_rigid.velocity + m_accel.normalized * 4.0f;
        m_rigid.angularVelocity = new Vector3(R(),R(),R());
        GetComponent<BatchRendererInstance>().m_renderer = ExampleGame.GetCubeRenderer();
    }

    public override void Update()
    {
        float dt = Time.deltaTime;
        m_lifetime -= dt;
        if (m_lifetime <= 0.0f)
        {
            AddDamage(m_life);
        }
        Vector3 vel = m_rigid.velocity;
        vel += m_accel * dt;
        vel.z = 0.0f;
        m_rigid.velocity = vel;
        base.Update();
    }

    public override void OnLifeZero()
    {
        base.OnLifeZero();
        Vector3 pos = m_trans.position;
        var bullets = ExampleGame.GetBulletManager();
        for (int i = 0; i < 512; ++i )
        {
            Vector3 vel = new Vector3(R(), R(), 0.0f).normalized * (4.0f + R(1.5f));
            Vector3 rd = new Vector3(R(), R(), 0.0f) * 0.5f;
            bullets.Shoot(pos + rd, vel);
        }
        Destroy(gameObject);
    }
}
