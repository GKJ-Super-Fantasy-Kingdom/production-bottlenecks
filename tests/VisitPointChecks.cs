using System;
using ProductionBottlenecks;

internal static class VisitPointChecks
{
    // Installed Human prefab Click boxes, including their transform scale, at
    // the positions recorded in the copied morning save. Dimensions are half sizes.
    private readonly record struct Box(string Name, float X, float Y, float Half)
    {
        public bool Contains(float x, float y) => Math.Abs(x - X) <= Half && Math.Abs(y - Y) <= Half;
        public bool MeetsMagnet(float x, float y)
        {
            float dx = Math.Max(0, Math.Abs(x - X) - Half);
            float dy = Math.Max(0, Math.Abs(y - Y) - Half);
            return dx * dx + dy * dy <= VisitPoint.Clearance * VisitPoint.Clearance;
        }
    }

    internal static void Run(Action<string, bool> check)
    {
        var plots = new[] {
            new Box("Castle", 0, -15, 2), new Box("Quarry", -5, -13, 1), new Box("Tavern", -2, -10, 1),
            new Box("Lumberyard", 0, -22, 1), new Box("House", -3, -14, .5f), new Box("Sawmill", 3, -17, 1),
            new Box("Goldmine", -5, -21, 1), new Box("Church", -1, -19.5f, 1), new Box("House", -3, -12, .5f),
            new Box("Stone vein", -7, -11, 1), new Box("House", -3, -18, .5f)
        };
        foreach (var plot in plots)
        {
            bool Clear(float x, float y) => plot.Contains(x, y) && !Array.Exists(plots, b => b != plot && b.MeetsMagnet(x, y));
            var center = (plot.X, plot.Y);
            check($"vanilla {plot.Name} at {center} keeps map focus",
                VisitPoint.Choose(center, center, plot.Half, plot.Half, false, Clear) == center);
        }

        var own = new Box("Lumberyard", 0, 0, 1);
        var neighbor = new Box("Lumberyard", -.38f, 0, 1);
        bool Exposed(float x, float y) => own.Contains(x, y) && !neighbor.MeetsMagnet(x, y);
        var origin = (0f, 0f);
        check("an edge is exposed in the crowded fixture", Exposed(.85f, 0));
        check("solid collider would magnet back into the overlap; request exact inspection",
            VisitPoint.Choose(origin, origin, 1, 1, false, Exposed) == null);
        var aim = VisitPoint.Choose(origin, origin, 1, 1, true, Exposed);
        check("a trigger can use an exposed point without a magnet warp", aim.HasValue && Exposed(aim.Value.X, aim.Value.Y));
        check("offset collider uses the exposed native magnet destination",
            VisitPoint.Choose(origin, (.9f, 0f), 1, 1, false, Exposed) == (.9f, 0f));
        check("fully coincident plots request exact inspection",
            VisitPoint.Choose(origin, origin, 1, 1, true, (x,y) => own.Contains(x,y) && !own.MeetsMagnet(x,y)) == null);
    }
}
