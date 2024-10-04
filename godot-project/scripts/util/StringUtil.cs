public static class StringUtil {
  public static string ToTitleCase(string text) {
    string returnText = "";

    bool newWord = true;
    foreach (char character in text) {
      if (newWord) {
        returnText += char.ToUpper(character);
        newWord = false;
      }
      else {
        returnText += char.ToLower(character);
      }

      if (character == ' ' || character == '_') {
        newWord = true;
      }
    }

    return returnText;
  }
}