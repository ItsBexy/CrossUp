using System;
using System.IO;
using Dalamud.Logging;
using static CrossUp.CrossUp;

namespace CrossUp.UI.Localization;

internal sealed class CrossUpLoc : IDisposable
{
    internal CrossUpLoc()
    {
        SetupLanguage(PluginInterface.UiLanguage);
        PluginInterface.LanguageChanged += SetupLanguage;
    }

    public void Dispose() => PluginInterface.LanguageChanged -= SetupLanguage;

    private static void SetupLanguage(string language)
    {
        var dirPath = Path.Combine(PluginInterface.AssemblyLocation.DirectoryName!, @"UI\Localization\");
        var filePath = $"{dirPath}{language}.json";
        var loc = new Dalamud.Localization(dirPath);

        if (File.Exists(filePath))
        {
            loc.SetupWithLangCode(language);
            PluginLog.Log($"Loaded localized text ({language})");
        }
        else
        {
            loc.SetupWithFallbacks();
            PluginLog.LogWarning($"Couldn't load localized text ({language}). Using fallback text (en).");
        }
    }
}