namespace GameAssist.Models;

public static class Dota2Prompts
{
    public const string Default = "分析这张 DOTA 2 游戏截图。首先识别游戏状态（游戏时间、英雄等级、经济情况），然后根据游戏阶段给出针对性的建议：\n" +
        "如果是早期（0-5分钟）：关注对线技巧、补刀、骚扰和眼位\n" +
        "如果是中期（6-20分钟）：关注装备选择、Gank时机、小团战\n" +
        "如果是后期（20分钟以上）：关注高地推进、买活时机、Roshan\n" +
        "如果团战爆发：关注站位、集火目标、技能释放时机\n" +
        "最后给出最关键的建议，用中文回复，控制在3句话以内。";

    public const string LaningPhase = "这是一张 DOTA 2 对线期的截图。请快速给出：1) 补刀和反补技巧，2) 骚扰时机，3) 眼位放置，4) 何时游走。 " +
        "请用中文回复，控制在2-3句话。";

    public const string TeamFight = "这是一张 DOTA 2 团战前或团战中的截图。分析：1) 对方核心英雄位置，2) 撤退机会，3) 集火目标，4) 技能使用优先级。 " +
        "请用中文回复，控制在2-3句话。";

    public const string Itemization = "分析这张 DOTA 2 截图的装备建议。考虑：1) 当前金币和商店购买情况，2) 装备强势期，3) 需要针对敌方装备，4) 核心装vs奢侈装。 " +
        "请用中文回复，控制在2-3句话。";

    public const string LateGame = "这是一张 DOTA 2 后期的截图。分析：1) 上高地时机，2) 买活考虑因素，3) 分带机会，4) Roshan/不朽盾优先级。 " +
        "请用中文回复，控制在2-3句话。";

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
               "\n根据分析结果，给出针对性的建议：\n" +
               "- 早期（0-7分钟）：重点在对线、补刀、骚扰\n" +
               "- 中期（8-25分钟）：关注装备、游走、小团战\n" +
               "- 后期（26分钟+）：关注高地、买活、Roshan\n" +
               "如果正在团战，重点分析站位和技能使用\n" +
               "用中文回复，给出最关键的建议，控制在3句话以内。";
    }

    /// <summary>
    /// 获取智能选择的提示词，根据游戏情况自动选择最合适的提示词类型
    /// </summary>
    /// <param name="gameTime">游戏时间（分钟）</param>
    /// <param name="playerLevel">玩家等级</param>
    /// <param name="netWorth">玩家经济净价值</param>
    /// <param type="teamLevel">平均团队等级</param>
    /// <param type="teamNetWorth">团队经济净价值</param>
    /// <returns>最合适的提示词</returns>
    public static string GetSmartPrompt(int gameTime, int playerLevel, long netWorth,
                                       int teamLevel, long teamNetWorth)
    {
        // 分析游戏阶段和情况
        if (gameTime <= 5)
        {
            // 早期对线期
            return LaningPhase;
        }
        else if (gameTime <= 15)
        {
            // 中期，查看是否有团战迹象
            if (teamLevel >= 6 && playerLevel >= 6)
            {
                // 有关键技能，可能发生团战
                return TeamFight;
            }
            else
            {
                // 仍然是发育阶段
                return Itemization;
            }
        }
        else if (gameTime <= 30)
        {
            // 中期到后期过渡
            if (netWorth >= 20000 || teamNetWorth >= 50000)
            {
                // 经济领先，可以开始分带
                return Itemization;
            }
            else
            {
                // 激烈期，团战频繁
                return TeamFight;
            }
        }
        else
        {
            // 后期
            return LateGame;
        }
    }

    /// <summary>
    /// 获取提示词
    /// </summary>
    public static string GetPrompt(PromptType type, string customPrompt = "")
    {
        return type switch
        {
            PromptType.Default => Default,
            PromptType.Smart => GetSmartPrompt(),
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
