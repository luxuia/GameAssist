namespace GameAssist.Models;

public static class Dota2Prompts
{
    public const string Default = "分析这张 DOTA 2 游戏截图。首先识别游戏状态（游戏时间、英雄等级、经济情况），然后根据游戏阶段给出详细分类的建议：\n" +
        "如果是早期（0-5分钟）：详细分析对线技巧、补刀、骚扰和眼位\n" +
        "如果是中期（6-20分钟）：详细分析装备选择、Gank时机、小团战\n" +
        "如果是后期（20分钟以上）：详细分析高地推进、买活时机、Roshan\n" +
        "如果团战爆发：详细分析站位、集火目标、技能释放时机\n" +
        "请按建议类型分类，每个类型单独成行，用中文回复，确保内容详细全面。";

    public const string LaningPhase = "这是一张 DOTA 2 对线期的截图。请详细给出以下分类建议：\n" +
        "1) 补刀和反补技巧\n" +
        "2) 骚扰时机和方法\n" +
        "3) 眼位放置位置和时机\n" +
        "4) 游走支援的最佳时机\n" +
        "每个建议类型单独成行，用中文回复，确保内容详细具体。";

    public const string TeamFight = "这是一张 DOTA 2 团战前或团战中的截图。请详细分析以下分类建议：\n" +
        "1) 对方核心英雄位置和威胁\n" +
        "2) 我方的撤退机会和路线\n" +
        "3) 集火目标的优先级排序\n" +
        "4) 技能使用的最佳时机\n" +
        "每个建议类型单独成行，用中文回复，确保内容详细具体。";

    public const string Itemization = "分析这张 DOTA 2 截图的装备建议。请详细考虑以下分类建议：\n" +
        "1) 当前金币和商店购买建议\n" +
        "2) 装备强势期分析\n" +
        "3) 针对敌方装备的克制方案\n" +
        "4) 核心装与奢侈装的选择建议\n" +
        "每个建议类型单独成行，用中文回复，确保内容详细具体。";

    public const string LateGame = "这是一张 DOTA 2 后期的截图。请详细分析以下分类建议：\n" +
        "1) 上高地的最佳时机和策略\n" +
        "2) 买活的考虑因素\n" +
        "3) 分带机会的分析\n" +
        "4) Roshan/不朽盾的优先级\n" +
        "每个建议类型单独成行，用中文回复，确保内容详细具体。";

    public const string HeroBuild = "请根据这张 DOTA 2 截图分析双方英雄选择（己方5位英雄和敌方5位英雄），然后给出以下详细分类建议：\n\n" +
        "**己方阵容分析**：\n" +
        "- 识别5位英雄的角色定位（1号位、2号位、3号位、4号位、5号位）\n" +
        "- 分析阵容优劣势（控制、爆发、推塔、续航）\n\n" +
        "**装备搭配建议**：\n" +
        "- 针对己方每个英雄推荐核心装备顺序\n" +
        "- 考虑经济节奏（优势期可以出大件，劣势期优先保命装）\n" +
        "- 推荐针对性出装方案（压制敌方阵容的装备）\n\n" +
        "**技能套路建议**：\n" +
        "- 推荐英雄之间的技能联动组合\n" +
        "- 团战时的先手技能顺序\n" +
        "- 反制敌方关键技能的应对策略\n" +
        "- 阵守转换时机\n\n" +
        "请用中文回复，每个建议类型单独成行，确保内容详细全面。";

    /// <summary>
    /// 获取智能提示词，让 AI 自动分析游戏状态并给出建议
    /// </summary>
    public static string GetSmartPrompt()
    {
        return "智能分析这张 DOTA 2 游戏截图。首先识别以下关键信息：\n" +
               "1. 游戏时间（看左上角计时器）\n" +
               "2. 玩家和敌方英雄等级\n" +
               "3. 经济情况（金钱和装备价值）\n" +
               "4. 是否正在团战或即将团战\n" +
               "5. 地图关键区域（Roshan、高地、兵线）\n" +
               "\n根据分析结果，给出详细分类的建议：\n" +
               "- 早期（0-7分钟）：重点在对线、补刀、骚扰\n" +
               "- 中期（8-25分钟）：关注装备、游走、小团战\n" +
               "- 后期（26分钟+）：关注高地、买活、Roshan\n" +
               "如果正在团战，重点分析站位和技能使用\n" +
               "每个建议类型单独成行，用中文回复，确保内容详细全面。";
    }

    /// <summary>
    /// 获取提示词
    /// </summary>
    public static string GetPrompt(PromptType type, string customPrompt = "")
    {
        return type switch
        {
            PromptType.Default => Default,
            PromptType.Smart => HeroBuild,
            PromptType.LaningPhase => LaningPhase,
            PromptType.TeamFight => TeamFight,
            PromptType.Itemization => Itemization,
            PromptType.LateGame => LateGame,
            PromptType.Custom => customPrompt,
            _ => Default
        };
    }

    /// <summary>
    /// 根据游戏提示词类型获取分析关键词
    /// </summary>
    public static string GetAnalysisKeywords(PromptType type)
    {
        return type switch
        {
            PromptType.LaningPhase => "对线,补刀,骚扰,眼位",
            PromptType.TeamFight => "团战,站位,集火,技能",
            PromptType.Itemization => "装备,经济,强势期,克制",
            PromptType.LateGame => "高地,买活,分带,Roshan",
            _ => "位置,装备,发育,配合"
        };
    }
}
