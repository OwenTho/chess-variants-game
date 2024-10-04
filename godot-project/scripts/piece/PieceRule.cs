public class PieceRule {
  // If true, this rule will always use the level above specifically.
  public bool EnforceLevel;

  public bool IsEnabled = true;

  // Level for this specific rule
  public int Level = 1;
  public ActionRuleBase Rule;

  public PieceRule(ActionRuleBase rule, int level = -1, bool enforceLevel = false) {
    Rule = rule;

    if (level < 0) {
      if (rule != null) {
        Level = rule.DefaultLevel;
      }
    }
    else {
      Level = level;
    }

    EnforceLevel = enforceLevel;
  }
}