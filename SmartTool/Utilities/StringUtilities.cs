namespace SmartTool.Utilities
{
    using System;
    using System.Text.RegularExpressions;

    public static class StringUtilities
    {
        public static string RemoveBetween(this string sourceString, string startTag, string endTag, bool includeTags = false)
        {
            Regex regex = new Regex($"{Regex.Escape(startTag)}(.*?){Regex.Escape(endTag)}");
            return regex.Replace(sourceString, includeTags == false ? startTag + endTag : "");
        }

        public static string GetBetween(this string sourceString, string firstString, string lastString)
        {
            var pos1 = sourceString.IndexOf(firstString, StringComparison.Ordinal) + firstString.Length;
            var pos2 = sourceString.IndexOf(lastString, StringComparison.Ordinal);
            var finalString = sourceString.Substring(pos1, pos2 - pos1);
            return finalString;
        }

        public static string RemoveMultipeSpaces(this string text)
        {
            return Regex.Replace(text, @"\s+", " ");
        }

        public static string RemoveTabsSpacing(this string text)
        {
            return Regex.Replace(text, "\t", "");
        }
    }
}
