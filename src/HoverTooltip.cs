namespace ProductionBottlenecks
{
    // One instance per focus. An initially empty tooltip can still show a new shortage.
    internal struct HoverVisibility
    {
        private string lastReason;
        private bool wasOpen;
        private bool dismissed;

        internal bool Observe(string reason, bool open)
        {
            if (reason != lastReason || reason == null)
                dismissed = false;
            else if (wasOpen && !open)
                dismissed = true;

            lastReason = reason;
            wasOpen = open;
            return dismissed;
        }
    }

    internal static class HoverTooltip
    {
        // The native top pivot centers the frame horizontally below its anchor.
        internal static (float X, float Y) FitBelowFocus((float X, float Y) anchor,
            (float Width, float Height) size, (float Left, float Bottom, float Right, float Top) area,
            float focusY)
        {
            float left = System.Math.Max(area.Left, System.Math.Min(anchor.X - size.Width / 2, area.Right - size.Width));
            float top = anchor.Y;
            if (top - size.Height < area.Bottom)
                top = focusY + (focusY - anchor.Y) + size.Height;
            top = System.Math.Min(area.Top, System.Math.Max(top, area.Bottom + size.Height));
            return (left + size.Width / 2, top);
        }

        // Null means the current tooltip should be left alone.
        internal static string NextText(string title, string reason, string visible, string owned, string nativeWarning = null, bool emptyOutputWarning = false)
        {
            if (visible != null && visible != title && visible != owned && visible != nativeWarning)
                return null;

            if (reason == null)
                return visible != null && (visible == owned || (emptyOutputWarning && visible == nativeWarning)) ? title : null;

            string message = title + "\n" + reason;
            return message != visible ? message : null;
        }
    }
}
