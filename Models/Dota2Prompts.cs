namespace GameAssist.Models;

public static class Dota2Prompts
{
    public const string Default = "Analyze this Dota 2 screenshot and provide concise gameplay suggestions. " +
        "Focus on: 1) Hero positioning, 2) Itemization status, 3) Farm priorities, " +
        "4) Team coordination opportunities. Keep response under 3 sentences if possible.";

    public const string LaningPhase = "This is a Dota 2 laning phase screenshot. Provide quick tips for: " +
        "1) Last hitting and denying, 2) Harassment windows, 3) Ward placement, " +
        "4) When to rotate. Keep response to 2-3 short sentences.";

    public const string TeamFight = "This is a Dota 2 screenshot during or before a team fight. Analyze: " +
        "1) Enemy carry positioning, 2) Disengage opportunities, 3) Focus targets, " +
        "4) Spell usage priorities. Keep response to 2-3 sentences.";

    public const string Itemization = "Analyze the Dota 2 screenshot for itemization advice. Consider: " +
        "1) Current gold and shop availability, 2) Power spike timing, " +
        "3) Enemy counter-items needed, 4) Core vs luxury items. Keep response to 2-3 sentences.";

    public const string LateGame = "This is a late game Dota 2 screenshot. Analyze: " +
        "1) High ground push timing, 2) Buyback considerations, " +
        "3) Split push opportunities, 4) Roshan/Aegis priority. Keep response to 2-3 sentences.";

    public static string GetPrompt(PromptType type, string customPrompt = "")
    {
        return type switch
        {
            PromptType.Default => Default,
            PromptType.LaningPhase => LaningPhase,
            PromptType.TeamFight => TeamFight,
            PromptType.Itemization => Itemization,
            PromptType.LateGame => LateGame,
            PromptType.Custom => customPrompt,
            _ => Default
        };
    }
}
