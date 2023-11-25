namespace image_gallery.Models;

public record ImageDescription(
    String category,
    String title,
    String description,
    DateTime? created
    );

        
