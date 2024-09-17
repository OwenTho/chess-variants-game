using Godot;
using Godot.Collections;
using System;
using System.Collections.Generic;

public partial class KnightMoveRule : ActionRuleBase
{
    public int LevelsBeforeRepeat = 3;
    public override void AddPossibleActions(GameState game, Piece piece, int level)
    {
        List<Vector2I> jumpPositions = GetJumpPositions(level, LevelsBeforeRepeat, new []{ GridVectors.Up, GridVectors.Left });
        GetJumpPositions(level, LevelsBeforeRepeat, new []{ GridVectors.Left, GridVectors.Up }, jumpPositions);
        GetJumpPositions(level, LevelsBeforeRepeat, new []{ GridVectors.Up, GridVectors.Right }, jumpPositions);
        GetJumpPositions(level, LevelsBeforeRepeat, new []{ GridVectors.Right, GridVectors.Up }, jumpPositions);
        GetJumpPositions(level, LevelsBeforeRepeat, new []{ GridVectors.Down, GridVectors.Left }, jumpPositions);
        GetJumpPositions(level, LevelsBeforeRepeat, new []{ GridVectors.Left, GridVectors.Down }, jumpPositions);
        GetJumpPositions(level, LevelsBeforeRepeat, new []{ GridVectors.Down, GridVectors.Right }, jumpPositions);
        GetJumpPositions(level, LevelsBeforeRepeat, new []{ GridVectors.Right, GridVectors.Down }, jumpPositions);

        AddJumps(piece, jumpPositions);
    }

    public List<Vector2I> GetJumpPositions(int level, int beforeRepeat, Vector2I[] dirOrder, List<Vector2I> jumpPositions = null)
    {
        if (jumpPositions == null)
        {
            jumpPositions = new List<Vector2I>();
        }

        if (dirOrder.Length == 0)
        {
            GD.PushError($"{GetType().Name} was provided a dirOrder of Length 0 for GetJumpPositions, when it needs at least 1.");
            return jumpPositions;
        }

        // If level is 0, it can't move.
        if (level == 0)
        {
            return jumpPositions;
        }
        
        if (level > beforeRepeat)
        {
            GetJumpPositions(level - beforeRepeat, beforeRepeat, dirOrder, jumpPositions);
        }
        
        Vector2I jumpLoc = GridVectors.Zero;
        int curInd = 0;
        for (int i = 0; i < level; i++)
        {
            jumpLoc += dirOrder[curInd];
            curInd++;
            if (curInd >= dirOrder.Length)
            {
                curInd = 0;
            }
        }

        jumpPositions.Add(jumpLoc);
        
        return jumpPositions;
    }

    public void AddJumps(Piece piece, List<Vector2I> jumpLocations)
    {
        List<Vector2I> jumpsDone = new List<Vector2I>();

        Vector2I thisPosition = piece.cell.pos;
        
        foreach (var jumpOffset in jumpLocations)
        {
            if (jumpsDone.Contains(jumpOffset))
            {
                continue;
            }

            Attack(piece, thisPosition + jumpOffset, AttackType.IfMove);
            jumpsDone.Add(jumpOffset);
        }
    }
}
