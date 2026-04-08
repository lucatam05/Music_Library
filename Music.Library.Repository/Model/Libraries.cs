namespace Music.User.Repository.Model;

public class Libraries
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public required string Nome { get; set; }
}