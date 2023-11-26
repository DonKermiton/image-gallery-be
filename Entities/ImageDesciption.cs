namespace image_gallery.Models;

public record ImageDescription(
    String id,
    List<string> imageIds,
    String collection,
    String title,
    String description,
    DateTime? created
    );

        
