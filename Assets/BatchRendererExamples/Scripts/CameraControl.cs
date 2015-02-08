using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Threading;


public class CameraControl : MonoBehaviour
{
    public bool m_rotate_by_time = false;
    public float m_rotate_speed = -10.0f;
    public Camera m_camera;


    void Awake()
    {
        if (m_camera == null)
        {
            m_camera = GetComponent<Camera>();
        }
    }

    void Update()
    {
        if (Input.GetKeyUp(KeyCode.R)) { m_rotate_by_time = !m_rotate_by_time; }

        if (m_camera == null) return;

        Vector3 pos = m_camera.transform.position;
        if (m_rotate_by_time)
        {
            pos = Quaternion.Euler(0.0f, Time.deltaTime * m_rotate_speed, 0) * pos;
        }
        if (Input.GetMouseButton(0))
        {
            float ry = Input.GetAxis("Mouse X") * 3.0f;
            float rxz = Input.GetAxis("Mouse Y") * 0.25f;
            pos = Quaternion.Euler(0.0f, ry, 0) * pos;
            pos.y += rxz;
        }
        {
            float wheel = Input.GetAxis("Mouse ScrollWheel");
            pos += pos.normalized * wheel * 4.0f;
        }
        m_camera.transform.position = pos;
        m_camera.transform.LookAt(new Vector3(0.0f, -2.0f, 0.0f));
    }
}
