﻿using Godot;
using Godot.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public abstract partial class LineMoveRule : ActionRuleBase
{

    public override void AddPossibleActions(GameState game, Piece piece, int maxForward)
    {
        Vector2I thisPosition = new Vector2I(piece.cell.x, piece.cell.y);

        foreach (Vector2I dir in GetDirs(game, piece))
        {
            MoveAction prevMove = null;
            for (int i = 1; i <= maxForward; i++)
            {
                Vector2I actionPos = thisPosition + (dir * i);
                AttackAction newAttack = Attack(piece, actionPos, AttackType.IfMove, prevMove);
                newAttack.moveAction.tags.Add("line_move");

                prevMove = newAttack.moveAction;
                OnNewLineAction(game, piece, prevMove, newAttack);
            }
        }
    }

    public virtual void OnNewLineAction(GameState game, Piece piece, MoveAction moveAction, AttackAction attackAction)
    {
        
    }

    public abstract Vector2I[] GetDirs(GameState game, Piece piece);
}
