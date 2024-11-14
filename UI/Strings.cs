using static Dalamud.Localization;

// ReSharper disable MemberHidesStaticFromOuterClass

namespace CrossUp.UI;

internal static class Strings
{
    public static string HelpMsg                       => Localize("HelpMsg", "Open CrossUp Settings");

    public static class LookAndFeel
    {
        public static string TabTitle                  => Localize("LookFeel", "Look & Feel");
        public static string CrossHotbarLayout         => Localize("CrossHotbarLayout", "Cross Hotbar Layout");
        public static string BarSeparation             => Localize("BarSeparation", "Bar Separation");
        public static string SeparateLR                => Localize("SeparateLR", "Separate Left/Right");
        public static string SplitDistance             => Localize("SplitDistance", "Separation Distance");
        public static string CenterPoint               => Localize("CenterPoint", "Center Point");
        public static string CenterNote                => Localize("CenterNote", "This will override your HUD setting for the bar's horizontal position.");
        public static string BarElements               => Localize("BarElements", "Bar Elements");
        public static string PadlockIcon               => Localize("PadlockIcon", "Padlock Icon");
        public static string SetNumText                => Localize("SetNumText", "SET # Text");
        public static string ChangeSetDisplay          => Localize("ChangeSetDisplay", "CHANGE SET Display");
        public static string HideTriggerText           => Localize("HideTriggerText", "Hide L/R Trigger Text");
        public static string HideUnassignedSlots       => Localize("HideUnassignedSlots", "Hide Unassigned Slots");
        public static string Colors                    => Localize("Colors", "Colors");
        public static string SelectedBar               => Localize("SelectedBar", "Selected Bar");
        public static string BackdropColor             => Localize("BackdropColor", "Backdrop Color");
        public static string ColorBlending             => Localize("ColorBlending", "Color Blending");
        public static string BlendNormal               => Localize("BlendNormal", "Normal");
        public static string BlendDodge                => Localize("BlendDodge", "Dodge");
        public static string BackdropStyle             => Localize("BackdropStyle", "Backdrop Style");
        public static string StyleFrame                => Localize("StyleFrame", "Frame");
        public static string StyleSolid                => Localize("StyleSolid", "Solid");
        public static string StyleHidden               => Localize("StyleHidden", "Hidden");
        public static string Buttons                   => Localize("Buttons", "Buttons");
        public static string ButtonGlow                => Localize("ButtonGlow", "Button Glow");
        public static string ButtonPulse               => Localize("ButtonPulse", "Button Pulse");
        public static string TextAndBorders            => Localize("TextAndBorders", "Text & Borders");
        public static string TextColor                 => Localize("TextColor", "Text Color");
        public static string TextGlowColor             => Localize("TextGlowColor", "Text Glow Color");
        public static string BorderColor               => Localize("BorderColor", "Border Color");
        public static string Hide                      => Localize("Hide", "Hide");
        public static string FadeOutsideCombat         => Localize("FadeOutsideCombat", "Fade Outside Combat");
        public static string InCombat                  => Localize("InCombat", "In Combat");
        public static string OutOfCombat               => Localize("OutOfCombat", "Out of Combat");
    }

