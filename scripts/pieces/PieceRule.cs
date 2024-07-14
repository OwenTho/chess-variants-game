
public struct PieceRule
{
    public bool isEnabled = true;
    internal int count = 1;
    public RuleBase rule;

    public PieceRule(RuleBase rule)
    {
        this.rule = rule;
    }
}