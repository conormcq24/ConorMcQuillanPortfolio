using Markdig;
using System.Text.RegularExpressions;

namespace ConorMcQuillanPortfolio.Models
{
    public class JournalItem
    {
        public string journalTitle { get; set; }
        public string journalBody { get; set; }
        public string journalAppType { get; set; }
        public string[] journalTech { get; set; }
        public DateOnly journalDate { get; set; }

        // Add this property to render Markdown as HTML
        public string MarkdownHtml
        {
            get
            {
                var pipeline = new MarkdownPipelineBuilder()
                    .UseAdvancedExtensions()  // Enables tables, task lists, etc.
                    .UseEmphasisExtras()      // For additional emphasis styles
                    .UseGridTables()          // For grid tables
                    .UsePipeTables()          // For pipe tables
                    .UseListExtras()          // For better list rendering
                    .UseTaskLists()           // For GitHub-style task lists
                    .UseAutoLinks()           // For automatic link detection
                    .UseEmojiAndSmiley()      // For emoji support
                    .UseSmartyPants()         // For smart punctuation
                    .UseBootstrap()           // For Bootstrap styling
                    .Build();

                return Markdown.ToHtml(journalBody ?? string.Empty, pipeline);
            }
        }

        public JournalItem(string title, string body, string appType, string tech, string date)
        {
            journalTitle = title;

            // Replace all occurrences of "Images/Notes" with "ObsidianImages" in the body
            journalBody = ReplaceImagePaths(body);

            journalAppType = appType;
            journalTech = ParseTechnologies(tech);
            journalDate = ParseDate(date);
        }

        private string ReplaceImagePaths(string markdownContent)
        {
            if (string.IsNullOrEmpty(markdownContent))
                return markdownContent;

            // Replace direct references to the path with absolute path from web root
            string result = markdownContent.Replace("Images/Notes", "/ObsidianImages");

            // Handle image references with Markdown syntax, ensuring paths start with /
            result = Regex.Replace(
                result,
                @"!\[(.*?)\]\((Images/Notes/.*?)\)",
                m => $"![{m.Groups[1].Value}]({m.Groups[2].Value.Replace("Images/Notes", "/ObsidianImages")})"
            );

            // Also handle any existing ObsidianImages references without leading slash
            result = Regex.Replace(
                result,
                @"!\[(.*?)\]\((ObsidianImages/.*?)\)",
                m => $"![{m.Groups[1].Value}](/{m.Groups[2].Value})"
            );

            return result;
        }

        public string[] ParseTechnologies(string techString)
        {
            // First trim the outer quotes ('" at the start and "' at the end)
            string innerContent = techString;
            if (techString.Length >= 4 && techString.StartsWith("'\"") && techString.EndsWith("\"'"))
            {
                innerContent = techString.Substring(2, techString.Length - 4);
            }

            // Now trim any remaining quotes and split by commas
            string trimmedString = innerContent.Trim('"');
            string[] tempJournalTech =
                trimmedString.Split(',')
                .Select(t => t.Trim())
                .ToArray();
            return tempJournalTech;
        }

        public DateOnly ParseDate(string dateString)
        {
            DateOnly.TryParse(dateString, out DateOnly result);
            DateOnly tempJournalDate = result;
            return tempJournalDate;
        }
    }
}