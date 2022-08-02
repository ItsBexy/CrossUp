// ReSharper disable UnusedMember.Global
namespace CrossUp;
internal sealed partial class CrossUpUI
{
    public static class Strings
    {
        public const string HelpMsg = "Open CrossUp Settings";
        public const string WindowTitle = "CrossUp Settings";

        public static class Terms
        {
            public const string Hotbar = "Hotbar";
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

            public const string LayoutHeader = "Cross Hotbar Layout";
            public const string LeftRightSplit = "Left/Right Separation";
            public const string PadlockIcon = "Padlock Icon";
            public const string SetNumText = "Set # Text";
            public static string ChangeSetText => "CHANGE SET Display";
            public static string LRTriggerText => "Hide L/R Trigger Text";
            public const string UnassignedSlots = "Hide Unassigned Slots";

            public const string ColorHeader = "Colors";

            public const string ColorSubheadBar = "SELECTED BAR";
            public const string ColorBarHighlight = "Backdrop Color";
            public const string ColorBarBlend = "Backdrop Type";
            public const string BlendNormal = "Normal";
            public const string BlendDodge = "Dodge";
            public const string BlendHide = "Hide";

            public const string ColorSubheadButtons = "BUTTONS";
            public const string ColorGlow = "Button Glow";
            public const string ColorPulse = "Button Pulse";

            public const string ColorSubheadTextBorders = "TEXT & BORDERS";
            public const string TextColor = "Text Color";
            public const string TextGlow = "Text Glow Color";
            public const string BorderColor = "Border Color";
            public const string HelpText = "Overrides HUD layout preferences to keep the Cross Hotbar centred horizontally on your screen no matter what." +
                                           "\n\n" +
                                           "You may wish to turn this setting on if you use other plugins that manipulate the HUD, " +
                                           "or if you notice the bar moving left/right unexpectedly.";

            public const string Hide = "Hide";
            public const string LockCenter = "Lock Center";
        }

        public static class SeparateEx
        {
            public const string TabTitle = "Separate Expanded Hold";
            public const string ToggleText = "Display Expanded Hold Controls Separately";
            public const string BarPosition = "Bar Position";
            public const string Warning = "NOTE: This feature functions by borrowing the buttons from two of your standard mouse/keyboard hotbars. " +
                                          "The hotbars you choose will not be overwritten, but they will be unavailable while the feature is active.";

            public const string ShowOnlyOne = "Show Only One Bar";
            public const string ShowBoth = "Show Both";
            public const string PickTwo = "SELECT 2 BARS:";
            
            public static string HudManWarning => "Hi there, HUD Manager user! To make sure that CrossUp and HUD Manager work nicely together, " +
                                                  "it's a good idea to turn off any layout preferences you might have in place for " +
                                                  $"{Terms.Hotbar} {Config.LRborrow + 1} and {Terms.Hotbar} {Config.RLborrow + 1}. " +
                                                  "That way, you won't end up with both plugins fighting over where to put them.";
            public const string OpenHudMan = "Open HUD Manager";
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