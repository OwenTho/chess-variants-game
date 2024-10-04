using Godot;

public abstract partial class ActionRuleBase : RuleBase {
  public enum AttackType {
    NoMove, // No move
    MoveIf, // Move if attack
    IfMove, // Attack if move
    AlsoMove // Move and / or attack
  }

  public abstract void AddPossibleActions(GameState game, Piece piece, int level);

  public virtual void NewTurn(GameState game, Piece piece) {
  }

  public virtual void EndTurn(GameState game, Piece piece) {
  }


  // Returns the newly created rules
  internal AttackAction Attack(Piece attacker, Vector2I attackLocation,
    AttackType attackType = AttackType.NoMove,
    ActionBase dependency = null) {
    var newAttack = new AttackAction(attacker, attackLocation, attackLocation);
    if (dependency != null && attackType != AttackType.IfMove) {
      newAttack.AddDependency(dependency);
    }

    attacker.AddAction(newAttack);

    // If Move:
    if (attackType != AttackType.NoMove) {
      var newMove = new MoveAction(attacker, attackLocation, attackLocation);
      newAttack.MoveAction = newMove;
      newMove.AttackAction = newAttack;
      attacker.AddAction(newMove);
      // If AndMove, add dependency for the attack
      if (attackType == AttackType.MoveIf) {
        newMove.AddDependency(newAttack);
      }

      if (attackType == AttackType.IfMove) {
        newAttack.AddDependency(newMove);
      }

      if (dependency != null && attackType != AttackType.MoveIf) {
        newMove.AddDependency(dependency);
      }
    }

    return newAttack;
  }
}