using GameLibrary.Data.Models;
using Microsoft.Data.SqlClient;

namespace GameLibrary.Data.DataAccess;

public class GameLibraryDataManager
{
    private const string ConnectionString =
        "Server=(localdb)\\MSSQLLocalDB;Database=GameLibraryDB;Trusted_Connection=True;TrustServerCertificate=True;";

    public List<Franchise> GetAllFranchises()
    {
        const string sql = "SELECT FranchiseId, FranchiseName, Publisher FROM Franchises ORDER BY FranchiseName;";
        List<Franchise> franchises = new();

        using SqlConnection connection = new(ConnectionString);
        using SqlCommand command = new(sql, connection);
        connection.Open();
        using SqlDataReader reader = command.ExecuteReader();

        while (reader.Read())
        {
            franchises.Add(new Franchise
            {
                FranchiseId = reader.GetInt32(0),
                FranchiseName = reader.GetString(1),
                Publisher = reader.GetString(2)
            });
        }

        return franchises;
    }

    public void AddFranchise(Franchise franchise)
    {
        const string sql = "INSERT INTO Franchises (FranchiseName, Publisher) VALUES (@FranchiseName, @Publisher);";
        using SqlConnection connection = new(ConnectionString);
        using SqlCommand command = new(sql, connection);
        command.Parameters.AddWithValue("@FranchiseName", franchise.FranchiseName);
        command.Parameters.AddWithValue("@Publisher", franchise.Publisher);
        connection.Open();
        command.ExecuteNonQuery();
    }

    public void UpdateFranchise(Franchise franchise)
    {
        const string sql = "UPDATE Franchises SET FranchiseName = @FranchiseName, Publisher = @Publisher WHERE FranchiseId = @FranchiseId;";
        using SqlConnection connection = new(ConnectionString);
        using SqlCommand command = new(sql, connection);
        command.Parameters.AddWithValue("@FranchiseId", franchise.FranchiseId);
        command.Parameters.AddWithValue("@FranchiseName", franchise.FranchiseName);
        command.Parameters.AddWithValue("@Publisher", franchise.Publisher);
        connection.Open();
        command.ExecuteNonQuery();
    }

    public bool FranchiseHasGames(int franchiseId) => RecordExists("SELECT COUNT(*) FROM Games WHERE FranchiseId = @Id;", franchiseId);

    public void DeleteFranchise(int franchiseId)
    {
        if (FranchiseHasGames(franchiseId))
        {
            throw new InvalidOperationException("This franchise cannot be deleted because it is used by one or more games.");
        }

        ExecuteDelete("DELETE FROM Franchises WHERE FranchiseId = @Id;", franchiseId);
    }

    public List<Platform> GetAllPlatforms()
    {
        const string sql = "SELECT PlatformId, PlatformName, Manufacturer FROM Platforms ORDER BY PlatformName;";
        List<Platform> platforms = new();

        using SqlConnection connection = new(ConnectionString);
        using SqlCommand command = new(sql, connection);
        connection.Open();
        using SqlDataReader reader = command.ExecuteReader();

        while (reader.Read())
        {
            platforms.Add(new Platform
            {
                PlatformId = reader.GetInt32(0),
                PlatformName = reader.GetString(1),
                Manufacturer = reader.GetString(2)
            });
        }

        return platforms;
    }

    public void AddPlatform(Platform platform)
    {
        const string sql = "INSERT INTO Platforms (PlatformName, Manufacturer) VALUES (@PlatformName, @Manufacturer);";
        using SqlConnection connection = new(ConnectionString);
        using SqlCommand command = new(sql, connection);
        command.Parameters.AddWithValue("@PlatformName", platform.PlatformName);
        command.Parameters.AddWithValue("@Manufacturer", platform.Manufacturer);
        connection.Open();
        command.ExecuteNonQuery();
    }

    public void UpdatePlatform(Platform platform)
    {
        const string sql = "UPDATE Platforms SET PlatformName = @PlatformName, Manufacturer = @Manufacturer WHERE PlatformId = @PlatformId;";
        using SqlConnection connection = new(ConnectionString);
        using SqlCommand command = new(sql, connection);
        command.Parameters.AddWithValue("@PlatformId", platform.PlatformId);
        command.Parameters.AddWithValue("@PlatformName", platform.PlatformName);
        command.Parameters.AddWithValue("@Manufacturer", platform.Manufacturer);
        connection.Open();
        command.ExecuteNonQuery();
    }

