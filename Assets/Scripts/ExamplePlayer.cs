using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ExamplePlayer : MonoBehaviour
{
    Transform m_trans;

    void Awake()
    {
        m_trans = GetComponent<Transform>();
    }

    void Update()
    {
    }
}
