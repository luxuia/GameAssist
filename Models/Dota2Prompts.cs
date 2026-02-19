namespace GameAssist.Models;

public static class Dota2Prompts
{
    public const string Default = "分析这张 DOTA 2 游戏截图并提供简洁的游戏建议。 " +
        "重点关注：1) 英雄位置，2) 装备情况，3) 打钱优先级，4) 团队配合机会。 " +
        "请用中文回复，控制在3句话以内。";

    public const string LaningPhase = "这是一张 DOTA 2 对线期的截图。请快速给出：1) 补刀和反补技巧，2) 骚扰时机，3) 眼位放置，4) 何时游走。 " +
        "请用中文回复，控制在2-3句话。";

    public const string TeamFight = "这是一张 DOTA 2 团战前或团战中的截图。分析：1) 对方核心英雄位置，2) 撤退机会，3) 集火目标，4) 技能使用优先级。 " +
        "请用中文回复，控制在2-3句话。";

    public const string Itemization = "分析这张 DOTA 2 截图的装备建议。考虑：1) 当前金币和商店购买情况，2) 装备强势期，3) 需要针对敌方装备，4) 核心装vs奢侈装。 " +
        "请用中文回复，控制在2-3句话。";

    public const string LateGame = "这是一张 DOTA 2 后期的截图。分析：1) 上高地时机，2) 买活考虑因素，3) 分带机会，4) Roshan/不朽盾优先级。 " +
        "请用中文回复，控制在2-3句话。";

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
