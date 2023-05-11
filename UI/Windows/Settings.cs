using CrossUp.UI.Tabs;
using ImGuiNET;
using static CrossUp.CrossUp;

namespace CrossUp.UI.Windows
{

    internal sealed class SettingsWindow
    {
        private bool show;
        internal bool Show
        {
            get => show;
            set => show = value;
        }

        public void Draw()
        {
            ImGui.SetNextWindowSizeConstraints(new(500 * Helpers.Scale, 450 * Helpers.Scale), new(9999f));
            ImGui.SetNextWindowSize(Config.ConfigWindowSize, ImGuiCond.Always);
            if (!ImGui.Begin("CrossUp", ref show, ImGuiWindowFlags.NoScrollbar)) return;

            if (ImGui.BeginTabBar("Nav"))
            {
                LookAndFeel.DrawTab();
                SeparateEx.DrawTab();
                SetSwitching.DrawTab();
                HudOptions.DrawTab();
                TextCommands.DrawTab();

                ImGui.EndTabBar();
            }

            Config.ConfigWindowSize = ImGui.GetWindowSize();
            ImGui.End();
        }
    }
}