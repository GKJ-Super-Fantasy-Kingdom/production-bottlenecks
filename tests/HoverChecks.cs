using ProductionBottlenecks;

internal static class HoverChecks
{
    internal static void Run(System.Action<string, bool> check)
    {
        const string title = "Lumberyard";
        const string blocked = "Lumberyard\nNo worker assigned";
        const string full = "Lumberyard\nOutput storage full";

        var area = (0f, 0f, 1920f, 1080f);
        var size = (164f, 71f);
        check("centered tooltip preserves native anchor", HoverTooltip.FitBelowFocus((960, 500), size, area, 540) == (960, 500));
        check("left edge keeps resource name and icon on screen", HoverTooltip.FitBelowFocus((25, 447), size, area, 487) == (82, 447));
        check("right edge respects the safe area at enlarged scale",
            HoverTooltip.FitBelowFocus((1900, 400), (246, 106), (20, 10, 1900, 1060), 440) == (1777, 400));
        check("bottom edge flips above pointer with native gap", HoverTooltip.FitBelowFocus((1094, 5), size, area, 45) == (1094, 156));
        check("worker count centers below controller focus at 1x",
            HoverTooltip.FitBelowFocus((960, 516), (80, 42), area, 540) == (960, 516));
        check("large worker count clears a lower-right safe area corner at 1.5x",
            HoverTooltip.FitBelowFocus((1890, 9), (120, 63), (20, 10, 1900, 1060), 45) == (1840, 144));

        check("native Lumberyard title gains the blocker",
            HoverTooltip.NextText(title, "No worker assigned", title, null) == blocked);
        check("switching blockers updates an open tooltip",
            HoverTooltip.NextText(title, "Output storage full", blocked, blocked) == full);
        check("resuming work restores the native title",
            HoverTooltip.NextText(title, null, blocked, blocked) == title);
        check("worker assignment popup keeps priority",
            HoverTooltip.NextText(title, "No worker assigned", "Workers 1/2", blocked) == null);
        check("localized native warning can be decorated",
            HoverTooltip.NextText(title, "Needs Tree", "Baeume fehlen", null, "Baeume fehlen") == "Lumberyard\nNeeds Tree");
        check("vanilla full warning for a non-stored output restores the title",
            HoverTooltip.NextText(title, null, "Storage full", null, "Storage full", emptyOutputWarning: true) == title);
        check("unconfirmed native input warning stays visible during active work",
            HoverTooltip.NextText(title, null, "Needs Wood", null, "Needs Wood") == null);
        check("valid native storage warning stays visible during active work",
            HoverTooltip.NextText(title, null, "Storage full", null, "Storage full") == null);
        check("clearing our warning preserves another mod's newly opened card",
            HoverTooltip.NextText(title, null, "Pin cost", blocked) == null);
        check("another mod retains its custom warning",
            HoverTooltip.NextText(title, "Needs Tree", "Custom job paused", null, "Baeume fehlen") == null);
        check("closed tooltip can display a blocker",
            HoverTooltip.NextText(title, "No worker assigned", null, null) == blocked);
        check("unchanged status does not reopen the tooltip",
            HoverTooltip.NextText(title, "No worker assigned", blocked, blocked) == null);
        check("healthy native title stays in place",
            HoverTooltip.NextText(title, null, title, null) == null);
        check("clearing a blocker leaves a closed tooltip closed",
            HoverTooltip.NextText(title, null, null, blocked) == null);
    }
}
