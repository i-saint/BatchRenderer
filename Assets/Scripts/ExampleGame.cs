using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ExampleGame : MonoBehaviour
{
    static ExampleGame s_instance;
    public ExampleBulletManager m_bullets;
    public ExamplePlayer m_player;

    public static ExampleGame GetInstance() { return s_instance; }
    public static ExampleBulletManager GetBulletManager() { return s_instance.m_bullets; }
    public static ExamplePlayer GetPlayer() { return s_instance.m_player; }

    void OnEnable()
    {
        s_instance = this;
    }

    void OnDisable()
    {
        if (s_instance == this) s_instance = null;
    }

    void Update()
    {
    }
}
