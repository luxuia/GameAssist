using System.Text.Json;

namespace GameAssist.Models;

public class AppConfig
{
    // API Provider and Configuration
    public ApiProvider Provider { get; set; } = ApiProvider.OpenAI;

    // OpenAI API Configuration
    public string OpenAiApiKey { get; set; } = string.Empty;
    public string OpenAiEndpoint { get; set; } = "https://api.openai.com/v1/chat/completions";

    // Zhipu AI (GLM) API Configuration
    public string ZhipuApiKey { get; set; } = string.Empty;
    public string ZhipuEndpoint { get; set; } = "https://open.bigmodel.cn/api/paas/v4/chat/completions";

    // Model Configuration
    public string ModelName { get; set; } = "gpt-4o";

    // Helper property to get default model based on provider
    public string GetDefaultModel()
    {
        return Provider switch
        {
            ApiProvider.ZhipuAI => "glm-4.6v",
            _ => "gpt-4o"
        };
    }
    public int MaxTokens { get; set; } = 500;
    public double Temperature { get; set; } = 0.7;

    // Analysis Settings
    public int IntervalSeconds { get; set; } = 60;
    public bool Dota2Only { get; set; } = true;
    public bool ShowPreview { get; set; } = true;
    public PromptType SelectedPrompt { get; set; } = PromptType.Default;
    public string CustomPrompt { get; set; } = string.Empty;

    // Overlay Settings
    public bool OverlayEnabled { get; set; } = true;
    public int OverlayWidth { get; set; } = 600;
    public int OverlayFontSize { get; set; } = 16;
    public int OverlayAutoHideSeconds { get; set; } = 30;

    // Application Settings
    public bool AutoStart { get; set; } = false;

    public static string ConfigPath => System.IO.Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "GameAssist",
        "config.json");

    public static AppConfig Load()
    {
        try
        {
            var dir = System.IO.Path.GetDirectoryName(ConfigPath);
            if (dir != null && !System.IO.Directory.Exists(dir))
                System.IO.Directory.CreateDirectory(dir);

            if (!System.IO.File.Exists(ConfigPath))
            {
                var cfg = new AppConfig();
                Save(cfg);
                return cfg;
            }

            var json = System.IO.File.ReadAllText(ConfigPath);
            return JsonSerializer.Deserialize<AppConfig>(json) ?? new AppConfig();
        }
        catch
        {
            return new AppConfig();
        }
    }

    public static void Save(AppConfig config)
    {
        try
        {
            var dir = System.IO.Path.GetDirectoryName(ConfigPath);
            if (dir != null && !System.IO.Directory.Exists(dir))
                System.IO.Directory.CreateDirectory(dir);

            var json = JsonSerializer.Serialize(config, new JsonSerializerOptions { WriteIndented = true });
            System.IO.File.WriteAllText(ConfigPath, json);
        }
        catch
        {
            // Silently fail - in production this should be logged
        }
    }
}

public enum PromptType
{
    Default,
    LaningPhase,
    TeamFight,
    Itemization,
    LateGame,
    Custom
}

public enum ApiProvider
{
    OpenAI,
    ZhipuAI
}
