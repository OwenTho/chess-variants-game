
using System.Collections.Generic;

public partial class RandomPieceRuleCardFactory : CardFactory
{
    public string[] RuleIds;
    public string RuleName;
    public string RuleDescription;

    public RandomPieceRuleCardFactory(string[] ruleIds, string ruleName, string ruleDescription = "No description provided.")
    {
        RuleIds = ruleIds;
        RuleName = ruleName;
        RuleDescription = ruleDescription;
    }
    
    public RandomPieceRuleCardFactory(string ruleId, string ruleName, string ruleDescription = "No description provided.") : this(new []{ ruleId }, ruleName, ruleDescription)
    {

    }

    protected override CardBase CreateCard(GameState game)
    {
        RuleCard newCard = new RuleCard(RuleIds, RuleName, RuleDescription);
        
        List<string> pieceIds = game.GetPieceIdsOnBoard();
        
        int randomInd = game.gameRandom.RandiRange(0, pieceIds.Count - 1);
        newCard.PieceId = pieceIds[randomInd];
        
        return newCard;
    }

    protected override CardBase CreateBlankCard(GameState game)
    {
        return new RuleCard(null);
    }

    public override bool CanMakeNewCard(GameState game)
    {
        return true;
    }
}