using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;


public class BatchRenderer : BatchRendererBase
{

    public void AddInstanceT(Vector3 t)
    {
        int i = Interlocked.Increment(ref m_instance_count) - 1;
        if (i < m_max_instances)
        {
            m_instance_data.translation[i] = t;
        }
    }
    public void AddInstancesT(Vector3[] t, int start = 0, int length = 0)
    {
        if (length == 0) length = t.Length;
        int reserved_index;
        int reserved_num;
        ReserveInstance(length, out reserved_index, out reserved_num);
        System.Array.Copy(t, start, m_instance_data.translation, reserved_index, reserved_num);
    }

    public void AddInstanceTR(Vector3 t, Quaternion r)
    {
        int i = Interlocked.Increment(ref m_instance_count) - 1;
        if (i < m_max_instances)
        {
            m_instance_data.translation[i] = t;
            m_instance_data.rotation[i] = r;
        }
    }
    public void AddInstancesTR(Vector3[] t, Quaternion[] r, int start = 0, int length = 0)
    {
        if (length == 0) length = t.Length;
        int reserved_index;
        int reserved_num;
        ReserveInstance(length, out reserved_index, out reserved_num);
        System.Array.Copy(t, start, m_instance_data.translation, reserved_index, reserved_num);
        System.Array.Copy(r, start, m_instance_data.rotation, reserved_index, reserved_num);
    }

    public void AddInstanceTRS(Vector3 t, Quaternion r, Vector3 s)
    {
        int i = Interlocked.Increment(ref m_instance_count) - 1;
        if (i < m_max_instances)
        {
            m_instance_data.translation[i] = t;
            m_instance_data.rotation[i] = r;
            m_instance_data.scale[i] = s;
        }
    }
    public void AddInstancesTRS(Vector3[] t, Quaternion[] r, Vector3[] s, int start = 0, int length = 0)
    {
        if (length == 0) length = t.Length;
        int reserved_index;
        int reserved_num;
        ReserveInstance(length, out reserved_index, out reserved_num);
        System.Array.Copy(t, start, m_instance_data.translation, reserved_index, reserved_num);
        System.Array.Copy(r, start, m_instance_data.rotation, reserved_index, reserved_num);
        System.Array.Copy(s, start, m_instance_data.scale, reserved_index, reserved_num);
    }

    public void AddInstanceTRSC(Vector3 t, Quaternion r, Vector3 s, Color c)
    {
        int i = Interlocked.Increment(ref m_instance_count) - 1;
        if (i < m_max_instances)
        {
            m_instance_data.translation[i] = t;
            m_instance_data.rotation[i] = r;
            m_instance_data.scale[i] = s;
            m_instance_data.color[i] = c;
        }
    }

    public void AddInstanceTRSCE(Vector3 t, Quaternion r, Vector3 s, Color c, Color e)
    {
        int i = Interlocked.Increment(ref m_instance_count) - 1;
        if (i < m_max_instances)
        {
            m_instance_data.translation[i] = t;
            m_instance_data.rotation[i] = r;
            m_instance_data.scale[i] = s;
            m_instance_data.color[i] = c;
            m_instance_data.emission[i] = e;
        }
    }

    public void AddInstanceTRC(Vector3 t, Quaternion r, Color c)
    {
        int i = Interlocked.Increment(ref m_instance_count) - 1;
        if (i < m_max_instances)
        {
            m_instance_data.translation[i] = t;
            m_instance_data.rotation[i] = r;
            m_instance_data.color[i] = c;
        }
    }

    public void AddInstanceTRU(Vector3 t, Quaternion r, Vector4 uv)
    {
        int i = Interlocked.Increment(ref m_instance_count) - 1;
        if (i < m_max_instances)
        {
            m_instance_data.translation[i] = t;
            m_instance_data.rotation[i] = r;
            m_instance_data.uv_offset[i] = uv;
        }
    }

    public void AddInstanceTRCU(Vector3 t, Quaternion r, Color c, Vector4 uv)
    {
        int i = Interlocked.Increment(ref m_instance_count) - 1;
        if (i < m_max_instances)
        {
            m_instance_data.translation[i] = t;
            m_instance_data.rotation[i] = r;
            m_instance_data.color[i] = c;
            m_instance_data.uv_offset[i] = uv;
        }
    }

    public void AddInstanceTRSCU(Vector3 t, Quaternion r, Vector3 s, Color c, Vector4 uv)
    {
        int i = Interlocked.Increment(ref m_instance_count) - 1;
        if (i < m_max_instances)
        {
            m_instance_data.translation[i] = t;
            m_instance_data.rotation[i] = r;
            m_instance_data.scale[i] = s;
            m_instance_data.color[i] = c;
            m_instance_data.uv_offset[i] = uv;
        }
    }


