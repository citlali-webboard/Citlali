namespace Citlali.Models
{
    public class AdminCategoriesViewModel
    {
        public List<Tag> Tags { get; set; } = [];
        public Tag CreateDto { get; set; } = new();
        public Tag DeleteDto { get; set; } = new();
    }

    public class AdminLocationsViewModel
    {
        public List<Location> Locations { get; set; } = [];
        public Location CreateDto { get; set; } = new();
        public Location DeleteDto { get; set; } = new();
    }
}