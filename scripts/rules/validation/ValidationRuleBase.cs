using System.Collections.Generic;

public abstract partial class ValidationRuleBase : RuleBase
{
    public abstract void CheckAction(GameController game, Piece piece, ActionBase action, Tags invalidTags, Tags extraTags);
}
