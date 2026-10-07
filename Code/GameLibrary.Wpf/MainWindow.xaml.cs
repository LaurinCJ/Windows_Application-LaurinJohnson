using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using GameLibrary.Data.DataAccess;
using GameLibrary.Data.Models;

namespace GameLibrary.Wpf;

public partial class MainWindow : Window
{
    private readonly GameLibraryDataManager _dataManager = new();

    public MainWindow()
    {
        InitializeComponent();
    }

    private void LoadGames_Click(object sender, RoutedEventArgs e)
    {
        RunDatabaseAction(() =>
        {
            GamesDataGrid.ItemsSource = _dataManager.GetAllGames();
            LoadGameLookups();
            SetStatus("Games loaded.");
        });
    }

    private void SearchGames_Click(object sender, RoutedEventArgs e)
    {
        RunDatabaseAction(() =>
        {
            string searchText = GameSearchTextBox.Text.Trim();
            bool searchByTitle = GameSearchTypeComboBox.SelectedIndex == 0;
            GamesDataGrid.ItemsSource = searchByTitle
                ? _dataManager.SearchGamesByTitle(searchText)
                : _dataManager.SearchGamesByFranchise(searchText);
            SetStatus(searchText.Length == 0 ? "All games loaded." : "Search complete.");
        });
    }

    private void NewGame_Click(object sender, RoutedEventArgs e)
    {
        RunDatabaseAction(() =>
        {
            LoadGameLookups();
            ClearGameEditor();
            SetStatus("Enter the details for a new game.");
        });
    }

    private void GamesDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (GamesDataGrid.SelectedItem is not Game game)
        {
            return;
        }

