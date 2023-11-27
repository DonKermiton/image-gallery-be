using image_gallery.Services;

namespace image_gallery.Models;

public record ImageDescription(
    String id,
    String collection,
    String title,
    String description,
    DateTime? created
);

public record ImageDescriptionWithImages(string id, string collection, string title, string description,
        DateTime? created, List<ContainerFile> images)
    : ImageDescription(id, collection, title, description, created);