    public static class SeparateEx
    {
        public static string TabTitle                  => Localize("SeparateExHold", "Separate Expanded Hold");
        public static string DisplayExSeparately       => Localize("DisplayExSeparately", "Display Expanded Hold Controls Separately");
        public static string BarPosition()             => Localize("BarPosition", "Bar Position");
        public static string BarPosition(string s)     => string.Format(Localize("BarPositionSpecific", "{0} Bar Position"), s);
        public static string SepExWarning              => Localize("SepExWarning", "NOTE: This feature functions by borrowing the buttons from two of your standard mouse/keyboard hotbars. The hotbars you choose will not be overwritten, but they will be unavailable while the feature is active.");
        public static string ShowOnlyOneBar            => Localize("ShowOnlyOneBar", "Show Only One Bar");
        public static string ShowBoth                  => Localize("ShowBoth", "Show Both");
        public static string SelectTwoBars             => Localize("SelectTwoBars", "Select 2 Bars:");
        public static string HotbarN(int n)            => string.Format(Localize("HotbarN", "Hotbar {0}"), n);
    }
    public static class SetSwitching
    {
        public static string TabTitle                  => Localize("SetSwitching", "Set Switching");
        public static string AutoSwitch(string s)      => string.Format(Localize("AutoSwitch", "Auto-switch bar sets for {0}"), s);
        public static string IfUsing                   => Localize("IfUsing", "If Using...");
        public static string MapTo(string s)           => string.Format(Localize("MapTo", "Map {0} to..."), s);
        public static string DoubleTap(string s)       => string.Format(Localize("DoubleTap", "{0} Double Tap"), s);
        public static string WXHB                      => Localize("WXHB", "WXHB");
        public static string ExpandedHoldControls      => Localize("ExpandedHoldControls", "Expanded Hold Controls");
        public static string Left                      => Localize("Left", "Left");
        public static string Right                     => Localize("Right", "Right");
        public static string Set                       => Localize("Set", "Set");
        public static string MenuText(int n, string s) => string.Format(Localize("CrossHotbarMenuTerm", "Cross Hotbar {0} - {1}"), n, s);
    }

    public static class Hud
    {
        public static string TabTitle                  => Localize("HudOptions", "HUD Options");
        public static string HudAllSame                => Localize("HudAllSame", "Apply the same settings to all HUD Slots");
        public static string HudUnique                 => Localize("HudUnique", "Apply different settings to each HUD Slot");
        public static string CurrentHudSlot            => Localize("CurrentHudSlot", "Current HUD Slot:");
        public static string HighlightMsg              => Localize("HighlightMsg", "Settings that are|highlighted|will change with your HUD.");
        public static string CopySettings              => Localize("CopySettings", "Copy Settings");
        public static string From                      => Localize("CopyFrom", "From");
        public static string To                        => Localize("CopyTo", "To");
        public static string Copy                      => Localize("Copy", "Copy");
        public static string AllHudSlots               => Localize("AllHudSlots", "All HUD Slots");
        public static string HudSlot                   => Localize("HudSlot", "HUD Slot");
    }

