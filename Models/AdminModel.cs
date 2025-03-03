namespace Citlali.Models
{
    public class AdminCategoriesViewModel
    {
        public List<Tag> Tags { get; set; } = [];
        public Tag CreateDto { get; set; } = new();
        public Tag DeleteDto { get; set; } = new();
    }
    public class CategoryCreateDto {
        public Tag CreateDto { get; set; } = new();
    }
}