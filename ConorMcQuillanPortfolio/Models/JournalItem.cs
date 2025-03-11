namespace ConorMcQuillanPortfolio.Models
{
    public class JournalItem
    {
        public string journalTitle { get; set; }
        public string journalBody { get; set; }
        public string journalAppType { get; set; }
        public string[] journalTech { get; set; }
        public DateOnly journalDate { get; set; }
        public JournalItem(string title, string body, string appType, string[] tech, DateOnly date)
        {
            journalTitle = title;
            journalBody = body;
            journalAppType = appType;
            journalTech = tech;
            journalDate = date;
        }

        public void ParseTechnologies(string techString)
        {

            string trimmedString = techString.Trim('"');

            journalTech = trimmedString.Split(',')
                                      .Select(t => t.Trim())
                                      .ToArray();
        }

        public void ParseDate(string dateString)
        {
            DateOnly.TryParse(dateString, out DateOnly result);
            journalDate = result;
        }
    }
}
