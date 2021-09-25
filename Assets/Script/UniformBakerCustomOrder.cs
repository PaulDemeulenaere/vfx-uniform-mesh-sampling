using System.Linq;
namespace UnityEngine.VFX
{
    public class UniformBakerCustomOrder : UniformBaker
    {
#if UNITY_EDITOR
        public override void ApplyCustomOrdering(MeshData meshData, ref TriangleSampling[] bakedSampling)
        {
            base.ApplyCustomOrdering(meshData, ref bakedSampling);
            var refPosition = new[]
            {
                new Vector3(200, 0, 0),
                new Vector3(-200, 0, 0),
                new Vector3(0, 200, 0),
                new Vector3(0, -200, 0),
            };

            var boundVertices = new Vector3[refPosition.Length];
            for (int i = 0; i < refPosition.Length; ++i)
            {
                var position = refPosition[i];
                var currentMinLength = float.MaxValue;
                foreach (var v in meshData.vertices)
                {
                    var currentLength = (v.position - position).sqrMagnitude;
                    if (currentLength < currentMinLength)
                    {
                        currentMinLength = currentLength;
                        boundVertices[i] = v.position;
                    }
                }
            }

            bakedSampling = bakedSampling.OrderBy(o =>
            {
                var vertex = VFXMeshSamplingHelper.GetInterpolatedVertex(meshData, o);
                return boundVertices.Select(p => (vertex.position - p).sqrMagnitude).Min();
            }).ToArray();
        }
#endif
    }
}
