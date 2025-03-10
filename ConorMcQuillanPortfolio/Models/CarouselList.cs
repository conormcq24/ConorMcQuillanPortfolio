namespace ProfessionalPortfolio.Models
{
    public class CarouselList
    {
        public List<CarouselItem> Items { get; set; } = new List<CarouselItem>();
        public string CurrentView { get; set; }
        public string PageTitle { get; set; }
    }
}
