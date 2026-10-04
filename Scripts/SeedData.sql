USE GameLibraryDB;
GO

INSERT INTO Franchises (FranchiseName, Publisher) VALUES
('Super Mario', 'Nintendo'),
('Fallout', 'Bethesda'),
('The Legend of Zelda', 'Nintendo');

INSERT INTO Platforms (PlatformName, Manufacturer) VALUES
('Nintendo Switch', 'Nintendo'),
('PlayStation 5', 'Sony'),
('PC', 'Various');

INSERT INTO Games (GameTitle, ReleaseYear, FranchiseId, PlatformId) VALUES
('Super Mario Odyssey', 2017, 1, 1),
('Fallout 4', 2015, 2, 3),
('The Legend of Zelda Tears of the Kingdom', 2023, 3, 1);
GO
