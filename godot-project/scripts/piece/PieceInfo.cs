using System.Collections.Generic;
using Godot;

[GlobalClass]
public partial class PieceInfo : Node {
  internal List<PieceRule> Rules;

  // Tags for the PieceInfo.
  public Tags Tags = new();
  internal List<ValidationRuleBase> ValidationRules;

  internal PieceInfo(string id, string displayName = null, int initialLevel = 1,
    List<PieceRule> initialRules = null,
    List<ValidationRuleBase> initialValidationRules = null) {
    if (displayName == null) {
      displayName = StringUtil.ToTitleCase(id);
    }

    DisplayName = displayName;
    if (initialRules == null) {
      initialRules = new List<PieceRule>();
    }

    if (initialValidationRules == null) {
      initialValidationRules = new List<ValidationRuleBase>();
    }

    Rules = initialRules;
    ValidationRules = initialValidationRules;
    PieceId = id;
    Level = initialLevel;
  }

  public string PieceId { get; internal set; }

  // The name that's displayed to users.
  public string DisplayName { get; internal set; }
  public string TextureLoc { get; internal set; }

  // Level for all pieces of this type
  public int Level { get; internal set; }


  public bool HasActionRule(ActionRuleBase rule) {
    if (rule == null) {
      return false;
    }

    foreach (PieceRule pieceRule in Rules) {
      if (pieceRule.Rule == rule) {
        return true;
      }
    }

    return false;
  }

  public bool HasValidationRule(ValidationRuleBase rule) {
    if (rule == null) {
      return false;
    }

    foreach (ValidationRuleBase validationRule in ValidationRules) {
      if (validationRule == rule) {
        return true;
      }
    }

    return false;
  }

  public bool TryGetPieceRule(RuleBase rule, out PieceRule pieceRule) {
    foreach (PieceRule thisRule in Rules) {
      if (thisRule.Rule == rule) {
        pieceRule = thisRule;
        return true;
      }
    }

    pieceRule = new PieceRule(null);
    return false;
  }

  public PieceInfo AddActionRule(ActionRuleBase rule, int customLevel = -1,
    bool enforceLevel = false) {
    if (rule == null) {
      GD.PushWarning($"Tried to give PieceInfo {PieceId} a null rule.");
      return this;
    }

    // If it already has the rule, add to count and enable if it's not.
    if (TryGetPieceRule(rule, out PieceRule pieceRule)) {
      pieceRule.Level += 1;
      pieceRule.IsEnabled = true;
      return this;
    }

    // If here, then add the rule
    Rules.Add(new PieceRule(rule, customLevel, enforceLevel));
    return this;
  }

  public PieceInfo AddValidationRule(ValidationRuleBase rule) {
    if (rule == null) {
      GD.PushWarning($"Tried to give PieceInfo {PieceId} a null rule.");
      return this;
    }

    // If it already has the rule, ignore
    if (HasValidationRule(rule)) {
      return this;
    }

    ValidationRules.Add(rule);
    return this;
  }
}