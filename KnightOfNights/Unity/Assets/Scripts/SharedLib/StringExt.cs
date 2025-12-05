namespace KnightOfNights.Scripts.SharedLib
{
    public static class StringExt
    {
        public static bool ConsumePrefix(this string str, string prefix, out string trimmed)
        {
            if (str.StartsWith(prefix))
            {
                trimmed = str.Substring(prefix.Length);
                return true;
            }
            else
            {
                trimmed = "";
                return false;
            }
        }
    }
}
