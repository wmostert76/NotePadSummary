using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace NotePadSummary;

public sealed class AppConfig
{
    // Legacy (v1). Kept for backward compat.
    public string? SystemPrompt { get; set; }

    // Current.
    public string? SummarySystemPrompt { get; set; }
    public string? SalesOpportunitySystemPrompt { get; set; }
}

internal static class AppConfigStore
{
    // Keep this in-code as requested; users can override via config in Documents.
    // This matches the "old" behavior/style expectations (bullets, compact, NL).
    public const string DefaultSummarySystemPrompt = """
Je schrijft in het Nederlands.

Stijlregels:
- Gebruik korte, natuurlijke zinnen.
- Gebruik bullets met een streepje (-) wanneer je een samenvatting/overzicht geeft.
- Houd tekst compact en zakelijk (geen lange uitleg).
- Technische termen en namen behouden.
- Schrijf waar mogelijk in tegenwoordige formulering (bijv. "Ik heb contact gehad met...", "Gebruiker gaat morgen...").
- Voeg geen voorwoorden, excuses of meta-tekst toe; geef direct het resultaat.
""";

    public const string DefaultSalesOpportunitySystemPrompt = """
Je schrijft in het Nederlands.

Stijlregels:
- Output bestaat uit maximaal 3 bullets.
- Elke regel begint met een streepje (-).
- Korte, amicale zinnen. Niet te formeel.
- Combineer waar mogelijk situatie + impact in 1 zin.
- Focus op praktisch advies met hardwarevervanging als logische stap bij oude of instabiele apparatuur.
- Niet verwijzen naar endpoint/monitoring services.
- Geen labels zoals "Situatie:", "Impact:", "Advies:" of "Vervolgstap:".
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

            // v1 -> v2 migration: treat legacy SystemPrompt as the SummarySystemPrompt.
            if (NormalizeSystemPrompt(cfg.SummarySystemPrompt) == null && NormalizeSystemPrompt(cfg.SystemPrompt) != null)
                cfg.SummarySystemPrompt = cfg.SystemPrompt;

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

        config.SummarySystemPrompt = NormalizeSystemPrompt(config.SummarySystemPrompt);
        config.SalesOpportunitySystemPrompt = NormalizeSystemPrompt(config.SalesOpportunitySystemPrompt);

        // Don't persist defaults as overrides.
        if (string.Equals(config.SummarySystemPrompt, DefaultSummarySystemPrompt, StringComparison.Ordinal))
            config.SummarySystemPrompt = null;
        if (string.Equals(config.SalesOpportunitySystemPrompt, DefaultSalesOpportunitySystemPrompt, StringComparison.Ordinal))
            config.SalesOpportunitySystemPrompt = null;

        // Keep legacy property empty in new versions to avoid confusion.
        config.SystemPrompt = null;

        var json = JsonSerializer.Serialize(config, JsonOptions);
        File.WriteAllText(ConfigPath, json);
    }

    public enum PromptKind
    {
        Summary,
        SalesOpportunity
    }

    public static string GetEffectiveSystemPrompt(AppConfig config, PromptKind kind)
    {
        var value = kind switch
        {
            PromptKind.Summary => NormalizeSystemPrompt(config.SummarySystemPrompt) ?? NormalizeSystemPrompt(config.SystemPrompt),
            PromptKind.SalesOpportunity => NormalizeSystemPrompt(config.SalesOpportunitySystemPrompt),
            _ => null
        };

        if (!string.IsNullOrWhiteSpace(value))
            return value!;

        return kind switch
        {
            PromptKind.Summary => DefaultSummarySystemPrompt,
            PromptKind.SalesOpportunity => DefaultSalesOpportunitySystemPrompt,
            _ => DefaultSummarySystemPrompt
        };
    }

    private static string? NormalizeSystemPrompt(string? value)
    {
        if (value == null) return null;
        var trimmed = value.Trim();
        return trimmed.Length == 0 ? null : trimmed;
    }
}
