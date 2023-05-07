using CheapLoc;

// ReSharper disable MemberHidesStaticFromOuterClass

namespace CrossUp;

internal sealed partial class CrossUpUI
{
    public static class Strings
    {
        public static string HelpMsg => Loc.Localize("HelpMsg", "Open CrossUp Settings");
        public static string UpdateWarning0420 => Loc.Localize("UpdateWarning0420", "Hi! Thanks for using CrossUp! \n\nVersion 0.4.2.0 includes some changes\nunder the hood to the way the plugin\nsettings are saved. Your previous settings\nSHOULD™ have carried over, but you might\nneed to make some adjustments.");
        public static class LookAndFeel
        {
            public static string TabTitle => Loc.Localize("LookFeel", "Look & Feel");
            public static string CrossHotbarLayout => Loc.Localize("CrossHotbarLayout", "Cross Hotbar Layout");
            public static string BarSeparation => Loc.Localize("BarSeparation", "Bar Separation");
            public static string SeparateLR => Loc.Localize("SeparateLR", "Separate Left/Right");
            public static string SplitDistance => Loc.Localize("SplitDistance", "Separation Distance");
            public static string CenterPoint => Loc.Localize("CenterPoint", "Center Point");
            public static string CenterNote => Loc.Localize("CenterNote", "This will override your HUD setting for the bar's horizontal position.");
            public static string BarElements => Loc.Localize("BarElements", "Bar Elements");
            public static string PadlockIcon => Loc.Localize("PadlockIcon", "Padlock Icon");
            public static string SetNumText => Loc.Localize("SetNumText", "SET # Text");
            public static string ChangeSetDisplay => Loc.Localize("ChangeSetDisplay", "CHANGE SET Display");
            public static string HideTriggerText => Loc.Localize("HideTriggerText", "Hide L/R Trigger Text");
            public static string HideUnassignedSlots => Loc.Localize("HideUnassignedSlots", "Hide Unassigned Slots");
            public static string Colors => Loc.Localize("Colors", "Colors");
            public static string SelectedBar => Loc.Localize("SelectedBar", "Selected Bar");
            public static string BackdropColor => Loc.Localize("BackdropColor", "Backdrop Color");
            public static string ColorBlending => Loc.Localize("ColorBlending", "Color Blending");
            public static string BlendNormal => Loc.Localize("BlendNormal", "Normal");
            public static string BlendDodge => Loc.Localize("BlendDodge", "Dodge");
            public static string BackdropStyle => Loc.Localize("BackdropStyle", "Backdrop Style");
            public static string StyleFrame => Loc.Localize("StyleFrame", "Frame");
            public static string StyleSolid => Loc.Localize("StyleSolid", "Solid");
            public static string StyleHidden => Loc.Localize("StyleHidden", "Hidden");
            public static string Buttons => Loc.Localize("Buttons", "Buttons");
            public static string ButtonGlow => Loc.Localize("ButtonGlow", "Button Glow");
            public static string ButtonPulse => Loc.Localize("ButtonPulse", "Button Pulse");
            public static string TextAndBorders => Loc.Localize("TextAndBorders", "Text & Borders");
            public static string TextColor => Loc.Localize("TextColor", "Text Color");
            public static string TextGlowColor => Loc.Localize("TextGlowColor", "Text Glow Color");
            public static string BorderColor => Loc.Localize("BorderColor", "Border Color");
            public static string Hide => Loc.Localize("Hide", "Hide");
            public static string FadeOutsideCombat => Loc.Localize("FadeOutsideCombat", "Fade Outside Combat");
            public static string InCombat => Loc.Localize("InCombat","In Combat");
            public static string OutOfCombat => Loc.Localize("OutOfCombat", "Out of Combat");
        }

