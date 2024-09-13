
// TODO: Add levels to the PieceRules with initial levels, rather than individual pieces
// This will fix pieces starting at level 7 (such as Queen, Bishop and Rook)s
public struct PieceRule
{
    public bool isEnabled = true;
    // Level for this specific rule
    public int level = 1;
    public ActionRuleBase rule;

    public PieceRule(ActionRuleBase rule, int level = -1)
    {
        this.rule = rule;
        if (level < 0)
        {
            if (rule != null)
            {
                this.level = rule.defaultLevel;
            }
        }
        else
        {
            this.level = level;
        }
    }
}