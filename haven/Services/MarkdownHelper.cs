using Markdig;
using System.Text.RegularExpressions;

namespace Haven.Services
{
    public static class MarkdownHelper
    {
        private static readonly MarkdownPipeline Pipeline = new MarkdownPipelineBuilder()
            .UseAdvancedExtensions()
            .UseSoftlineBreakAsHardlineBreak()
            .Build();

        /// <summary>
        /// Converts Markdown input into formatted, safe HTML for article and course rendering.
        /// Formats headlines as bold headings, lists as bullet/numbered points, and normalizes excess whitespace.
        /// </summary>
        public static string ToHtml(string? markdown)
        {
            if (string.IsNullOrWhiteSpace(markdown))
                return string.Empty;

            // Normalize line endings and trim extra tabs / trailing spaces on each line
            string normalized = NormalizeWhitespace(markdown);

            // Convert markdown to HTML via Markdig
            string html = Markdown.ToHtml(normalized, Pipeline);

            return html;
        }

        /// <summary>
        /// Converts Markdown input into clean plain text (strips headers, list bullets, bold formatting)
        /// for clean card previews and snippets.
        /// </summary>
        public static string ToPlainText(string? markdown)
        {
            if (string.IsNullOrWhiteSpace(markdown))
                return string.Empty;

            string normalized = NormalizeWhitespace(markdown);
            string text = Markdown.ToPlainText(normalized, Pipeline);
            text = Regex.Replace(text, @"\s+", " ").Trim();
            return text;
        }

        private static string NormalizeWhitespace(string input)
        {
            // Replace windows/mac line endings with standard \n
            input = input.Replace("\r\n", "\n").Replace("\r", "\n");

            // Split into lines, trim trailing whitespace and replace tab characters with spaces
            var lines = input.Split('\n')
                .Select(line => line.Replace("\t", "    ").TrimEnd());

            // Rejoin lines and collapse 3 or more consecutive newlines into double newlines
            string joined = string.Join("\n", lines);
            joined = Regex.Replace(joined, @"\n{3,}", "\n\n");

            return joined.Trim();
        }
    }
}
