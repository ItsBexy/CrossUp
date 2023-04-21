namespace CrossUp;
internal sealed partial class CrossUpUI
{
    public static class Strings
    {
        public const string HelpMsg = "Open CrossUp Settings";
        public const string WindowTitle = "CrossUp Settings";

        public const string UpdateWarning =
            "Hi! Thanks for using CrossUp! \n\nVersion 0.4.2.0 includes some changes\nunder the hood to the way the plugin\nsettings are saved. Your previous settings\nSHOULD™ have carried over, but you might\nneed to make some adjustments.";

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
            public static string HudSlot => "HUD Slot";
        }

        public static class LookAndFeel
        {
            public const string TabTitle = "Look & Feel";

            public const string LayoutHeader = "Cross Hotbar Layout";
            public const string Split = "BAR SEPARATION";
            public const string SplitOn = "Separate Left/Right";
            public const string SplitDistance = "Separation Distance";
            public const string SplitCenter = "Center Point ";
            public const string SplitNote = "This will override your HUD setting for the bar's horizontal position.";
            public const string BarElements = "BAR ELEMENTS";
            public const string PadlockIcon = "Padlock Icon";
            public const string SetNumText = "SET # Text";
            public static string ChangeSetText => "CHANGE SET Display";
            public static string LRTriggerText => "Hide L/R Trigger Text";
            public const string UnassignedSlots = "Hide Unassigned Slots";

            public const string ColorHeader = "Colors";

            public const string ColorSubheadBar = "SELECTED BAR";
            public const string ColorBarHighlight = "Backdrop Color";
            public const string ColorBarBlend = "Color Blending";
            public const string BlendNormal = "Normal";
            public const string BlendDodge = "Dodge";
            public const string StyleHide = "Hidden";
            public const string ColorBarStyle = "Backdrop Style";
            public const string StyleSolid = "Solid";
            public const string StyleFrame = "Frame";

            public const string ColorSubheadButtons = "BUTTONS";
            public const string ColorGlow = "Button Glow";
            public const string ColorPulse = "Button Pulse";

            public const string ColorSubheadTextBorders = "TEXT & BORDERS";
            public const string TextColor = "Text Color";
            public const string TextGlow = "Text Glow Color";
            public const string BorderColor = "Border Color";

            public const string Hide = "Hide";
            public const string FadeOutsideCombat = "Fade Outside Combat";
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


        }

        public static class Hud
        {
            public const string TabTitle = "HUD Options";
            public const string AllSame = "Apply the same settings to all HUD Slots";
            public const string Unique = "Apply different settings to each HUD Slot";
            public const string Current = "Current HUD Slot:";

            public const string HighlightMsg1 = "Settings that are";
            public const string HighlightMsg2 = "highlighted";
            public const string HighlightMsg3 = "will change with your HUD.";

            public const string CopyProfile = "COPY SETTINGS";
            public const string From = "FROM";
            public const string To = "TO";
            public const string Copy = "COPY";

            public static readonly string AllSlots = "All HUD Slots " + NumSymbols[0];
        }

        public static class BarMapping
        {
            public const string TabTitle = "Bar Assignments";
            public const string UseSetSpecific = "Use set-specific assignments for";
            public const string IfUsing = "If Using...";
            public static string MapTo(string str) => $"Map {str} to...";
        }

        public static readonly string[] NumSymbols = { "", "", "", "", "" };
    }
}