using ProductionBottlenecks;

internal static class HoverChecks
{
    internal static void Run(System.Action<string, bool> check)
    {
        const string title = "Lumberyard";
        const string blocked = "Lumberyard\nNo worker assigned";
        const string full = "Lumberyard\nOutput storage full";

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
            HoverTooltip.NextText(title, null, "Storage full", null, "Storage full") == title);
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
