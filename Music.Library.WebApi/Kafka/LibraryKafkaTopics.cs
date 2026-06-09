using Utility.Kafka;
namespace MusicLibrary.Kafka;

public class LibraryKafkaTopics : AbstractKafkaTopics
{
    public string SongAdded { get; set; } = "song-added-to-library";
    
    public override IEnumerable<string> GetTopics()
    {
        yield return SongAdded;
    }
}