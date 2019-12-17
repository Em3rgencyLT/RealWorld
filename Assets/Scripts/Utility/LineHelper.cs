using UnityEngine;

namespace Utility
{
    //Code based on http://csharphelper.com/blog/2014/08/determine-where-two-lines-intersect-in-c/
    public class LineHelper
    {
        /// <summary>
        /// Finds the point where two lines intersect.
        /// <returns>The point of intersection. Vector2.negativeInfinity if none.</returns>
        /// </summary>
        public static Vector2 FindIntersection(Vector2 point1, Vector2 point2, Vector2 point3, Vector2 point4)
        {
            float deltaX1 = point2.x - point1.x;
            float deltaY1 = point2.y - point1.y;
            float deltaX2 = point4.x - point3.x;
            float deltaY2 = point4.y - point3.y;

            float denominator = deltaY1 * deltaX2 - deltaX1 * deltaY2;
            if ((int) denominator == 0)
            {
                //Lines are parallel
                return Vector2.negativeInfinity;
            }

            float t1 = ((point1.x - point3.x) * deltaY2 + (point3.y - point1.y) * deltaX2) / denominator;
            return new Vector2(point1.x + deltaX1 * t1, point1.y + deltaY1 * t1);
        }
    }
}