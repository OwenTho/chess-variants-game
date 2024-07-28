
// TODO: Add levels to the PieceRules with initial levels, rather than individual pieces
// This will fix pieces starting at level 7 (such as Queen, Bishop and Rook)s
public struct PieceRule
{
    public bool isEnabled = true;
    internal int count = 1;
    public ActionRuleBase rule;

    public PieceRule(ActionRuleBase rule)
    {
        this.rule = rule;
    }
}