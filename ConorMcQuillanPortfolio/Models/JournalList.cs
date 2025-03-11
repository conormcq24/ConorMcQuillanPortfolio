using System.Collections.Generic;

namespace ConorMcQuillanPortfolio.Models
{
    public class JournalList
    {
        public List<JournalItem> journalList;
        public JournalList()
        {
            journalList = new List<JournalItem>();
        }

        public void AddJournal(JournalItem journal)
        {
            journalList.Add(journal);
        }
        public void Clear()
        {
            journalList.Clear();
        }
        public List<string> GetUniqueTechnologies()
        {
            return journalList
                .SelectMany(journal => journal.journalTech)
                .Distinct()
                .OrderBy(tech => tech)
                .ToList();
        }
        public List<string> GetUniqueAppTypes()
        {
            return journalList
                .Select(journal => journal.journalAppType)
                .Distinct()
                .OrderBy(appType => appType)
                .ToList();
        }
    }
}