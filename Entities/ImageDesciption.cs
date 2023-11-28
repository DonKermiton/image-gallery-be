using image_gallery.Services;

namespace image_gallery.Models;

public record PostRecord(
    String id,
    String collection,
    String title,
    String description,
    DateTime? created
);

public record PostRecordWithImages(
        string id,
        string collection,
        string title,
        string description,
        DateTime? created,
        List<ContainerFile> images
        )
    : PostRecord(id,
        collection,
        title,
        description,
        created);