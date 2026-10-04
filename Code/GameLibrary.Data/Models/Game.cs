namespace GameLibrary.Data.Models;

public class Game
{
    public int GameId { get; set; }
    public string GameTitle { get; set; } = string.Empty;
    public int ReleaseYear { get; set; }
    public int FranchiseId { get; set; }
    public int PlatformId { get; set; }
    public string FranchiseName { get; set; } = string.Empty;
    public string PlatformName { get; set; } = string.Empty;
}