        public static class SeparateEx
        {
            public static string TabTitle => Loc.Localize("SeparateExHold", "Separate Expanded Hold");
            public static string DisplayExSeparately => Loc.Localize("DisplayExSeparately",  "Display Expanded Hold Controls Separately");
            public static string BarPosition => Loc.Localize("BarPosition",  "Bar Position");
            public static string BarPositionSpecific(string str) => string.Format(Loc.Localize("BarPositionSpecific", "{0} Bar Position"), str);
            public static string SepExWarning => Loc.Localize("SepExWarning",  "NOTE: This feature functions by borrowing the buttons from two of your standard mouse/keyboard hotbars. The hotbars you choose will not be overwritten, but they will be unavailable while the feature is active.");
            public static string ShowOnlyOneBar => Loc.Localize("ShowOnlyOneBar",  "Show Only One Bar");
            public static string ShowBoth => Loc.Localize("ShowBoth",  "Show Both");
            public static string SelectTwoBars => Loc.Localize("SelectTwoBars",  "Select 2 Bars:");
            public static string HotbarN(int n) => string.Format(Loc.Localize("HotbarN", "Hotbar {0}"), n);
        }
        public static class SetSwitching
        {
            public static string TabTitle => Loc.Localize("SetSwitching", "Set Switching");
            public static string UseSetSpecific(string str) => string.Format(Loc.Localize("AutoSwitch", "Auto-switch bar sets for {0}"), str);
            public static string IfUsing => Loc.Localize("IfUsing", "If Using...");
            public static string MapTo(string str) => string.Format(Loc.Localize("MapTo", "Map {0} to..."), str);
            public static string DoubleTap(string str) => string.Format(Loc.Localize("DoubleTap", "{0} Double Tap"), str);
            public static string WXHB => Loc.Localize("WXHB", "WXHB");
            public static string ExpandedHoldControls => Loc.Localize("ExpandedHoldControls", "Expanded Hold Controls");
            public static string Left => Loc.Localize("Left", "Left");
            public static string Right => Loc.Localize("Right", "Right");
            public static string Set => Loc.Localize("Set", "Set");
            public static string MenuText(int num, string side) => string.Format(Loc.Localize("CrossHotbarMenuTerm", "Cross Hotbar {0} - {1}"), num,side);
        }

        public static class Hud
        {
            public static string TabTitle => Loc.Localize("HudOptions",  "HUD Options");
            public static string HudAllSame => Loc.Localize("HudAllSame",  "Apply the same settings to all HUD Slots");
            public static string HudUnique => Loc.Localize("HudUnique",  "Apply different settings to each HUD Slot");
            public static string CurrentHudSlot => Loc.Localize("CurrentHudSlot",  "Current HUD Slot:");
            public static string HighlightMsg => Loc.Localize("HighlightMsg", "Settings that are|highlighted|will change with your HUD.");
            public static string CopySettings => Loc.Localize("CopySettings",  "Copy Settings");
            public static string From => Loc.Localize("CopyFrom",  "From");
            public static string To => Loc.Localize("CopyTo",  "To");
            public static string Copy => Loc.Localize("Copy",  "Copy");
            public static string AllHudSlots => Loc.Localize("AllHudSlots", "All HUD Slots");
            public static string HudSlot => Loc.Localize("HudSlot", "HUD Slot");
        }

