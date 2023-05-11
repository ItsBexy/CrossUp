using CrossUp.UI.Tabs;
using ImGuiNET;
using static CrossUp.CrossUp;

// ReSharper disable InconsistentNaming

namespace CrossUp.UI.Windows
{
    internal sealed class DebugWindow
    {
        private bool show;
        internal bool Show
        {
            get => show;
            set => show = value;
        }

        public void Draw()
        {
            ImGui.SetNextWindowSize(Config.DebugWindowSize, ImGuiCond.Always);

            if (!ImGui.Begin("CrossUp Debug Tools", ref show)) return;

            if (ImGui.BeginTabBar("DebugTabs"))
            {
                DebugConfig.DrawTab();

                ImGui.EndTabBar();
            }

            Config.DebugWindowSize = ImGui.GetWindowSize();
            ImGui.End();
        }
    }
}