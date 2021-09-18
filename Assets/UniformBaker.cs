using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.VFX;

[VFXType(VFXTypeAttribute.Usage.GraphicsBuffer)]
struct TriangleSampling
{
    public Vector2 coord;
    public int index;
}

public class UniformBaker : MonoBehaviour
{
    private static readonly int s_BufferID = Shader.PropertyToID("bakedSampling");
    private GraphicsBuffer m_Buffer;

    void Start()
    {
        m_Buffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, 1024, Marshal.SizeOf(typeof(TriangleSampling)));
        var fakeData = Enumerable.Range(0, 1024).Select(o => new TriangleSampling { index = o * 32 }).ToArray();
        m_Buffer.SetData(fakeData);

        var vfx = GetComponent<VisualEffect>();
        vfx.SetGraphicsBuffer(s_BufferID, m_Buffer);
    }

    public void OnDisable()
    {
        if (m_Buffer != null)
        {
            m_Buffer.Release();
            m_Buffer = null;
        }
    }

}
