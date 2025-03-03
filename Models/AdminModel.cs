namespace Citlali.Models
{
    public class AdminCategoryCreateDto
    {
        public Tag Tag = new();
    }
    public class AdminCategoryDeleteDto
    {
        public Tag Tag = new();
    }
    public class AdminCategoriesViewModel
    {
        public List<Tag> Tags = [];
        public AdminCategoryCreateDto CreateDto = new();
        public AdminCategoryDeleteDto DeleteDto = new();
    }
}
