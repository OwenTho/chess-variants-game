using Godot;

public abstract partial class RuleBase : Node {
  public string RuleId { get; internal set; }
  public int DefaultLevel { get; internal set; }
}