    public bool PlatformHasGames(int platformId) => RecordExists("SELECT COUNT(*) FROM Games WHERE PlatformId = @Id;", platformId);

    public void DeletePlatform(int platformId)
    {
        if (PlatformHasGames(platformId))
        {
            throw new InvalidOperationException("This platform cannot be deleted because it is used by one or more games.");
        }

        ExecuteDelete("DELETE FROM Platforms WHERE PlatformId = @Id;", platformId);
    }

    public List<Game> GetAllGames() => GetGames("", "");

    public List<Game> SearchGamesByTitle(string title) => GetGames(title, "");

    public List<Game> SearchGamesByFranchise(string franchiseName) => GetGames("", franchiseName);

    public void AddGame(Game game)
    {
        const string sql = "INSERT INTO Games (GameTitle, ReleaseYear, FranchiseId, PlatformId) VALUES (@GameTitle, @ReleaseYear, @FranchiseId, @PlatformId);";
        using SqlConnection connection = new(ConnectionString);
        using SqlCommand command = CreateGameCommand(sql, connection, game);
        connection.Open();
        command.ExecuteNonQuery();
    }

    public void UpdateGame(Game game)
    {
        const string sql = "UPDATE Games SET GameTitle = @GameTitle, ReleaseYear = @ReleaseYear, FranchiseId = @FranchiseId, PlatformId = @PlatformId WHERE GameId = @GameId;";
        using SqlConnection connection = new(ConnectionString);
        using SqlCommand command = CreateGameCommand(sql, connection, game);
        command.Parameters.AddWithValue("@GameId", game.GameId);
        connection.Open();
        command.ExecuteNonQuery();
    }

    public void DeleteGame(int gameId) => ExecuteDelete("DELETE FROM Games WHERE GameId = @Id;", gameId);

    private List<Game> GetGames(string title, string franchiseName)
    {
        const string sql = @"SELECT g.GameId, g.GameTitle, g.ReleaseYear, g.FranchiseId, g.PlatformId,
                                    f.FranchiseName, p.PlatformName
                             FROM Games g
                             INNER JOIN Franchises f ON g.FranchiseId = f.FranchiseId
                             INNER JOIN Platforms p ON g.PlatformId = p.PlatformId
                             WHERE g.GameTitle LIKE @Title AND f.FranchiseName LIKE @FranchiseName
                             ORDER BY g.GameTitle;";
        List<Game> games = new();

        using SqlConnection connection = new(ConnectionString);
        using SqlCommand command = new(sql, connection);
        command.Parameters.AddWithValue("@Title", $"%{title}%");
        command.Parameters.AddWithValue("@FranchiseName", $"%{franchiseName}%");
        connection.Open();
        using SqlDataReader reader = command.ExecuteReader();

        while (reader.Read())
        {
            games.Add(new Game
            {
                GameId = reader.GetInt32(0),
                GameTitle = reader.GetString(1),
                ReleaseYear = reader.GetInt32(2),
                FranchiseId = reader.GetInt32(3),
                PlatformId = reader.GetInt32(4),
                FranchiseName = reader.GetString(5),
                PlatformName = reader.GetString(6)
            });
        }

        return games;
    }

    private static SqlCommand CreateGameCommand(string sql, SqlConnection connection, Game game)
    {
        SqlCommand command = new(sql, connection);
        command.Parameters.AddWithValue("@GameTitle", game.GameTitle);
        command.Parameters.AddWithValue("@ReleaseYear", game.ReleaseYear);
        command.Parameters.AddWithValue("@FranchiseId", game.FranchiseId);
        command.Parameters.AddWithValue("@PlatformId", game.PlatformId);
        return command;
    }

    private bool RecordExists(string sql, int id)
    {
        using SqlConnection connection = new(ConnectionString);
        using SqlCommand command = new(sql, connection);
        command.Parameters.AddWithValue("@Id", id);
        connection.Open();
        return (int)command.ExecuteScalar()! > 0;
    }

    private void ExecuteDelete(string sql, int id)
    {
        using SqlConnection connection = new(ConnectionString);
        using SqlCommand command = new(sql, connection);
        command.Parameters.AddWithValue("@Id", id);
        connection.Open();
        command.ExecuteNonQuery();
    }
}
