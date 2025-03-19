namespace ConorMcQuillanPortfolio.Models
{
    public class JournalItem
    {
        public string journalTitle { get; set; }
        public string journalBody { get; set; }
        public string journalAppType { get; set; }
        public string[] journalTech { get; set; }
        public DateOnly journalDate { get; set; }
        public JournalItem(string title, string body, string appType, string tech, string date)
        {
            journalTitle = title;
            journalBody = body;
            journalAppType = appType;
            journalTech = ParseTechnologies(tech);
            journalDate = ParseDate(date);
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