    public InstanceData ReserveInstance(int num, out int reserved_index, out int reserved_num)
    {
        reserved_index = Interlocked.Add(ref m_instance_count, num) - num;
        reserved_num = Mathf.Clamp(m_max_instances - reserved_index, 0, num);
        return m_instance_data;
    }


    public enum DataTransferMode
    {
        Buffer,
        TextureWithMesh,
        TextureWithPlugin,
    }

    public enum DataFlags
    {
        Translation = 1 << 0,
        Rotation    = 1 << 1,
        Scale       = 1 << 2,
        Color       = 1 << 3,
        Emission    = 1 << 4,
        UVOffset    = 1 << 5,
        UseBuffer   = 1 << 6,
    }

    [System.Serializable]
    public class InstanceData
    {
        public Vector3[] translation;
        public Quaternion[] rotation;
        public Vector3[] scale;
        public Color[] color;
        public Color[] emission;
        public Vector4[] uv_offset;

        public void Resize(int size)
        {
            translation = new Vector3[size];
            rotation = new Quaternion[size];
            scale = new Vector3[size];
            color = new Color[size];
            emission = new Color[size];
            uv_offset = new Vector4[size];

            Vector3 default_scale = Vector3.one;
            Color default_color = Color.white;
            Vector4 default_uvoffset = new Vector4(1.0f, 1.0f, 0.0f, 0.0f);
            for (int i = 0; i < scale.Length; ++i) { scale[i] = default_scale; }
            for (int i = 0; i < color.Length; ++i) { color[i] = default_color; }
            for (int i = 0; i < uv_offset.Length; ++i) { uv_offset[i] = default_uvoffset; }
        }
    }

    public class InstanceBuffer
    {
        public ComputeBuffer translation;
        public ComputeBuffer rotation;
        public ComputeBuffer scale;
        public ComputeBuffer color;
        public ComputeBuffer emission;
        public ComputeBuffer uv_offset;

        public void Release()
        {
            if (translation != null){ translation.Release(); translation = null; }
            if (rotation != null)   { rotation.Release(); rotation = null; }
            if (scale != null)      { scale.Release(); scale = null; }
            if (color != null)      { color.Release(); color = null; }
            if (emission != null)   { emission.Release(); emission = null; }
            if (uv_offset != null)  { uv_offset.Release(); uv_offset = null; }
        }

        public void Allocate(int num_max_instances)
        {
            Release();
            translation = new ComputeBuffer(num_max_instances, 12);
            rotation = new ComputeBuffer(num_max_instances, 16);
            scale = new ComputeBuffer(num_max_instances, 12);
            color = new ComputeBuffer(num_max_instances, 16);
            emission = new ComputeBuffer(num_max_instances, 16);
            uv_offset = new ComputeBuffer(num_max_instances, 16);
        }
    }

    // I will need this when I make OpenGL implementation
    public class InstanceTexture
    {
        const int texture_width = 1024;

        public RenderTexture translation;
        public RenderTexture rotation;
        public RenderTexture scale;
        public RenderTexture color;
        public RenderTexture emission;
        public RenderTexture uv_offset;

        public void Release()
        {
            if (translation != null) { translation.Release(); translation = null; }
            if (rotation != null) { rotation.Release(); rotation = null; }
            if (scale != null) { scale.Release(); scale = null; }
            if (color != null) { color.Release(); color = null; }
            if (emission != null) { emission.Release(); emission = null; }
            if (uv_offset != null) { uv_offset.Release(); uv_offset = null; }
        }

        RenderTexture CreateDataTexture(int num_max_instances)
        {
            int width = texture_width;
            int height = num_max_instances / texture_width + (num_max_instances % texture_width != 0 ? 1 : 0);
            var r = new RenderTexture(width, height, 0, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Default);
            r.filterMode = FilterMode.Point;
            r.Create();
            return r;
        }

        public void Allocate(int num_max_instances)
        {
            Release();
            translation = CreateDataTexture(num_max_instances);
            rotation = CreateDataTexture(num_max_instances);
            scale = CreateDataTexture(num_max_instances);
            color = CreateDataTexture(num_max_instances);
            emission = CreateDataTexture(num_max_instances);
            uv_offset = CreateDataTexture(num_max_instances);
        }
    }


    public DataTransferMode m_data_transfer_mode;
    public bool m_enable_rotation;
    public bool m_enable_scale;
    public bool m_enable_color;
    public bool m_enable_emission;
    public bool m_enable_uv_offset;

    protected int m_data_flags;

    protected Mesh m_data_transfer_mesh;
    protected Material m_data_transfer_material;

    protected InstanceData m_instance_data;
    protected InstanceBuffer m_instance_buffer;
    protected InstanceTexture m_instance_texture;


