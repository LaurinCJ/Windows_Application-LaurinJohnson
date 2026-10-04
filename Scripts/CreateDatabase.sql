IF DB_ID('GameLibraryDB') IS NULL
BEGIN
    CREATE DATABASE GameLibraryDB;
END
GO

USE GameLibraryDB;
GO

IF OBJECT_ID('dbo.Games', 'U') IS NOT NULL DROP TABLE dbo.Games;
IF OBJECT_ID('dbo.Platforms', 'U') IS NOT NULL DROP TABLE dbo.Platforms;
IF OBJECT_ID('dbo.Franchises', 'U') IS NOT NULL DROP TABLE dbo.Franchises;
GO

CREATE TABLE Franchises
(
    FranchiseId INT IDENTITY(1,1) PRIMARY KEY,
    FranchiseName NVARCHAR(100) NOT NULL,
    Publisher NVARCHAR(100) NOT NULL
);

CREATE TABLE Platforms
(
    PlatformId INT IDENTITY(1,1) PRIMARY KEY,
    PlatformName NVARCHAR(100) NOT NULL,
    Manufacturer NVARCHAR(100) NOT NULL
);

CREATE TABLE Games
(
    GameId INT IDENTITY(1,1) PRIMARY KEY,
    GameTitle NVARCHAR(150) NOT NULL,
    ReleaseYear INT NOT NULL,
    FranchiseId INT NOT NULL,
    PlatformId INT NOT NULL,
    CONSTRAINT FK_Games_Franchises FOREIGN KEY (FranchiseId) REFERENCES Franchises(FranchiseId),
    CONSTRAINT FK_Games_Platforms FOREIGN KEY (PlatformId) REFERENCES Platforms(PlatformId)
);
GO
