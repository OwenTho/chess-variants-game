﻿using System;
using Godot;
using System.Collections.Generic;

public abstract partial class ActionBase : GridItem<ActionBase>
{
    public int actionId = -1;
    public Piece owner { get; private set; }
    public Vector2I actionLocation;
    
    // If the action does something or not. Actions for checking validating (e.g: Sliding) set this to false.
    public bool acting { get; protected set; } = true;

    // If the action is valid and can be done
    public bool valid { get; private set; } = true;

    // Rules that this rule depends on. If these are invalid, this one should
    // also be invalid.
    public List<ActionBase> dependencies { get; private set; }

    // Rules that depend on this one.
    public List<ActionBase> dependents { get; private set; }

    // Extra tags of the Action. Add to this in the Move Rules.
    public Tags tags { get; } = new Tags();
    
    // Tags used in validation steps. Add this in Verification Rules.
    public CountingTags verifyTags { get; } = new CountingTags();

    // Tags that make this action invalid. Add to this in Verification Rules.
    public CountingTags invalidTags { get; } = new CountingTags();

    public ActionBase(Piece owner, Vector2I actionLocation)
    {
        this.actionLocation = actionLocation;
        this.owner = owner;

        dependencies = new List<ActionBase>();
        dependents = new List<ActionBase>();
    }

    public abstract void ActOn(GameState game, Piece piece);

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
            GD.PushError($"Tried to make {this} a dependent of {action}, when it is a dependency.");
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
        if (HasDependency(action))
        {
            GD.PushError($"Tried to make {this} a dependency of {action}, when it is a dependent.");
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
            TagDependencies(tag);
        }
    }

    public void TagDependents(string tag)
    {
        foreach (ActionBase dependent in dependents)
        {
            dependent.Tag(tag, CarryType.DOWN);
        }
    }

    public void TagDependencies(string tag)
    {
        foreach (ActionBase dependency in dependencies)
        {
            dependency.Tag(tag, CarryType.UP);
        }
    }
    
    public void VerifyTag(string tag, CarryType carryType = CarryType.DOWN, int tagCount = 1)
    {
        // If this already has the tag, ignore to avoid
        // infinite loops.
        if (verifyTags.Contains(tag))
        {
            return;
        }
        verifyTags.Add(tag);
        // If carry, also tag dependents
        if (carryType == CarryType.DOWN)
        {
            VerifyTagDependents(tag, tagCount);
        }
        if (carryType == CarryType.UP)
        {
            VerifyTagDependencies(tag, tagCount);
        }
    }

    public void VerifyTagDependents(string tag, int tagCount = 1)
    {
        foreach (ActionBase dependent in dependents)
        {
            dependent.VerifyTag(tag, CarryType.DOWN, tagCount);
        }
    }

    public void VerifyTagDependencies(string tag, int tagCount = 1)
    {
        foreach (ActionBase dependency in dependencies)
        {
            dependency.VerifyTag(tag, CarryType.UP, tagCount);
        }
    }

    public void RemoveVerifyTag(string tag, CarryType carryType = CarryType.DOWN, int tagCount = 1)
    {
        VerifyTag(tag, carryType, -tagCount);
    }

    public void RemoveVerifyTagDependents(string tag, int tagCount = 1)
    {
        VerifyTagDependents(tag, -tagCount);
    }

    public void RemoveVerifyTagDependencies(string tag, int tagCount = 1)
    {
        VerifyTagDependencies(tag, -tagCount);
    }






    public void InvalidTag(string tag, CarryType carryType = CarryType.DOWN, int tagCount = 1)
    {
        invalidTags.Add(tag, tagCount);
        // If carry, also tag dependents
        if (carryType == CarryType.DOWN)
        {
            InvalidTagDependents(tag, tagCount);
        }
        if (carryType == CarryType.UP)
        {
            InvalidTagDependencies(tag, tagCount);
        }
    }

    public void InvalidTagDependents(string tag, int tagCount = 1)
    {
        foreach (ActionBase dependent in dependents)
        {
            dependent.InvalidTag(tag, CarryType.DOWN, tagCount);
        }
    }

    public void InvalidTagDependencies(string tag, int tagCount = 1)
    {
        foreach (ActionBase dependency in dependencies)
        {
            dependency.InvalidTag(tag, CarryType.UP, tagCount);
        }
    }

    public void RemoveInvalidTag(string tag, CarryType carryType = CarryType.DOWN, int tagCount = 1)
    {
        InvalidTag(tag, carryType, -tagCount);
    }

    public void RemoveInvalidTagDependents(string tag, int tagCount = 1)
    {
        InvalidTagDependents(tag, -tagCount);
    }

    public void RemoveInvalidTagDependencies(string tag, int tagCount = 1)
    {
        InvalidTagDependents(tag, -tagCount);
    }

    internal void ResetValidity()
    {
        // Set to be valid again
        valid = true;
        // Clear tags
        verifyTags.Clear();
        invalidTags.Clear();
    }

    public void SetOwner(Piece newOwner, CarryType carryType = CarryType.DOWN)
    {
        
        // If owner is the same, ignore
        if (owner == newOwner)
        {
            return;
        }
        
        // If old owner is not null, remove this from the owner
        if (owner != null)
        {
            owner.RemoveAction(this);
        }
        
        owner = newOwner;

        if (carryType == CarryType.DOWN)
        {
            foreach (var dependent in dependents)
            {
                dependent.SetOwner(newOwner);
            }
        }

        if (carryType == CarryType.UP)
        {
            foreach (var dependency in dependencies)
            {
                dependency.SetOwner(newOwner, CarryType.UP);
            }
        }
        
        // Assume the owner already has the action
    }
    
    

    public abstract object Clone();
    
    protected void CloneTo(ActionBase action)
    {
        // First clear the action
        action.verifyTags.Clear();
        action.invalidTags.Clear();
        action.tags.Clear();
        
        // Then copy over the tags
        foreach (var tag in tags)
        {
            action.tags.Add(tag);
        }
        
        foreach (var tag in verifyTags)
        {
            // Get the tag counts, and add it that number of times
            if (verifyTags.TryGetTagCount(tag, out int tagCount))
            {
                action.verifyTags.Add(tag, tagCount);
            }
        }

        foreach (var tag in invalidTags)
        {
            // Get the tag counts, and add it that number of times
            if (invalidTags.TryGetTagCount(tag, out int tagCount))
            {
                action.invalidTags.Add(tag, tagCount);
            }
        }
        
        // Copy over the other variables
        // Don't copy owner, as it would have the copy owner too
        action.actionId = actionId;
        action.valid = valid;
        action.actionLocation = actionLocation;
    }

    public virtual Dictionary<string, int> GetExtraCopyLinks()
    {
        return null;
    }

    public virtual void SetExtraCopyLinks(GameState game, Dictionary<string, int> extraLinks, Dictionary<int, ActionBase> links)
    {
        
    }
}
