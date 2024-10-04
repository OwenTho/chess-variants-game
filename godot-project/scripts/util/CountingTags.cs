using System.Collections.Generic;

public class CountingTags : Tags {
  private readonly Dictionary<string, int> _tagCounts = new();

  public bool Add(string tag, int additions = 1) {
    if (AddCount(tag, additions) <= 0) {
      base.Remove(tag);
      return false;
    }

    return base.Add(tag);
  }

  public bool Remove(string tag, int removals = 1) {
    // If there is none left, ignore remove
    if (AddCount(tag, -removals) > 0) {
      base.Add(tag);
      return false;
    }

    return base.Remove(tag);
  }

  public new void Clear() {
    base.Clear();
    _tagCounts.Clear();
  }

  public bool TryGetTagCount(string tag, out int tagCount) {
    if (_tagCounts.TryGetValue(tag, out tagCount)) {
      return true;
    }

    return false;
  }

  public new CountingTags Clone() {
    // Create the new tag, and copy over all the tag counts.
    var newTags = new CountingTags();

    foreach (KeyValuePair<string, int> tagCountPair in _tagCounts) {
      newTags.Add(tagCountPair.Key, tagCountPair.Value);
    }

    return newTags;
  }


  private int AddCount(string tag, int addition) {
    int num = addition;
    if (_tagCounts.TryGetValue(tag, out int count)) {
      num = count + addition;
      _tagCounts.Remove(tag);
    }

    _tagCounts.Add(tag, num);
    return num;
  }
}