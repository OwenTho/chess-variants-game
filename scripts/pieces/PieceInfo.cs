using Godot;
using System.Collections.Generic;

public partial class PieceInfo : GodotObject
{
    internal List<PieceRule> rules;

    public string pieceId { get; internal set; }
    public string textureLoc { get; internal set; }

    public int level { get; internal set; } = 1;

    internal PieceInfo(string id, int initialLevel = 1, List<PieceRule> initialRules = null)
    {
        if (initialRules == null)
        {
            initialRules = new List<PieceRule>();
        }
        rules = initialRules;
        this.pieceId = id;
        level = initialLevel;
    }


    public bool HasRule(RuleBase rule)
    {
        foreach (PieceRule pieceRule in rules)
        {
            if (pieceRule.rule == rule)
            {
                return true;
            }
        }
        return false;
    }

    public bool TryGetPieceRule(RuleBase rule, out PieceRule pieceRule)
    {
        foreach (PieceRule thisRule in rules)
        {
            if (thisRule.rule == rule)
            {
                pieceRule = thisRule;
                return true;
            }
        }
        pieceRule = new PieceRule(null);
        return false;
    }

    public PieceInfo AddRule(RuleBase rule)
    {
        if (rule == null)
        {
            GD.PushWarning($"Tried to give PieceInfo {pieceId} a null rule.");
            return this;
        }

        // If it already has the rule, add to count and enable if it's not.
        if (TryGetPieceRule(rule, out PieceRule pieceRule))
        {
            pieceRule.count += 1;
            pieceRule.isEnabled = true;
            return this;
        }

        // If here, then add the rule
        rules.Add(new PieceRule(rule));
        return this;
    }
}