    public InstanceBuffer GetInstanceBuffer() { return m_instance_buffer; }
    public InstanceTexture GetInstanceTexture() { return m_instance_texture; }


    public void ReleaseGPUData()
    {
        if (m_instance_buffer != null) { m_instance_buffer.Release(); }
        if (m_instance_texture != null) { m_instance_texture.Release(); }
        m_materials.Clear();
    }

    public void ResetGPUData()
    {
        ReleaseGPUData();

        m_instance_data.Resize(m_max_instances);
        m_instance_buffer.Allocate(m_max_instances);

        // set default values
        UpdateGPUResources();
    }

    public override Material CloneMaterial(int nth)
    {
        Material m = new Material(m_material);
        m.SetInt("g_batch_begin", nth * m_instances_par_batch);
        if (m_instance_buffer != null)
        {
            m.SetBuffer("g_instance_buffer_t", m_instance_buffer.translation);
            m.SetBuffer("g_instance_buffer_r", m_instance_buffer.rotation);
            m.SetBuffer("g_instance_buffer_s", m_instance_buffer.scale);
            m.SetBuffer("g_instance_buffer_color", m_instance_buffer.color);
            m.SetBuffer("g_instance_buffer_emission", m_instance_buffer.emission);
            m.SetBuffer("g_instance_buffer_uv", m_instance_buffer.uv_offset);
        }
        if (m_instance_texture != null)
        {
            m.SetTexture("g_instance_texture_t", m_instance_texture.translation);
            m.SetTexture("g_instance_texture_r", m_instance_texture.rotation);
            m.SetTexture("g_instance_texture_s", m_instance_texture.scale);
            m.SetTexture("g_instance_texture_color", m_instance_texture.color);
            m.SetTexture("g_instance_texture_emission", m_instance_texture.emission);
            m.SetTexture("g_instance_texture_uv", m_instance_texture.uv_offset);
        }

        // fix rendering order for transparent objects
        if (m.renderQueue >= 3000)
        {
            m.renderQueue = m.renderQueue + (nth + 1);
        }
        return m;
    }

    public override void UpdateGPUResources()
    {
        switch(m_data_transfer_mode)
        {
            case DataTransferMode.Buffer:
                UploadInstanceData_Buffer();
                break;
            case DataTransferMode.TextureWithMesh:
                UploadInstanceData_TextureWithMesh();
                break;
            case DataTransferMode.TextureWithPlugin:
                UploadInstanceData_TextureWithPlugin();
                break;
        }
        m_materials.ForEach((v) =>
        {
            v.SetInt("g_data_flags", m_data_flags);
            v.SetInt("g_num_instances", m_instance_count);
            v.SetVector("g_scale", m_scale);
        });
    }

    public void UploadInstanceData_Buffer()
    {
        int data_flags = (int)DataFlags.UseBuffer | (int)DataFlags.Translation;
        m_instance_buffer.translation.SetData(m_instance_data.translation);
        if (m_enable_rotation)
        {
            data_flags |= (int)DataFlags.Rotation;
            m_instance_buffer.rotation.SetData(m_instance_data.rotation);
        }
        if (m_enable_scale)
        {
            data_flags |= (int)DataFlags.Scale;
            m_instance_buffer.scale.SetData(m_instance_data.scale);
        }
        if (m_enable_color)
        {
            data_flags |= (int)DataFlags.Color;
            m_instance_buffer.color.SetData(m_instance_data.color);
        }
        if (m_enable_emission)
        {
            data_flags |= (int)DataFlags.Emission;
            m_instance_buffer.emission.SetData(m_instance_data.emission);
        }
        if (m_enable_uv_offset)
        {
            data_flags |= (int)DataFlags.UVOffset;
            m_instance_buffer.uv_offset.SetData(m_instance_data.uv_offset);
        }
        m_data_flags = data_flags;
    }

    public void UploadInstanceData_TextureWithMesh()
    {
        // todo
    }

    public void UploadInstanceData_TextureWithPlugin()
    {
        // todo
    }


    public override void OnEnable()
    {
        base.OnEnable();
        if (m_mesh == null) return;

        if (m_data_transfer_mode == DataTransferMode.Buffer && !SystemInfo.supportsComputeShaders)
        {
            Debug.Log("BatchRenderer: system does not support ComputeBuffer. use RenderTexture & Mesh to transfer instance data.");
            m_data_transfer_mode = DataTransferMode.TextureWithMesh;
        }

        m_instance_data = new InstanceData();
        if (m_data_transfer_mode == DataTransferMode.Buffer)
        {
            m_instance_buffer = new InstanceBuffer();
        }
        else
        {
            m_instance_texture = new InstanceTexture();
        }

        ResetGPUData();
    }

    public override void OnDisable()
    {
        base.OnDisable();
        ReleaseGPUData();
    }
}
