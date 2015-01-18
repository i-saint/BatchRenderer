using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class BulletEntity
{
    public Vector3 position;
    public Vector3 velosity;
    public Quaternion rotation;
    public float lifetime;
    public bool is_dead;
}

public class BulletManager : MonoBehaviour
{
    public List<BulletEntity> m_eneitites = new List<BulletEntity>();
    BatchRenderer m_renderer;

    public BulletEntity Shoot(Vector3 pos, Vector3 vel, float lifetime = 10.0f)
    {
        var e = new BulletEntity {
            position = pos,
            velosity = vel,
            rotation = Quaternion.LookRotation(vel),
            lifetime = lifetime,
            is_dead = false
        };
        m_eneitites.Add(e);
        return e;
    }

    void Awake()
    {
        m_renderer = GetComponent<BatchRenderer>();
    }

    void Update ()
    {
        float dt = Time.deltaTime;
        m_eneitites.ForEach((e) => {
            e.position += e.velosity * dt;
            e.lifetime -= dt;
            if (e.lifetime <= 0.0f)
            {
                e.is_dead = true;
            }
        });
        m_eneitites.RemoveAll((e) => { return e.is_dead; });

        m_renderer.AddInstances(m_eneitites.Count, (BatchRenderer.TR[] instances, int start, int n) =>
        {
            for (int i = 0; i < n; ++i )
            {
                instances[start + i].translation = m_eneitites[i].position;
                instances[start + i].rotation = m_eneitites[i].rotation;
            }
        });
    }
}
