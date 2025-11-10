using System.Text;

namespace BlazeJump.Helpers
{
    /// <summary>
    /// Utility class for converting titles to URL-friendly slugs.
    /// </summary>
    public static class TitleToSlug
    {
        /// <summary>
        /// Converts a title string to a URL-friendly slug (lowercase, hyphenated).
        /// </summary>
        /// <param name="title">The title to convert.</param>
        /// <returns>A slug representation of the title.</returns>
        /// <exception cref="ArgumentNullException">Thrown when title is null or empty.</exception>
        public static string ConvertTitleToSlug(string title)
        {
            if (string.IsNullOrEmpty(title))
            {
                throw new ArgumentNullException("input");
            }
            var stringBuilder = new StringBuilder();
            foreach (char c in title.ToArray())
            {
                if (Char.IsLetterOrDigit(c))
                {
                    stringBuilder.Append(c);
                }
                else if (c == ' ')
                {
                    stringBuilder.Append("-");
                }
            }
            return stringBuilder.ToString().ToLower();
        }
    }
}
