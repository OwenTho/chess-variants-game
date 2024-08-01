
using System.Collections.Generic;

public class CountingTags : Tags
{
    internal Dictionary<string, int> tagCounts = new();

    private int AddCount(string tag, int addition)
    {
        int num = addition;
        if (tagCounts.TryGetValue(tag, out int count))
        {
            num = count + addition;
            tagCounts.Remove(tag);
        }
        tagCounts.Add(tag, num);
        return num;
    }
    
    public new bool Add(string tag, int additions = 1)
    {
        if (AddCount(tag, additions) <= 0)
        {
            base.Remove(tag);
            return false;
        }
        return base.Add(tag);
    }

    public new bool Remove(string tag, int removals = 1)
    {
        // If there is none left, ignore remove
        if (AddCount(tag, -removals) > 0)
        {
            base.Add(tag);
            return false;
        }
        return base.Remove(tag);
    }

    public new void Clear()
    {
        base.Clear();
        tagCounts.Clear();
    }

    public bool TryGetTagCount(string tag, out int tagCount)
    {
        if (tagCounts.TryGetValue(tag, out tagCount))
        {
            return true;
        }
        return false;
    }

    public new CountingTags Clone()
    {
        // Create the new tag, and copy over all the tag counts.
        CountingTags newTags = new CountingTags();
        
        foreach (var tagCountPair in tagCounts)
        {
            newTags.Add(tagCountPair.Key, tagCountPair.Value);
        }
        
        return newTags;
    }
}