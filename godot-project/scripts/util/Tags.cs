using System.Collections;
using System.Collections.Generic;


// GodotObject wrapper for a string HashSet, so that it can be used in GDScript if needed.
public class Tags : IEnumerable<string> {
  public int Count => _tags.Count;

  private readonly HashSet<string> _tags = new();

  public IEnumerator<string> GetEnumerator() {
    return _tags.GetEnumerator();
  }

  public bool Add(string tag) {
    return _tags.Add(tag);
  }

  public bool Remove(string tag) {
    return _tags.Remove(tag);
  }

  public bool Contains(string tag) {
    return _tags.Contains(tag);
  }

  public void Clear() {
    _tags.Clear();
  }

  public Tags Clone() {
    var newTags = new Tags();
    foreach (string tag in newTags) {
      newTags.Add(tag);
    }

    return newTags;
  }

  public override string ToString() {
    string text = "{ ";

    int toGet = _tags.Count;
    foreach (string tag in _tags) {
      text += $"{tag}, ";
    }

    text += " }";
    return text;
  }


  IEnumerator IEnumerable.GetEnumerator() {
    return GetEnumerator();
  }
}