        public static class TextCommands
        {
            public static string Header => Loc.Localize("TextCommands", "Text Commands");
            public static string OpenClose => Loc.Localize("CmdOpenClose", "Opens/Closes the CrossUp Settings window");
            public static string TogglesFeat => Loc.Localize("CmdTogglesFeature", "Toggles the feature");
            public static string TogglesVis => Loc.Localize("CmdTogglesVis", "Toggles visibility");
            public static string SetsX => Loc.Localize("CmdSetsX", "Sets the horizontal position");
            public static string SetsY => Loc.Localize("CmdSetsY", "Sets the vertical position");
            public static string SetsSeparationDistance => Loc.Localize("CmdSetsSeparationDistance", "Sets the separation distance");
            public static string SetsCenterPoint => Loc.Localize("CmdSetsCenter", "Sets the center point");
            public static string Split => $"{Loc.Localize("CmdSplit", "Controls the left/right bar separation")}\n{TogglesFeatIf}";
            public static string Padlock => $"{Loc.Localize("CmdPadlock", "Controls the Padlock Icon visibility and position")}\n{TogglesVisIf}";
            public static string SetNum => $"{Loc.Localize("CmdSetNum", "Controls the SET # Text visibility and position")}\n{TogglesVisIf}";
            public static string ChangeSet => $"{Loc.Localize("CmdChangeSet", "Controls the CHANGE SET Display position")}\n{ResetsToDefault}";
            public static string TriggerText => Loc.Localize("CmdTriggerText", "Toggles the visibility of the L/R trigger text");
            public static string UnassignedSlots => Loc.Localize("CmdUnassignedSlots", "Toggles the visibility of unassigned slots"); 
            public static string SelectBg => $"{Loc.Localize("CmdSelectBg", "Controls the selection backdrop")}\n{ResetsToDefault}";
            public static string BgStyle => Loc.Localize("CmdBgStyle", "Sets the backdrop style\n\nOPTIONS: solid, frame, hidden");
            public static string BgBlend => Loc.Localize("CmdBgBlend", "Sets the backdrop color blending mode\n\nOPTIONS: normal, dodge");
            public static string BgColor => $"{Loc.Localize("CmdBgColor", "Sets the backdrop color")}\n\n{UseHex}";
            public static string ButtonColor => $"{Loc.Localize("CmdButtonColor", "Controls the button colors")}\n{ResetsToDefault}";
            public static string ButtonGlow => $"{Loc.Localize("CmdButtonGlow", "Sets the button glow color")}\n\n{UseHex}";
            public static string ButtonPulse => $"{Loc.Localize("CmdButtonPulse", "Sets the button pulse color")}\n\n{UseHex}";
            public static string TextAndBorders => $"{Loc.Localize("CmdTextAndBorders", "Controls the text & border colors")}\n{ResetsToDefault}";
            public static string TextColor => $"{Loc.Localize("CmdTextColor", "Sets the text color")}\n\n{UseHex}";
            public static string TextGlow => $"{Loc.Localize("CmdTextGlow", "Sets the text glow color")}\n\n{UseHex}";
            public static string BorderColor => $"{Loc.Localize("CmdBorderColor", "Sets the border element color")}\n\n{UseHex}";
            public static string SepEx => $"{Loc.Localize("CmdSepEx", "Controls the Separate Expanded Hold options")}\n{TogglesFeatIf}";
            public static string BorrowBar => Loc.Localize("CmdBorrowBar", "Selects a hotbar to borrow (2-9)");
            public static string OnlyOne => Loc.Localize("CmdOnlyOne", "Toggles whether only one Expanded Hold bar is shown");
            public static string OnlyOneTip => Loc.Localize("CmdOnlyOneTip", "true: only one\nfalse: both");
            public static string ExPos(string str) => $"{string.Format(Loc.Localize("CmdExPos", "Sets the position of the {0} bar"), str)}\n{ResetsToDefault}";
            public static string Fader => $"{Loc.Localize("CmdFader", "Controls the combat fader option")}\n{TogglesFeatIf}";
            public static string SetsTransparency => Loc.Localize("CmdTransparency", "Sets transparency (0-100)");
            private static string UseHex => Loc.Localize("CmdUseHex", "Use hex format (ie, #ffffff)");
            private static string ResetsToDefault => Loc.Localize("CmdResetsToDefault", "Resets to default if no parameters are given");
            private static string TogglesFeatIf => Loc.Localize("CmdTogglesFeatureIf", "Toggles the feature if no parameters are given");
            private static string TogglesVisIf => Loc.Localize("CmdTogglesVisIf", "Toggles visibility if no parameters are given");

        }

        public static readonly string[] NumSymbols = { "", "", "", "", "" };
    }
}