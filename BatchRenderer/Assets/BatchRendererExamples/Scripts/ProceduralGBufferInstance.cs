using UnityEngine;
using System.Collections;
using Ist;

public class ProceduralGBufferInstance : MonoBehaviour
{
    public ProceduralGBuffer m_renderer;
    Transform m_trans;

    void OnEnable()
    {
        m_trans = GetComponent<Transform>();
    }
    
    void Update()
    {
        if (m_renderer!=null)
        {
            Vector3 pos = m_trans.position;
            //pos.y -= 2.0f;
            m_renderer.AddInstanceTRS(pos, m_trans.rotation, m_trans.localScale);
        }
    }

    //void OnDrawGizmos()
    //{
    //    Transform t = GetComponent<Transform>();
    //    Gizmos.color = Color.yellow;
    //    Gizmos.matrix = t.localToWorldMatrix;
    //    Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
    //    Gizmos.matrix = Matrix4x4.identity;
    //}
}
