using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public struct PieceRule
{
    public bool isEnabled = true;
    internal int count = 1;
    public RuleBase rule;

    public PieceRule(RuleBase rule)
    {
        this.rule = rule;
    }
}