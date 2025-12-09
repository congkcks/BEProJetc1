namespace TOEICWEB.Extensions
{
    public static class StringExtensions
    {
        public static string Truncate(this string? value, int maxLength)
        {
            if (string.IsNullOrEmpty(value)) return string.Empty;
            return value.Length <= maxLength ? value : value.Substring(0, maxLength);
        }
    }
}