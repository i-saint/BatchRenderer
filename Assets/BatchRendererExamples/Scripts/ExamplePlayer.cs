using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ExamplePlayer : ExampleEntity
{
    public float m_move_speed = 5.0f;

    public override void Update()
    {
        base.Update();
        {
            // move
            Vector3 move_dir = Vector3.zero;
            move_dir.x = Input.GetAxis("Horizontal");
            move_dir.y = Input.GetAxis("Vertical");
            m_rigid.velocity = move_dir * m_move_speed;
        }
        {
            // look mouse cursor
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            Plane plane = new Plane(new Vector3(0.0f, 0.0f, 1.0f), Vector3.zero);
            float distance = 0;
            if (plane.Raycast(ray, out distance))
            {
                m_rigid.rotation = Quaternion.LookRotation(ray.GetPoint(distance) - m_trans.position);
            }
            m_rigid.angularVelocity = Vector3.zero;
        }
        if (Input.GetButton("Fire1"))
        {
            ExampleBulletManager bm = ExampleGame.GetBulletManager();
            Vector3 center = m_trans.position;
            Vector3 direction = m_trans.forward;
            for (int i = 0; i < 32; ++i )
            {
                Vector3 pos = center + new Vector3(R(), R(), 0.0f).normalized * R() * 0.75f;
                bm.Shoot(pos, direction*20.0f, 5.0f, m_bcol.m_id);
            }
        }
    }
}
