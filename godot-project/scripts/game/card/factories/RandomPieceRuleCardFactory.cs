
using System.Collections.Generic;
using Godot;

public partial class RandomPieceRuleCardFactory : CardFactory
{
    public string[] RuleIds;
    public string RuleName;
    public string RuleDescription;
    
    public int MaxOccurrences;

    public RandomPieceRuleCardFactory(string[] ruleIds, string ruleName, string ruleDescription = "No description provided.", int maxOccurrences = 0)
    {
        RuleIds = ruleIds;
        RuleName = ruleName;
        RuleDescription = ruleDescription;
        MaxOccurrences = maxOccurrences;
    }
    
    public RandomPieceRuleCardFactory(string ruleId, string ruleName, string ruleDescription = "No description provided.", int maxOccurrences = 0)
        : this(new []{ ruleId }, ruleName, ruleDescription, maxOccurrences)
    {

    }

    private bool IsRuleUnderMax(GameState game, PieceInfo info, string ruleId)
    {
        if (info.TryGetPieceRule(game.GetActionRule(ruleId), out PieceRule rule))
        {
            if (rule.level >= MaxOccurrences)
            {
                return false;
            }
        }

        return true;
    }
    
    private bool PieceCanGetRule(GameState game, PieceInfo info)
    {
        if (MaxOccurrences <= 0)
        {
            return true;
        }
        
        foreach (var ruleId in RuleIds)
        {
            if (IsRuleUnderMax(game, info, ruleId))
            {
                return true;
            }
        }

        return false;
    }
    
    protected override CardBase CreateCard(GameState game)
    {

        List<PieceInfo> validPieceInfo = new List<PieceInfo>();

        foreach (var info in game.GetPieceInfoOnBoard())
        {
            // If a piece can get at least one rule, then add it to the
            // above list
            if (PieceCanGetRule(game, info))
            {
                validPieceInfo.Add(info);
            }
        }

        if (validPieceInfo.Count == 0)
        {
            GD.PushError($"{GetType().Name} Couldn't find a valid PieceInfo.");
            return null;
        }
        
        int randomInd = game.gameRandom.RandiRange(0, validPieceInfo.Count - 1);
        PieceInfo chosenInfo = validPieceInfo[randomInd];
        
        // With a random pieceId selected, get the possible rules it can take
        // and only use those for the card.
        List<string> ruleIds = new List<string>();
        foreach (var ruleId in RuleIds)
        {
            if (IsRuleUnderMax(game, chosenInfo, ruleId))
            {
                ruleIds.Add(ruleId);
            }
        }
        
        RuleCard newCard = new RuleCard(ruleIds.ToArray(), RuleName, RuleDescription);
        newCard.PieceId = chosenInfo.pieceId;
        
        return newCard;
    }

    protected override CardBase CreateBlankCard(GameState game)
    {
        return new RuleCard(null);
    }

    public override bool CanMakeNewCard(GameState game)
    {
        // If 0 or less max occurrences, it doesn't limit occurrences.
        if (MaxOccurrences <= 0)
        {
            return true;
        }
        
        // If there is a max occurrence of 1 or more, then confirm there is at least one piece info that can have it
        foreach (var info in game.GetPieceInfoOnBoard())
        {
            if (PieceCanGetRule(game, info))
            {
                return true;
            }
        }

        return false;
    }
}