        RunDatabaseAction(() =>
        {
            LoadGameLookups();
            GameTitleTextBox.Text = game.GameTitle;
            GameReleaseYearTextBox.Text = game.ReleaseYear.ToString();
            GameFranchiseComboBox.SelectedValue = game.FranchiseId;
            GamePlatformComboBox.SelectedValue = game.PlatformId;
            SetStatus($"Editing {game.GameTitle}.");
        });
    }

    private void SaveGame_Click(object sender, RoutedEventArgs e)
    {
        if (!TryGetGameFromEditor(out Game game))
        {
            return;
        }

        RunDatabaseAction(() =>
        {
            if (GamesDataGrid.SelectedItem is Game selectedGame)
            {
                game.GameId = selectedGame.GameId;
                _dataManager.UpdateGame(game);
                SetStatus("Game updated.");
            }
            else
            {
                _dataManager.AddGame(game);
                SetStatus("Game added.");
            }

            GamesDataGrid.ItemsSource = _dataManager.GetAllGames();
            ClearGameEditor();
        });
    }

    private void DeleteGame_Click(object sender, RoutedEventArgs e)
    {
        if (GamesDataGrid.SelectedItem is not Game game)
        {
            ShowMessage("Select a game to delete.");
            return;
        }

        if (!ConfirmDelete($"Delete '{game.GameTitle}'?"))
        {
            return;
        }

        RunDatabaseAction(() =>
        {
            _dataManager.DeleteGame(game.GameId);
            GamesDataGrid.ItemsSource = _dataManager.GetAllGames();
            ClearGameEditor();
            SetStatus("Game deleted.");
        });
    }

    private void LoadFranchises_Click(object sender, RoutedEventArgs e) => LoadFranchises();

    private void NewFranchise_Click(object sender, RoutedEventArgs e)
    {
        FranchisesDataGrid.SelectedItem = null;
        FranchiseNameTextBox.Clear();
        FranchisePublisherTextBox.Clear();
        SetStatus("Enter the details for a new franchise.");
    }

    private void FranchisesDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (FranchisesDataGrid.SelectedItem is Franchise franchise)
        {
            FranchiseNameTextBox.Text = franchise.FranchiseName;
            FranchisePublisherTextBox.Text = franchise.Publisher;
            SetStatus($"Editing {franchise.FranchiseName}.");
        }
    }

    private void SaveFranchise_Click(object sender, RoutedEventArgs e)
    {
        if (!TryGetFranchiseFromEditor(out Franchise franchise))
        {
            return;
        }

        RunDatabaseAction(() =>
        {
            if (FranchisesDataGrid.SelectedItem is Franchise selectedFranchise)
            {
                franchise.FranchiseId = selectedFranchise.FranchiseId;
                _dataManager.UpdateFranchise(franchise);
                SetStatus("Franchise updated.");
            }
            else
            {
                _dataManager.AddFranchise(franchise);
                SetStatus("Franchise added.");
            }

            LoadFranchisesCore();
            NewFranchise_Click(sender, e);
        });
    }

    private void DeleteFranchise_Click(object sender, RoutedEventArgs e)
    {
        if (FranchisesDataGrid.SelectedItem is not Franchise franchise)
        {
            ShowMessage("Select a franchise to delete.");
            return;
        }

        if (!ConfirmDelete($"Delete '{franchise.FranchiseName}'?"))
        {
            return;
        }

        RunDatabaseAction(() =>
        {
            _dataManager.DeleteFranchise(franchise.FranchiseId);
            LoadFranchisesCore();
            NewFranchise_Click(sender, e);
            SetStatus("Franchise deleted.");
        });
    }

    private void LoadPlatforms_Click(object sender, RoutedEventArgs e) => LoadPlatforms();

    private void LoadAdminPlatforms_Click(object sender, RoutedEventArgs e) => LoadPlatforms();

    private void NewPlatform_Click(object sender, RoutedEventArgs e)
    {
        PlatformsDataGrid.SelectedItem = null;
        PlatformNameTextBox.Clear();
        PlatformManufacturerTextBox.Clear();
        SetStatus("Enter the details for a new platform.");
    }

    private void PlatformsDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (PlatformsDataGrid.SelectedItem is Platform platform)
        {
            PlatformNameTextBox.Text = platform.PlatformName;
            PlatformManufacturerTextBox.Text = platform.Manufacturer;
            SetStatus($"Editing {platform.PlatformName}.");
        }
    }

    private void SavePlatform_Click(object sender, RoutedEventArgs e)
    {
        if (!TryGetPlatformFromEditor(PlatformNameTextBox, PlatformManufacturerTextBox, out Platform platform))
        {
            return;
        }

        RunDatabaseAction(() =>
        {
            if (PlatformsDataGrid.SelectedItem is Platform selectedPlatform)
            {
                platform.PlatformId = selectedPlatform.PlatformId;
                _dataManager.UpdatePlatform(platform);
                SetStatus("Platform updated.");
            }
            else
            {
                _dataManager.AddPlatform(platform);
                SetStatus("Platform added.");
            }

            LoadPlatformsCore();
            NewPlatform_Click(sender, e);
        });
    }

    private void DeletePlatform_Click(object sender, RoutedEventArgs e)
    {
        if (PlatformsDataGrid.SelectedItem is not Platform platform)
        {
            ShowMessage("Select a platform to delete.");
            return;
        }

        DeletePlatform(platform, () =>
        {
            LoadPlatformsCore();
            NewPlatform_Click(sender, e);
        });
    }

    private void AdminPlatformsDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (AdminPlatformsDataGrid.SelectedItem is Platform platform)
        {
            AdminPlatformNameTextBox.Text = platform.PlatformName;
            AdminPlatformManufacturerTextBox.Text = platform.Manufacturer;
            SetStatus($"Editing {platform.PlatformName} in the admin area.");
        }
    }

    private void NewAdminPlatform_Click(object sender, RoutedEventArgs e)
    {
        AdminPlatformsDataGrid.SelectedItem = null;
        AdminPlatformNameTextBox.Clear();
        AdminPlatformManufacturerTextBox.Clear();
        SetStatus("Enter the details for a new platform.");
    }

    private void SaveAdminPlatform_Click(object sender, RoutedEventArgs e)
    {
        if (!TryGetPlatformFromEditor(AdminPlatformNameTextBox, AdminPlatformManufacturerTextBox, out Platform platform))
        {
            return;
        }

        RunDatabaseAction(() =>
        {
            if (AdminPlatformsDataGrid.SelectedItem is Platform selectedPlatform)
            {
                platform.PlatformId = selectedPlatform.PlatformId;
                _dataManager.UpdatePlatform(platform);
                SetStatus("Platform updated from Administration.");
            }
            else
            {
                _dataManager.AddPlatform(platform);
                SetStatus("Platform added from Administration.");
            }

            LoadPlatformsCore();
            NewAdminPlatform_Click(sender, e);
        });
    }

    private void DeleteAdminPlatform_Click(object sender, RoutedEventArgs e)
    {
        if (AdminPlatformsDataGrid.SelectedItem is not Platform platform)
        {
            ShowMessage("Select a platform to delete.");
            return;
        }

        DeletePlatform(platform, () =>
        {
            LoadPlatformsCore();
            NewAdminPlatform_Click(sender, e);
        });
    }

    private void SaveSettings_Click(object sender, RoutedEventArgs e)
    {
        bool darkTheme = ThemeComboBox.SelectedIndex == 1;
        Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(darkTheme ? "#202938" : "#F4F7FB"));
        SetStatus($"{(darkTheme ? "Dark" : "Light")} theme applied. Delete confirmations are {(ConfirmDeleteCheckBox.IsChecked == true ? "on" : "off")}.");
    }

    private void LoadFranchises()
    {
        RunDatabaseAction(() =>
        {
            LoadFranchisesCore();
            SetStatus("Franchises loaded.");
        });
    }

    private void LoadPlatforms()
    {
        RunDatabaseAction(() =>
        {
            LoadPlatformsCore();
            SetStatus("Platforms loaded.");
        });
    }

    private void LoadFranchisesCore() => FranchisesDataGrid.ItemsSource = _dataManager.GetAllFranchises();

    private void LoadPlatformsCore()
    {
        List<Platform> platforms = _dataManager.GetAllPlatforms();
        PlatformsDataGrid.ItemsSource = platforms;
        AdminPlatformsDataGrid.ItemsSource = platforms;
    }

    private void LoadGameLookups()
    {
        GameFranchiseComboBox.ItemsSource = _dataManager.GetAllFranchises();
        GamePlatformComboBox.ItemsSource = _dataManager.GetAllPlatforms();
    }

    private bool TryGetGameFromEditor(out Game game)
    {
        game = new Game();
        if (string.IsNullOrWhiteSpace(GameTitleTextBox.Text))
        {
            ShowMessage("Enter a game title.");
            return false;
        }

        if (!int.TryParse(GameReleaseYearTextBox.Text, out int releaseYear) || releaseYear < 1950 || releaseYear > 2100)
        {
            ShowMessage("Enter a valid release year between 1950 and 2100.");
            return false;
        }

        if (GameFranchiseComboBox.SelectedValue is not int franchiseId || GamePlatformComboBox.SelectedValue is not int platformId)
        {
            ShowMessage("Choose both a franchise and a platform.");
            return false;
        }

        game = new Game
        {
            GameTitle = GameTitleTextBox.Text.Trim(),
            ReleaseYear = releaseYear,
            FranchiseId = franchiseId,
            PlatformId = platformId
        };
        return true;
    }

    private bool TryGetFranchiseFromEditor(out Franchise franchise)
    {
        franchise = new Franchise();
        if (string.IsNullOrWhiteSpace(FranchiseNameTextBox.Text) || string.IsNullOrWhiteSpace(FranchisePublisherTextBox.Text))
        {
            ShowMessage("Enter both a franchise name and publisher.");
            return false;
        }

        franchise = new Franchise
        {
            FranchiseName = FranchiseNameTextBox.Text.Trim(),
            Publisher = FranchisePublisherTextBox.Text.Trim()
        };
        return true;
    }

    private bool TryGetPlatformFromEditor(TextBox nameTextBox, TextBox manufacturerTextBox, out Platform platform)
    {
        platform = new Platform();
        if (string.IsNullOrWhiteSpace(nameTextBox.Text) || string.IsNullOrWhiteSpace(manufacturerTextBox.Text))
        {
            ShowMessage("Enter both a platform name and manufacturer.");
            return false;
        }

        platform = new Platform
        {
            PlatformName = nameTextBox.Text.Trim(),
            Manufacturer = manufacturerTextBox.Text.Trim()
        };
        return true;
    }

    private void ClearGameEditor()
    {
        GamesDataGrid.SelectedItem = null;
        GameTitleTextBox.Clear();
        GameReleaseYearTextBox.Clear();
        GameFranchiseComboBox.SelectedIndex = -1;
        GamePlatformComboBox.SelectedIndex = -1;
    }

    private void DeletePlatform(Platform platform, Action refresh)
    {
        if (!ConfirmDelete($"Delete '{platform.PlatformName}'?"))
        {
            return;
        }

        RunDatabaseAction(() =>
        {
            _dataManager.DeletePlatform(platform.PlatformId);
            refresh();
            SetStatus("Platform deleted.");
        });
    }

    private bool ConfirmDelete(string question)
    {
        return ConfirmDeleteCheckBox.IsChecked != true ||
               MessageBox.Show(question, "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes;
    }

    private void RunDatabaseAction(Action action)
    {
        try
        {
            action();
        }
        catch (Exception exception)
        {
            MessageBox.Show(exception.Message, "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
            SetStatus("The database action could not be completed.");
        }
    }

    private void ShowMessage(string message) => MessageBox.Show(message, "Game Library Manager", MessageBoxButton.OK, MessageBoxImage.Information);

    private void SetStatus(string message) => StatusTextBlock.Text = message;
}
