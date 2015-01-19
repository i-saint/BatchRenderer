using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ExamplePlayer : ExampleEntity
{
    public float m_move_speed = 5.0f;

    Rigidbody m_rigid;

    void Awake()
    {
        m_rigid = GetComponent<Rigidbody>();
    }

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
        }
    }
}
