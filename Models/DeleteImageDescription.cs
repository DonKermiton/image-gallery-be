namespace image_gallery.Models;

public class DeleteImageRequestBody
{
    public string id { get; set; }
    public string? collection { get; set; }
}