using Godot;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

public abstract partial class ActionBase : GridItem
{
    public Piece owner { get; private set; }
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

    public ActionBase(Piece owner, Vector2I actionLocation)
    {
        this.actionLocation = actionLocation;
        this.owner = owner;
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
        if (action == this)
        {
            GD.PushError("Tried to add an Action as a dependency of itself.");
            return;
        }
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
        if (action == this)
        {
            GD.PushError("Tried to add an Action as a dependent of itself.");
            return;
        }
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

    public enum CarryType
    {
        NONE,
        DOWN,
        UP
    }

    public void Tag(string tag, CarryType carryType = CarryType.DOWN)
    {
        // If this already has the tag, ignore to avoid
        // infinite loops.
        if (tags.Contains(tag))
        {
            return;
        }
        tags.Add(tag);
        // If carry, also tag dependents
        if (carryType == CarryType.DOWN)
        {
            TagDependents(tag);
        }
        if (carryType == CarryType.UP)
        {
            TagDependancies(tag);
        }
    }

    public void TagDependents(string tag)
    {
        foreach (ActionBase dependent in dependents)
        {
            dependent.Tag(tag, CarryType.DOWN);
        }
    }

    public void TagDependancies(string tag)
    {
        foreach (ActionBase dependency in dependencies)
        {
            dependency.Tag(tag, CarryType.UP);
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

    public void InvalidTag(string tag, CarryType carryType = CarryType.DOWN)
    {
        AddInvalidTag(tag);
        // If carry, also tag dependents
        if (carryType == CarryType.DOWN)
        {
            InvalidTagDependents(tag);
        }
        if (carryType == CarryType.UP)
        {
            InvalidTagDependencies(tag);
        }
    }

    public void InvalidTagDependents(string tag)
    {
        foreach (ActionBase dependent in dependents)
        {
            dependent.InvalidTag(tag, CarryType.DOWN);
        }
    }

    public void InvalidTagDependencies(string tag)
    {
        foreach (ActionBase dependency in dependencies)
        {
            dependency.InvalidTag(tag, CarryType.UP);
        }
    }

    public void RemoveInvalidTag(string tag, CarryType carryType = CarryType.DOWN)
    {
        RemoveInvalidTag(tag);
        // If carry, also remove the tag from dependents
        if (carryType == CarryType.DOWN)
        {
            RemoveInvalidTagDependents(tag);
        }
        if (carryType == CarryType.UP)
        {
            RemoveInvalidTagDependencies(tag);
        }
    }

    public void RemoveInvalidTagDependents(string tag)
    {
        foreach (ActionBase dependent in dependents)
        {
            dependent.RemoveInvalidTag(tag, CarryType.DOWN);
        }
    }

    public void RemoveInvalidTagDependencies(string tag)
    {
        foreach (ActionBase dependency in dependencies)
        {
            dependency.RemoveInvalidTag(tag, CarryType.UP);
        }
    }
}
