using System.Linq;
namespace UnityEngine.VFX
{
    [ExecuteInEditMode]
    public class UniformBakerCustomOrder : UniformBaker
    {
#if UNITY_EDITOR
        public override void ApplyCustomOrdering(MeshData meshData, ref TriangleSampling[] bakedSampling)
        {
            base.ApplyCustomOrdering(meshData, ref bakedSampling);

            bakedSampling = bakedSampling.OrderBy(o =>
            {
                var vertex = VFXMeshSamplingHelper.GetInterpolatedVertex(meshData, o);
                var texCoord = vertex.uvs[0];
                return (texCoord - new Vector4(0.5f, 0.5f, 0.5f, 0.5f)).sqrMagnitude;

                //Alternative:
                //var sampledColor = m_OrderTexture.GetPixelBilinear(texCoord.x, texCoord.y);
                //return sampledColor.r;
            }).ToArray();
        }
#endif
    }
}
