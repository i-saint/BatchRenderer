using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ExampleEnemy : ExampleEntity
{
    public override void OnEnable()
    {
        base.OnEnable();
    }

    public override void OnDisable()
    {
        base.OnDisable();
    }

    public override void Update()
    {
        base.Update();
    }

    public override void OnLifeZero()
    {
        base.OnLifeZero();
        Vector3 pos = m_trans.position;
        var bullets = ExampleGame.GetBulletManager();
        for (int i = 0; i < 256; ++i )
        {
            Vector3 vel = new Vector3(R(), R(), 0.0f).normalized * (4.0f + R(1.5f));
            Vector3 rd = new Vector3(R(), R(), 0.0f) * 0.5f;
            bullets.Shoot(pos + rd, vel);
        }
        Destroy(gameObject);
    }
}
