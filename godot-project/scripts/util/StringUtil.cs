using System;

public static class StringUtil
{
    public static string ToTitleCase(string text)
    {
        string returnText = "";

        bool newWord = true;
        foreach (var character in text)
        {
            if (newWord)
            {
                returnText += Char.ToUpper(character);
                newWord = false;
            }
            else
            {
                returnText += Char.ToLower(character);
            }

            if (character == ' ' || character == '_')
            {
                newWord = true;
            }
        }
        
        return returnText;
    }
}