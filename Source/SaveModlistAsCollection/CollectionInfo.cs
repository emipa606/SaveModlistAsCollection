using Steamworks;
using Verse;

namespace SaveModlistAsCollection;

[StaticConstructorOnStartup]
public class CollectionInfo
{
    public string CollectionName { get; set; }
    public string CollectionDescription { get; set; }
    public PublishedFileId_t CollectionId { get; set; }
    public string CollectionUrl { get; set; }
    public uint CollectionItems { get; set; }
}