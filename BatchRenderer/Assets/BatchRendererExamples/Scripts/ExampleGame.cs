using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Ist;

public class ExampleGame : MonoBehaviour
{
    static ExampleGame s_instance;
    public static ExampleGame GetInstance() { return s_instance; }
    public static ExampleBulletManager GetBulletManager() { return s_instance.m_bullets; }
    public static ExamplePlayer GetPlayer() { return s_instance.m_player; }
    public static BatchRenderer GetCubeRenderer() { return s_instance.m_cube_renderer; }
    public static BatchRenderer GetSphereRenderer() { return s_instance.m_sphere_renderer; }

    public ExampleBulletManager m_bullets;
    public ExamplePlayer m_player;
    public BatchRenderer m_cube_renderer;
    public BatchRenderer m_sphere_renderer;

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
