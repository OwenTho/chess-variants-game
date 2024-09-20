
using System.Collections.Generic;

public partial class RandomPieceRuleCardFactory : CardFactory
{
    public string RuleId;
    public string RuleName;
    public string RuleDescription;
    public RandomPieceRuleCardFactory(string ruleId, string ruleName, string ruleDescription = "No description provided.")
    {
        RuleId = ruleId;
        RuleName = ruleName;
        RuleDescription = ruleDescription;
    }

    protected override CardBase CreateCard(GameState game)
    {
        RuleCard newCard = new RuleCard(RuleId, RuleName, RuleDescription);
        
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