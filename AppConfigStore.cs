using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace NotePadSummary;

public sealed class AppConfig
{
    public string? SystemPrompt { get; set; }
}

internal static class AppConfigStore
{
    // Keep this in-code as requested; users can override via config in Documents.
    // This matches the "old" behavior/style expectations (bullets, compact, NL).
    public const string DefaultSystemPrompt = """
Je schrijft in het Nederlands.

Stijlregels:
- Gebruik korte, natuurlijke zinnen.
- Gebruik bullets met een streepje (-) wanneer je een samenvatting/overzicht geeft.
- Houd tekst compact en zakelijk (geen lange uitleg).
- Technische termen en namen behouden.
- Schrijf waar mogelijk in tegenwoordige formulering (bijv. "Ik heb contact gehad met...", "Gebruiker gaat morgen...").
- Voeg geen voorwoorden, excuses of meta-tekst toe; geef direct het resultaat.
""";

    // Migration: earlier version used a generic default. If users never changed anything, don't treat that as an override.
    private const string LegacyDefaultSystemPrompt =
        "Je bent een betrouwbare assistent. Volg de regels in de prompt strikt. " +
        "Geef alleen het resultaat, zonder extra uitleg of voorwoorden.";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public static string ConfigDirectory =>
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "NotePadSummary");

    public static string ConfigPath => Path.Combine(ConfigDirectory, "config.json");

    public static AppConfig Load()
    {
        try
        {
            if (!File.Exists(ConfigPath))
                return new AppConfig();

            var json = File.ReadAllText(ConfigPath);
            var cfg = JsonSerializer.Deserialize<AppConfig>(json, JsonOptions) ?? new AppConfig();

            // If a config contains the legacy default, clear it so the new default takes effect.
            if (string.Equals(NormalizeSystemPrompt(cfg.SystemPrompt), LegacyDefaultSystemPrompt, StringComparison.Ordinal))
                cfg.SystemPrompt = null;

            return cfg;
        }
        catch
        {
            // If config is unreadable, fall back to defaults and keep the app usable.
            return new AppConfig();
        }
    }

    public static void Save(AppConfig config)
    {
        Directory.CreateDirectory(ConfigDirectory);

        // Don't persist default value as an override. If user sets it back to default, clear it.
        var normalized = NormalizeSystemPrompt(config.SystemPrompt);
        if (string.Equals(normalized, DefaultSystemPrompt, StringComparison.Ordinal))
            config.SystemPrompt = null;
        else
            config.SystemPrompt = normalized;

        var json = JsonSerializer.Serialize(config, JsonOptions);
        File.WriteAllText(ConfigPath, json);
    }

    public static string GetEffectiveSystemPrompt(AppConfig config)
    {
        var normalized = NormalizeSystemPrompt(config.SystemPrompt);
        if (string.IsNullOrWhiteSpace(normalized))
            return DefaultSystemPrompt;
        return normalized!;
    }

    private static string? NormalizeSystemPrompt(string? value)
    {
        if (value == null) return null;
        var trimmed = value.Trim();
        return trimmed.Length == 0 ? null : trimmed;
    }
}
