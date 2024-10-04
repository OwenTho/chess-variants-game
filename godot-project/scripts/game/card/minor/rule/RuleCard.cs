using System.Collections.Generic;
using Godot;

public partial class RuleCard : CardBase {
  public string PieceId;
  public string RuleDescription;
  public string[] RuleIds;
  public string RuleName;

  public RuleCard(string[] ruleIds, string ruleName = "No rule name provided.",
    string ruleDescription = "No rule description provided.") {
    RuleIds = ruleIds;
    RuleName = ruleName;
    RuleDescription = ruleDescription;
  }

  public override void OnAddCard(GameState game) {
    foreach (string ruleId in RuleIds) {
      // GD.Print($"Adding rule {ruleId} to {PieceId}.");
      game.AddActionRule(ruleId, PieceId);
      game.EnableActionUpdatesForPieceId(PieceId);
    }
  }

  public override string GetCardName(GameState game) {
    return RuleName;
  }

  public override string GetCardDescription(GameState game) {
    return $"{game.GetPieceName(PieceId)} can {RuleDescription}";
  }


  public override void FromDict(GameState game,
    Godot.Collections.Dictionary<string, string> dataDict) {
    if (!dataDict.TryGetValue("piece_id", out PieceId)) {
      GD.PushError("Data did not contain the value for the piece id.");
    }

    if (!dataDict.TryGetValue("rule_name", out RuleName)) {
      GD.PushError("Data did not contain the value for the rule name.");
    }

    if (!dataDict.TryGetValue("rule_description", out RuleDescription)) {
      GD.PushError("Data did not contain the value for the rule description.");
    }


    if (dataDict.TryGetValue("rule_id_count", out string ruleCountString)) {
      if (int.TryParse(ruleCountString, out int ruleCount)) {
        List<string> ruleIds = new();
        for (int i = 0; i < ruleCount; i++) {
          if (dataDict.TryGetValue($"rule_{i}_id", out string id)) {
            ruleIds.Add(id);
          }
          else {
            GD.PushError($"Data did not contain the value for the rule {i}.");
          }
        }

        RuleIds = ruleIds.ToArray();
      }
      else {
        GD.PushError("rule_id_count value was not an integer.");
      }
    }
    else {
      GD.PushError("Data did not contain the number of rule ids.");
    }
  }


  protected override CardBase CloneCard() {
    var newCard = new RuleCard(RuleIds, RuleName, RuleDescription);
    newCard.PieceId = PieceId;
    return newCard;
  }

  protected override Godot.Collections.Dictionary<string, string> ToDict(GameState game) {
    var cardData = new Godot.Collections.Dictionary<string, string> {
      { "piece_id", PieceId },
      { "rule_name", RuleName },
      { "rule_description", RuleDescription },
      { "rule_id_count", RuleIds.Length.ToString() }
    };

    for (int i = 0; i < RuleIds.Length; i++) {
      cardData.Add($"rule_{i}_id", RuleIds[i]);
    }

    return cardData;
  }
}