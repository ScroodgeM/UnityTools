using UnityEngine;
using UnityTools.UnityRuntime.UI.Element;

namespace UnityTools.UnityRuntime.Helpers
{
    public static class RectTransformHelpers
    {
        private static readonly Vector3[] fourCornersCache = new Vector3[4];

        public static bool Overlaps(this ElementBase elementA, ElementBase elementB)
        {
            return Overlaps(elementA.transform as RectTransform, elementB.transform as RectTransform);
        }

        public static bool Overlaps(this RectTransform rectA, RectTransform rectB)
        {
            rectA.GetWorldCorners(fourCornersCache);
            GetMinMax(fourCornersCache, 0, out float xMin1, out float xMax1);
            GetMinMax(fourCornersCache, 1, out float yMin1, out float yMax1);

            rectB.GetWorldCorners(fourCornersCache);
            GetMinMax(fourCornersCache, 0, out float xMin2, out float xMax2);
            GetMinMax(fourCornersCache, 1, out float yMin2, out float yMax2);

            return xMin1 <= xMax2 && xMin2 <= xMax1 && yMin1 <= yMax2 && yMin2 <= yMax1;
        }

        private static void GetMinMax(Vector3[] points, int dimensionIndex, out float min, out float max)
        {
            min = points[0][dimensionIndex];
            max = points[0][dimensionIndex];
            for (int i = 1; i < points.Length; i++)
            {
                min = Mathf.Min(min, points[i][dimensionIndex]);
                max = Mathf.Max(max, points[i][dimensionIndex]);
            }
        }
    }
}
