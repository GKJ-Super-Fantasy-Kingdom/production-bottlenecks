using System;

namespace ProductionBottlenecks
{
    internal static class VisitPoint
    {
        // Native magnet radius plus clearance for the camera's 1/32-unit snap.
        internal const float Clearance = .18f;

        internal static (float X, float Y)? Choose(
            (float X, float Y) center, (float X, float Y) pivot, float halfWidth, float halfHeight,
            bool trigger, Func<float, float, bool> clear)
        {
            // A solid collider can attract the camera to its transform on the next
            // input update. An exposed edge cannot fix an ambiguous destination.
            if (!trigger) return clear(pivot.X, pivot.Y) ? pivot : ((float, float)?)null;
            if (clear(center.X, center.Y)) return center;
            if (pivot != center && clear(pivot.X, pivot.Y)) return pivot;

            // Triggers do not attract the camera. Look inside their shape for an
            // exposed point; the caller checks actual collider containment at each.
            foreach (float fraction in new[] { .5f, .85f })
                for (int y = -1; y <= 1; y++)
                    for (int x = -1; x <= 1; x++)
                    {
                        if (x == 0 && y == 0) continue;
                        var point = (X: center.X + x * halfWidth * fraction, Y: center.Y + y * halfHeight * fraction);
                        if (clear(point.X, point.Y)) return point;
                    }
            return null;
        }
    }
}
