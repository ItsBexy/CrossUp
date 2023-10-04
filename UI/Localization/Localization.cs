using System;
using System.IO;
using static CrossUp.CrossUp;
using static CrossUp.Utility.Service;

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
            Log.Info($"Loaded localized text ({language})");
        }
        else
        {
            loc.SetupWithFallbacks();
            Log.Info($"Couldn't load localized text ({language}). Using fallback text (en).");
        }
    }
}