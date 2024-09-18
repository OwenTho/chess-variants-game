
public class PieceRule
{
    public bool isEnabled = true;
    // Level for this specific rule
    public int level = 1;
    // If true, this rule will always use the level above specifically.
    public bool enforceLevel = false;
    public ActionRuleBase rule;

    public PieceRule(ActionRuleBase rule, int level = -1, bool enforceLevel = false)
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

        this.enforceLevel = enforceLevel;
    }
}