using Godot;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


// GodotObject wrapper for a string HashSet, so that it can be used in GDScript if needed.
public partial class Tags : IEnumerable<string>
{
    private HashSet<string> tags = new();
    public int Count { get { return tags.Count; } }
    public bool Add(string tag)
    {
        return tags.Add(tag);
    }

    public bool Remove(string tag)
    {
        return tags.Remove(tag);
    }

    public bool Contains(string tag)
    {
        return tags.Contains(tag);
    }

    public void Clear()
    {
        tags.Clear();
    }

    public IEnumerator<string> GetEnumerator()
    {
        return tags.GetEnumerator();
    }

    public override string ToString()
    {
        string text = "{ ";

        int toGet = tags.Count;
        foreach (string tag in tags)
        {
            text += $"{tag}, ";
        }

        text += " }";
        return text;
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public Tags Clone()
    {
        Tags newTags = new Tags();
        foreach (var tag in newTags)
        {
            newTags.Add(tag);
        }

        return newTags;
    }
}
