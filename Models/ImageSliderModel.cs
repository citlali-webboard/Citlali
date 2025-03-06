namespace Citlali.Models;

public class ImageSliderModel
{
    public ImageSliderItem[]? Images { get; set; }
}

public class ImageSliderItem
{
    public string? imageUrl { get; set; }
    public string? caption { get; set; } = "";
}