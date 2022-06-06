namespace CrossUp;

internal partial class CrossUpUI
{
    public static class Strings
    {
        public const string HelpMsg = "Open CrossUp Settings";
        public const string WindowTitle = "CrossUp Settings";

        public static class Terms
        {
            public const string CrossHotbar = "Cross Hotbar";
            public const string ExpandedHold = "Expanded Hold Controls";
            public const string WXHB = "WXHB";
            public const string Left = "Left";
            public const string Right = "Right";
            public static string LRinput => "L→R";
            public static string RLinput => "R→L";
            public static string LLinput => "L Double Tap";
            public static string RRinput => "R Double Tap";
            public static string Set => "SET";
        }
        public static class LookAndFeel
        {
            public const string TabTitle = "Look & Feel";

            public const string LayoutHeader = "CROSS HOTBAR LAYOUT";
            public const string LeftRightSplit = "Left/Right Separation";
            public const string PadlockIcon = "Padlock Icon";
            public const string SetNumText = "Set # Text";
            public static string ChangeSetText => "CHANGE SET Display";
            public static string LRTriggerText => "Hide L/R Trigger Text";
            public const string UnassignedSlots = "Hide Unassigned Slots";

            public const string ColorHeader = "CUSTOMIZE COLORS";
            public const string ColorBarHighlight = "Bar Highlight";
            public const string ColorGlow = "Button Glow";
            public const string ColorPulse = "Button Pulse";
        }
        public static class SeparateEx
        {
            public const string TabTitle = "Separate Expanded Hold";
            public const string ToggleText = "Display Expanded Hold Controls Separately";
            public const string BarPosition = "Bar Position";
            public const string Warning =
                "NOTE: This feature functions by borrowing the buttons from two of your standard mouse/kb hotbars. The hotbars you choose will not be overwritten, but they will be unavailable while the feature is active.";

            public const string ShowOnlyOne = "Show Only One Bar";
            public const string ShowBoth = "Show Both";
            public const string PickTwo = "SELECT 2 BARS:";
        }
        public static class BarMapping
        {
            public const string TabTitle = "Bar Assignments";
            public const string UseSetSpecific = "Use set-specific assignments for";
            public const string IfUsing = "If Using...";
            public static string MapTo(string str) => $"Map {str} to...";
        }
    }
}