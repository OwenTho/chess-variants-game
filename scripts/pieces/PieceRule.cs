
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