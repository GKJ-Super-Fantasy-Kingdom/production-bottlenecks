namespace ProductionBottlenecks
{
    internal static class HoverTooltip
    {
        // Null means the current tooltip should be left alone.
        internal static string NextText(string title, string reason, string visible, string owned, string nativeWarning = null)
        {
            if (visible != null && visible != title && visible != owned && visible != nativeWarning)
                return null;

            if (reason == null)
                return visible != null && (visible == owned || visible == nativeWarning) ? title : null;

            string message = title + "\n" + reason;
            return message != visible ? message : null;
        }
    }
}