    public static class TextCommands
    {
        public static string Header                    => Localize("TextCommands", "Text Commands");
        public static string OpenClose                 => Localize("CmdOpenClose", "Opens/Closes the CrossUp Settings window");
        public static string TogglesFeat               => Localize("CmdTogglesFeature", "Toggles the feature");
        public static string TogglesVis                => Localize("CmdTogglesVis", "Toggles visibility");
        public static string SetsX                     => Localize("CmdSetsX", "Sets the horizontal position");
        public static string SetsY                     => Localize("CmdSetsY", "Sets the vertical position");
        public static string SetsSeparationDistance    => Localize("CmdSetsSeparationDistance", "Sets the separation distance");
        public static string SetsCenterPoint           => Localize("CmdSetsCenter", "Sets the center point");
        public static string Split                     => $"{Localize("CmdSplit", "Controls the left/right bar separation")}\n{TogglesFeatIf}";
        public static string Padlock                   => $"{Localize("CmdPadlock", "Controls the Padlock Icon visibility and position")}\n{TogglesVisIf}";
        public static string SetNum                    => $"{Localize("CmdSetNum", "Controls the SET # Text visibility and position")}\n{TogglesVisIf}";
        public static string ChangeSet                 => $"{Localize("CmdChangeSet", "Controls the CHANGE SET Display position")}\n{ResetsToDefault}";
        public static string TriggerText               => Localize("CmdTriggerText", "Toggles the visibility of the L/R trigger text");
        public static string UnassignedSlots           => Localize("CmdUnassignedSlots", "Toggles the visibility of unassigned slots");
        public static string SelectBg                  => $"{Localize("CmdSelectBg", "Controls the selection backdrop")}\n{ResetsToDefault}";
        public static string BgStyle                   => Localize("CmdBgStyle", "Sets the backdrop style\n\nOPTIONS: solid, frame, hidden");
        public static string BgBlend                   => Localize("CmdBgBlend", "Sets the backdrop color blending mode\n\nOPTIONS: normal, dodge");
        public static string BgColor                   => $"{Localize("CmdBgColor", "Sets the backdrop color")}\n\n{UseHex}";
        public static string ButtonColor               => $"{Localize("CmdButtonColor", "Controls the button colors")}\n{ResetsToDefault}";
        public static string ButtonGlow                => $"{Localize("CmdButtonGlow", "Sets the button glow color")}\n\n{UseHex}";
        public static string ButtonPulse               => $"{Localize("CmdButtonPulse", "Sets the button pulse color")}\n\n{UseHex}";
        public static string TextAndBorders            => $"{Localize("CmdTextAndBorders", "Controls the text & border colors")}\n{ResetsToDefault}";
        public static string TextColor                 => $"{Localize("CmdTextColor", "Sets the text color")}\n\n{UseHex}";
        public static string TextGlow                  => $"{Localize("CmdTextGlow", "Sets the text glow color")}\n\n{UseHex}";
        public static string BorderColor               => $"{Localize("CmdBorderColor", "Sets the border element color")}\n\n{UseHex}";
        public static string SepEx                     => $"{Localize("CmdSepEx", "Controls the Separate Expanded Hold options")}\n{TogglesFeatIf}";
        public static string BorrowBar                 => Localize("CmdBorrowBar", "Selects a hotbar to borrow (2-9)");
        public static string OnlyOne                   => Localize("CmdOnlyOne", "Toggles whether only one Expanded Hold bar is shown");
        public static string OnlyOneTip                => Localize("CmdOnlyOneTip", "true: only one\nfalse: both");
        public static string ExPos(string s)           => $"{string.Format(Localize("CmdExPos", "Sets the position of the {0} bar"), s)}\n{ResetsToDefault}";
        public static string Fader                     => $"{Localize("CmdFader", "Controls the combat fader option")}\n{TogglesFeatIf}";
        public static string SetsTransparency          => Localize("CmdTransparency", "Sets transparency (0-100)");
        private static string UseHex                   => Localize("CmdUseHex", "Use hex format (ie, #ffffff)");
        private static string ResetsToDefault          => Localize("CmdResetsToDefault", "Resets to default if no parameters are given");
        private static string TogglesFeatIf            => Localize("CmdTogglesFeatureIf", "Toggles the feature if no parameters are given");
        private static string TogglesVisIf             => Localize("CmdTogglesVisIf", "Toggles visibility if no parameters are given");

        public class ArgLabels
        {
            public static string OnOff                 => Localize("CmdArgOnOff", "on/off");
            public static string ShowHide              => Localize("CmdArShowHide", "show/hide");
            public static string TrueFalse             => Localize("CmdArgTrueFalse", "true/false");
            public static string Distance              => Localize("CmdArgDistance", "distance");
            public static string Center                => Localize("CmdArgCenter", "center");
            public static string Style                 => Localize("CmdArgStyle", "style");
            public static string Blend                 => Localize("CmdArgBlend", "blend");
            public static string Color                 => Localize("CmdArgColor", "color");
            public static string GlowColor             => Localize("CmdArgGlow", "glow color");
            public static string PulseColor            => Localize("CmdArgPulse", "pulse color");
            public static string TextColor             => Localize("CmdArgTextColor", "text color");
            public static string BorderColor           => Localize("CmdArgBorderColor", "border color");
            public static string BorrowBar             => Localize("CmdArgBorrow", "borrowed hotbar");
            public static string CombatTransp          => Localize("CmdArgCombatTransp", "combat transparency");
            public static string IdleTransp            => Localize("CmdArgIdleTransp", "idle transparency");
        }
    }

    public static readonly string[] NumSymbols = ["", "", "", "", ""];
}