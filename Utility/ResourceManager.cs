using System;
using System.IO;
using System.Reflection;
using CheapLoc;
using Dalamud.Logging;

namespace CrossUp;

internal sealed class ResourceManager : IDisposable
{
    internal ResourceManager()
    {
        Setup(Service.PluginInterface.UiLanguage);
        Service.PluginInterface.LanguageChanged += Setup;
    }

    public void Dispose() => Service.PluginInterface.LanguageChanged -= Setup;

    private static void Setup(string language)
    {
        try
        {
            using var resource = Assembly.GetExecutingAssembly().GetManifestResourceStream($"CrossUp.UI.Localization.{language}.json");
            if (resource == null) throw new FileNotFoundException($"Could not find resource file for language [{language}].");

            using var reader = new StreamReader(resource);
            Loc.Setup(reader.ReadToEnd());
            PluginLog.LogWarning($"ResourceManager(Setup): Resource file for language [{language}] loaded successfully.");
        }
        catch (Exception ex)
        {
            PluginLog.LogWarning($"ResourceManager(Setup): Falling back to English resource file.\n{ex}");
            Loc.SetupWithFallbacks();
        }
    }
}