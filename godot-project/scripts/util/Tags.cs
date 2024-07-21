using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


// GodotObject wrapper for a string HashSet, so that it can be used in GDScript if needed.
public partial class Tags
{
    private HashSet<string> tags = new HashSet<string>();
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
}
