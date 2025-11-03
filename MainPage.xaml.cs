
using Microsoft.Maui.Controls;
using CommunityToolkit.Maui.Storage;
using System;
using Microsoft.Maui.Controls;



namespace AudioPlayer;

public partial class MainPage : ContentPage
{
    private readonly IFolderPicker _folderPicker;
    public MainPage()
    {
        InitializeComponent();
        _folderPicker = FolderPicker.Default;
    }

    private async void OnOpenFolderClicked(object sender, EventArgs e)
    {
        var result = await _folderPicker.PickAsync(CancellationToken.None);

        // тут потом открыть папку типо
        await Navigation.PushAsync(new PlayerPage(), animated: false);
        ////Application.Current.MainPage = new PlayerPage();
        try
        {


            if (result.IsSuccessful && result.Folder != null)
            {
                var folder = result.Folder;
                folderPath.Text = $"Name: {folder.Name}\nPath: {folder.Path}";


                // Переход на страницу плеера после выбора папки
                await Navigation.PushAsync(new PlayerPage(), animated: false);


            }
            else
            {
                folderPath.Text = "Выбор папки отменён.";
            }
        }
        catch (Exception ex)
        {
            folderPath.Text = $"Ошибка: {ex.Message}";
        }
    }
}