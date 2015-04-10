using UnityEngine;
using System.Collections;

public class FrameRateCounter : MonoBehaviour
{
    // Attach this to a GUIText to make a frames/second indicator.
    //
    // It calculates frames/second over each updateInterval,
    // so the display does not keep changing wildly.
    //
    // It is also fairly accurate at very low FPS counts (<10).
    // We do this not by simply counting frames per interval, but
    // by accumulating FPS for each frame. This way we end up with
    // correct overall FPS even if the interval renders something like
    // 5.5 frames.

    public float m_update_interval = 0.5f;
    public bool m_show_GUI = true;
    public Vector2 m_GUI_position = new Vector2(5, 5);

    private float m_accum = 0.0f; // FPS accumulated over the interval
    private int m_frames = 0; // Frames drawn over the interval
    private float m_time_left; // Left time for current interval
    private float m_last_result;

    void Start()
    {
        m_time_left = m_update_interval;
    }

    void Update()
    {
        m_time_left -= Time.deltaTime;
        m_accum += Time.timeScale / Time.deltaTime;
        ++m_frames;

        // Interval ended - update result
        if (m_time_left <= 0.0)
        {
            m_last_result = m_accum / m_frames;
            m_time_left = m_update_interval;
            m_accum = 0.0f;
            m_frames = 0;
        }
    }

    void OnGUI()
    {
        if (!m_show_GUI) return;
        GUI.Label(new Rect(m_GUI_position.x, m_GUI_position.y, 100, 20), m_last_result.ToString("f2"));
    }
}

