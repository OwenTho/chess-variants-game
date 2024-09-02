using Godot;
using Godot.Collections;

public abstract partial class RuleBase : Node
{
    public string ruleId { get; internal set; }
    public int defaultLevel { get; internal set; }
}
