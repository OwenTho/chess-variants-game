
using Godot;
using Godot.Collections;

public partial class RuleCard : CardBase
{
    public string PieceId;
    public string RuleId;
    public string RuleName;
    public string RuleDescription;
    
    public RuleCard (string ruleId, string ruleName = "No rule name provided.", string ruleDescription = "No rule description provided.")
    {
        RuleId = ruleId;
        RuleName = ruleName;
        RuleDescription = ruleDescription;
    }

    public override void OnAddCard(GameState game)
    {
        game.AddActionRule(RuleId, PieceId);
    }

    public override string GetCardName(GameState game)
    {
        return RuleName;
    }

    public override string GetCardDescription(GameState game)
    {
        return $"{game.GetPieceName(PieceId)} can {RuleDescription}";
    }


    protected override CardBase CloneCard()
    {
        RuleCard newCard = new RuleCard(RuleId, RuleName, RuleDescription);
        newCard.PieceId = PieceId;
        return newCard;
    }

    protected override Dictionary<string, string> ToDict(GameState game)
    {
        return new Dictionary<string, string>
        {
            { "piece_id", PieceId },
            { "rule_id", RuleId },
            { "rule_name", RuleName },
            { "rule_description", RuleDescription }
        };
    }

    public override void FromDict(GameState game, Dictionary<string, string> dataDict)
    {
        if (!dataDict.TryGetValue("piece_id", out PieceId))
        {
            GD.PushError("Data did not contain the value for the piece id.");
        }

        if (!dataDict.TryGetValue("rule_id", out RuleId))
        {
            GD.PushError("Data did not contain the value for the rule id.");
        }

        if (!dataDict.TryGetValue("rule_name", out RuleName))
        {
            GD.PushError("Data did not contain the value for the rule name.");
        }

        if (!dataDict.TryGetValue("rule_description", out RuleDescription))
        {
            GD.PushError("Data did not contain the value for the rule description.");
        }
    }
}