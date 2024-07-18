using Godot;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

public abstract partial class ActionBase : GodotObject
{
    public string id { get; internal set; }

    public Vector2I actionLocation;
    public bool valid { get; private set; } = true;

    // Rules that this rule depends on. If these are invalid, this one should
    // also be invalid.
    public List<ActionBase> dependencies { get; private set; } = new List<ActionBase>();

    // Rules that depend on this one.
    public List<ActionBase> dependents { get; private set; } = new List<ActionBase>();

    // Extra tags of the Action
    public Tags tags { get; private set; } = new Tags();

    // Tags that make this action invalid
    public Tags invalidTags { get; private set; } = new Tags();

    // Dictionary which holds the invalid tags, and the number of occurrences.
    // If it's positive, it's added. If it's negative, it's removed.
    internal Dictionary<string, int> invalidTagCounts = new Dictionary<string, int>();

    public ActionBase(Vector2I actionLocation)
    {
        this.actionLocation = actionLocation;
    }

    public abstract void ActOn(GameController game, Piece piece);

    public void MakeDependentsInvalid()
    {
        foreach (ActionBase dependent in dependents)
        {
            dependent.MakeInvalid();
        }
    }

    public void MakeInvalid()
    {
        // If already invalid, ignore
        if (!valid)
        {
            return;
        }
        // Make invalid, and pass to dependents
        valid = false;
        MakeDependentsInvalid();
    }

    // Find if this action has a certain dependency.
    // It checks all dependencies, to make sure.
    public bool HasDependency(ActionBase action)
    {
        if (dependencies.Contains(action))
        {
            return true;
        }
        // If not a dependency here, check dependencies
        foreach (ActionBase dependency in dependencies)
        {
            if (dependency.HasDependency(action))
            {
                return true;
            }
        }
        return false;
    }

    public void AddDependency(ActionBase action)
    {
        if (action == null || dependencies.Contains(action))
        {
            return;
        }
        // Make sure it's not a dependent
        if (HasDependent(action))
        {
            GD.PushError($"Tried to make {this} a dependent of {action}, when it is already a dependency.");
            return;
        }
        dependencies.Add(action);
        action.AddDependent(this);
    }

    // Find if this action has a certain dependent.
    // It checks all dependents, to make sure.
    public bool HasDependent(ActionBase action)
    {
        if (dependencies.Contains(action))
        {
            return true;
        }
        // If not a dependent here, check dependencies
        foreach (ActionBase dependent in dependents)
        {
            if (dependent.HasDependent(action))
            {
                return true;
            }
        }
        return false;
    }

    public void AddDependent(ActionBase action)
    {
        if (action == null || dependents.Contains(action))
        {
            return;
        }
        // Make sure it's not a dependency
        if (HasDependent(action))
        {
            GD.PushError($"Tried to make {this} a dependency of {action}, when it is already a dependent.");
            return;
        }
        dependents.Add(action);
        action.AddDependency(this);
    }

    public void Tag(string tag, bool carry)
    {
        // If this already has the tag, ignore to avoid
        // infinite loops.
        if (tags.Contains(tag))
        {
            return;
        }
        tags.Add(tag);
        // If carry, also tag dependents
        if (carry)
        {
            TagDependents(tag, carry);
        }
    }

    public void TagDependents(string tag, bool carry = true)
    {
        foreach(ActionBase dependent in dependents)
        {
            dependent.Tag(tag, carry);
        }
    }

    public void TagDependancies(string tag, bool carry = true)
    {
        foreach (ActionBase dependency in dependencies)
        {
            dependency.Tag(tag, carry);
        }
    }




    private void AddInvalidTag(string tag)
    {

        int num = 1;
        if (invalidTagCounts.TryGetValue(tag, out int count))
        {
            num = count + 1;
            invalidTagCounts.Remove(tag);
        }
        // If 1 or higher, then add the tag
        invalidTagCounts.Add(tag, num);
        if (num > 0)
        {
            invalidTags.Add(tag);
        }
        //GD.Print($"Tag {tag} upped to {num}");
    }

    private void RemoveInvalidTag(string tag)
    {
        int num = -1;
        if (invalidTagCounts.TryGetValue(tag, out int count))
        {
            num = count - 1;
            invalidTagCounts.Remove(tag);
        }
        invalidTagCounts.Add(tag, num);
        if (num <= 0)
        {
            invalidTags.Remove(tag);
        }
        //GD.Print($"Tag {tag} lowered to {num}");
    }

    public void InvalidTag(string tag, bool carry = true)
    {
        AddInvalidTag(tag);
        // If carry, also tag dependents
        if (carry)
        {
            InvalidTagDependents(tag, carry);
        }
    }

    public void InvalidTagDependents(string tag, bool carry = true)
    {
        foreach (ActionBase dependent in dependents)
        {
            dependent.InvalidTag(tag, carry);
        }
    }

    public void InvalidTagDependencies(string tag, bool carry = true)
    {
        foreach (ActionBase dependency in dependencies)
        {
            dependency.InvalidTag(tag, carry);
        }
    }

    public void RemoveInvalidTag(string tag, bool carry = true)
    {
        RemoveInvalidTag(tag);
        // If carry, also remove the tag from dependents
        if (carry)
        {
            RemoveInvalidTagDependents(tag, carry);
        }
    }

    public void RemoveInvalidTagDependents(string tag, bool carry = true)
    {
        foreach (ActionBase dependent in dependents)
        {
            dependent.RemoveInvalidTag(tag, carry);
        }
    }

    public void RemoveInvalidTagDependencies(string tag, bool carry = true)
    {
        foreach (ActionBase dependency in dependencies)
        {
            dependency.RemoveInvalidTag(tag, carry);
        }
    }
}
