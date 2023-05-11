using System;
using System.IO;
using Dalamud.Logging;
using static CrossUp.CrossUp;

namespace CrossUp.UI.Localization;

internal sealed class CrossUpLoc : IDisposable
{

    internal CrossUpLoc()
    {
        UpdateLang(PluginInterface.UiLanguage);
        PluginInterface.LanguageChanged += UpdateLang;
    }

    public void Dispose() => PluginInterface.LanguageChanged -= UpdateLang;

    private static void UpdateLang(string language)
    {
        var dirPath = Path.Combine(PluginInterface.AssemblyLocation.DirectoryName!, @"UI\Localization\");
        var loc = new Dalamud.Localization(dirPath);

        if (File.Exists($"{dirPath}{language}.json"))
        {
            loc.SetupWithLangCode(language);
            PluginLog.Log($"Loaded localized text ({language})");
        }
        else
        {
            loc.SetupWithFallbacks();
            PluginLog.LogWarning($"Couldn't load localized text ({language}). Using fallback text.");
        }
    }
}