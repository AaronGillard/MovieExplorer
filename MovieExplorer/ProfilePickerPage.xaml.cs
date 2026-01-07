using MovieExplorer.Models;
using MovieExplorer.Services;
using System.Linq;

namespace MovieExplorer;

public partial class ProfilePickerPage : ContentPage
{
    private readonly ProfileService _profiles = new();
    private List<Profile> _data = new();
    public ProfilePickerPage()
    {
		InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await ReloadAsync();
    }
    // Reloads profiles from storage and updates the CollectionView
    private async Task ReloadAsync()
    {
        // Ensure there is atleast one profile and a current profile is set
        await _profiles.EnsureCurrentProfileAsync();

        _data = await _profiles.LoadProfilesAsync();
        ProfilesView.ItemsSource = _data;
    }

    // Handles profile selection
    private async void AddProfile_Clicked(object sender, EventArgs e)
    {
        var name = await DisplayPromptAsync("New Profile", "Enter profile name:");
        if (string.IsNullOrWhiteSpace(name))
            return;

        name = name.Trim();

        _data.Add(new Profile { Name = name });
        await _profiles.SaveProfilesAsync(_data);

        await ReloadAsync();
    }

    private async void ProfilesView_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is not Profile p)
            return;

        _profiles.SetCurrentProfileId(p.Id);

        // Prevent double-firing
        if (sender is CollectionView cv)
            cv.SelectedItem = null;

        // Navigate to your existing app page
        var pickerPage = this;
        await Navigation.PushAsync(new MainPage());

        // Remove picker so Back doesn't return to it
        Navigation.RemovePage(pickerPage);
    }
}