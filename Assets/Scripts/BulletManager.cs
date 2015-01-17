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
    }

    void Update ()
    {
        float dt = Time.deltaTime;
        m_eneitites.ForEach((e) => { e.position += e.velosity * dt; });
        m_eneitites.RemoveAll((e) => { return e.is_dead; });
    }
}
