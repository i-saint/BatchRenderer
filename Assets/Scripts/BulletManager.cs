using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class BulletEntity
{
    public Vector3 position;
    public Vector3 velosity;
    public bool is_dead;
}

public class BulletManager : MonoBehaviour
{
    public List<BulletEntity> m_eneitites = new List<BulletEntity>();
    BatchRenderer m_renderer;
    Matrix4x4[] m_instance_matrices;
    Vector3[] m_instance_positions;

    public BulletEntity Shoot(Vector3 pos, Vector3 vel)
    {
        var e = new BulletEntity {
            position = pos,
            velosity = vel,
            is_dead = false
        };
        m_eneitites.Add(e);
        return e;
    }

    void Awake()
    {
        m_renderer = GetComponent<BatchRenderer>();

        const int num = 65536;
        m_instance_matrices = new Matrix4x4[num];
        m_instance_positions = new Vector3[num];
        Vector3 scale = Vector3.one * 0.2f;
        Quaternion rot = Quaternion.identity;
        for (int i = 0; i < num; ++i)
        {
            Vector3 pos = new Vector3(
                0.25f * (i / 256) - 32.0f,
                Random.Range(-0.5f, -0.1f),
                0.25f * (i % 256) - 32.0f);
            m_instance_positions[i] = pos;
            m_instance_matrices[i] = Matrix4x4.TRS(pos, rot, scale);
        }
    }

    void Update ()
    {
        float dt = Time.deltaTime;
        m_eneitites.ForEach((e) => { e.position += e.velosity * dt; });
        m_eneitites.RemoveAll((e) => { return e.is_dead; });

        switch(m_renderer.GetDataType()) {
            case BatchRenderer.DataType.Matrix:
                m_renderer.AddInstances(m_instance_matrices, 0, m_instance_matrices.Length);
                break;
            case BatchRenderer.DataType.Position:
                m_renderer.AddInstances(m_instance_positions, 0, m_instance_matrices.Length);
                break;
        }
    }
}
