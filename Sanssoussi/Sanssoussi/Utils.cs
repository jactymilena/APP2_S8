using Ganss.Xss;

namespace Sanssoussi
{
    public static class Utils
    {
        public static string SanitizeInput(string input)
        {
            var sanitizer = new HtmlSanitizer();
            return sanitizer.Sanitize(input);
        }
    }
}
