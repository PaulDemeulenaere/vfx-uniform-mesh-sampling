using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.VFX;
using System;

namespace UnityEngine.VFX
{
    [ExecuteInEditMode]
    public class UniformBaker : MonoBehaviour
    {
        [Delayed]
        public int Seed = 0x123;
        [Min(1)]
        public int SampleCount = 2048;
        [Delayed]
        public string MeshPropertyName = "skinnedMeshRenderer";
        [Delayed]
        public string GraphicsBufferName = "bakedSampling";

        [SerializeField]
        TriangleSampling[] m_BakedSampling;

        private GraphicsBuffer m_Buffer;

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

            Mesh mesh;
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
        }

        public virtual void ApplyCustomOrdering(MeshData meshData, ref TriangleSampling[] bakedSampling)
        {
        }

        public void OnValidate()
        {
            //Lazy update & bind only for editor
            var vfx = GetComponent<VisualEffect>();
            if (m_BakedSampling == null || m_BakedSampling.Length != SampleCount)
            {
                ComputeBakedSampling();
            }

            if (m_Buffer == null || m_Buffer.count != SampleCount)
            {
                UpdateGraphicsBuffer();
            }
            BindGraphicsBuffer(vfx);
        }
#endif
        public void UpdateGraphicsBuffer()
        {
            if (SampleCount != m_BakedSampling.Length)
            {
                Debug.LogErrorFormat("The length of baked data mismatches with sample count : {0} vs {1}", SampleCount, m_BakedSampling.Length);
                return;
            }

            if (m_Buffer != null)
            {
                m_Buffer.Release();
                m_Buffer = null;
            }

            m_Buffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, SampleCount, Marshal.SizeOf(typeof(TriangleSampling)));
            m_Buffer.SetData(m_BakedSampling);
        }

        public void BindGraphicsBuffer(VisualEffect vfx)
        {
            vfx.SetGraphicsBuffer(GraphicsBufferName, m_Buffer);
        }

        public void Start()
        {
            var vfx = GetComponent<VisualEffect>();
            UpdateGraphicsBuffer();
            BindGraphicsBuffer(vfx);
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
