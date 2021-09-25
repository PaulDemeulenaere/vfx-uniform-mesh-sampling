using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.VFX;
using System;

namespace UnityEngine.VFX
{
    public class UniformBaker : MonoBehaviour
    {
        private static readonly int s_BufferID = Shader.PropertyToID("bakedSampling");
        private static readonly int s_SkinnedMeshID = Shader.PropertyToID("skinnedMeshRenderer");

        private GraphicsBuffer m_Buffer;

        [Delayed]
        public int Seed = 0x123;
        [Delayed, Min(1)]
        public int SampleCount = 2048;
        [Delayed]
        public string MeshPropertyName = "skinnedMeshRenderer";
        [Delayed]
        public string GraphicsBufferName = "bakedSampling";

        [SerializeField]
        TriangleSampling[] m_BakedSampling;

//ComputeBakedSampling could be executed in runtime but this computation isn't optimized for runtime
#if UNITY_EDITOR
        public void ComputeBakedSampling()
        {
            var vfx = GetComponent<VisualEffect>();
            if (vfx == null)
            {
                Debug.LogWarning("UniformBaker expects a VisualEffect on the shared game object.");
                return;
            }

            if (!vfx.HasGraphicsBuffer(GraphicsBufferName))
            {
                Debug.LogWarningFormat("Graphics Buffer property '{0}' is invalid.", GraphicsBufferName);
                return;
            }

            Mesh mesh = null;
            var meshPropertyNameID = Shader.PropertyToID(MeshPropertyName);
            if (vfx.HasSkinnedMeshRenderer(meshPropertyNameID))
            {
                mesh = vfx.GetSkinnedMeshRenderer(meshPropertyNameID).sharedMesh;
            }
            else if (vfx.HasMesh(meshPropertyNameID))
            {
                mesh = vfx.GetMesh(meshPropertyNameID);
            }
            else
            {
                Debug.LogWarningFormat("Mesh property '{0}' is invalid.", MeshPropertyName);
                return;
            }

            if (mesh == null)
            {
                Debug.LogWarningFormat("Unexpected null mesh.");
                return;
            }

            var meshData = VFXMeshSamplingHelper.ComputeDataCache(mesh);

            var rand = new System.Random(Seed);
            m_BakedSampling = new TriangleSampling[SampleCount];
            for (int i = 0; i < SampleCount; ++i)
            {
                m_BakedSampling[i] = VFXMeshSamplingHelper.GetNextSampling(meshData, rand);
            }
            ApplyCustomOrdering(meshData, ref m_BakedSampling);

            UpdateAndBindGraphisBuffer(vfx);
        }

        public virtual void ApplyCustomOrdering(MeshData meshData, ref TriangleSampling[] bakedSampling)
        {
        }

        public void OnValidate()
        {
            //TODO: Use better UX than invalidate which can be called several time
            //ComputeBakedSampling();
        }
#endif

        public void UpdateAndBindGraphisBuffer(VisualEffect vfx)
        {
            if (m_Buffer != null)
            {
                m_Buffer.Release();
                m_Buffer = null;
            }

            if (m_BakedSampling == null)
            {
                Debug.LogError("Unexpected null baked sampling.");
                return;
            }

            if (SampleCount != m_BakedSampling.Length)
            {
                Debug.LogErrorFormat("The length of baked data mismatches with sample count : {0} vs {1}", SampleCount, m_BakedSampling.Length);
                return;
            }

            m_Buffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, SampleCount, Marshal.SizeOf(typeof(TriangleSampling)));
            m_Buffer.SetData(m_BakedSampling);
            vfx.SetGraphicsBuffer(GraphicsBufferName, m_Buffer);
        }

        public void Start()
        {
            var vfx = GetComponent<VisualEffect>();
            UpdateAndBindGraphisBuffer(vfx);
#if !UNITY_EDITOR
            //Optional: In runtime, we can now free the serialized data
            m_BakedSampling = null;
#endif
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
}
