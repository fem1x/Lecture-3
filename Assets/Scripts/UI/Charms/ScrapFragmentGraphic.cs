using UnityEngine;
using UnityEngine.UI;

namespace _Scripts.UI.Charms
{
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class ScrapFragmentGraphic : MaskableGraphic
    {
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
        private static readonly Vector2[][] Silhouettes =
        {
            new Vector2[] { new(-0.49f, -0.25f), new(-0.22f, 0.43f), new(0.48f, 0.32f), new(0.26f, -0.39f) },
            new Vector2[] { new(-0.43f, -0.31f), new(-0.38f, 0.28f), new(0.28f, 0.46f), new(0.48f, -0.16f) }
        };

        private int _shapeIndex;
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====

        public void SetShape(int index)
        {
            _shapeIndex = index % Silhouettes.Length;
            SetVerticesDirty();
        }

        protected override void OnPopulateMesh(VertexHelper mesh)
        {
            mesh.Clear();
            var bounds = GetPixelAdjustedRect();
            foreach (var point in Silhouettes[_shapeIndex])
                mesh.AddVert(bounds.center + Vector2.Scale(point, bounds.size), color, Vector2.zero);

            mesh.AddTriangle(0, 1, 2);
            mesh.AddTriangle(0, 2, 3);
        }
    }
}
