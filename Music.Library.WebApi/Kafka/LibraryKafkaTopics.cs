namespace MusicLibrary.Kafka;

public class LibraryKafkaTopics : AbstractKafkaTopics
{
    public string SongAdded { get; set; } = "song-added-to-library";
    public string SongRemoved { get; set; } = "song-removed-from-library";
    
    public override IEnumerable<string> GetTopics()
    {
        yield return SongAdded;
        yield return SongRemoved;
    }
}