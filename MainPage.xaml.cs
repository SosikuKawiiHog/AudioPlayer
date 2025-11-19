
using Microsoft.Maui.Controls;
using CommunityToolkit.Maui.Storage;
using System;
using Microsoft.Maui.Controls;
using AudioPlayer.Services;



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
        //var result = await _folderPicker.PickAsync(CancellationToken.None);

        //// тут потом открыть папку типо
        //await Navigation.PushAsync(new PlayerPage(), animated: false);
        //////Application.Current.MainPage = new PlayerPage();
        //try
        //{


        //    if (result.IsSuccessful && result.Folder != null)
        //    {
        //        var folder = result.Folder;
        //        folderPath.Text = $"Name: {folder.Name}\nPath: {folder.Path}";


        //        // Переход на страницу плеера после выбора папки
        //        await Navigation.PushAsync(new PlayerPage(), animated: false);


        //    }
        //    else
        //    {
        //        folderPath.Text = "Выбор папки отменён.";
        //    }
        //}
        //catch (Exception ex)
        //{
        //    folderPath.Text = $"Ошибка: {ex.Message}";
        //}
        try
        {
            var result = await _folderPicker.PickAsync(CancellationToken.None);

            if (result.IsSuccessful && result.Folder != null)
            {
                var allowedExtensions = new[] { ".mp3", ".wav", ".ogg", ".flac" };

                var files = Directory.GetFiles(result.Folder.Path).Where(f => allowedExtensions.Contains(Path.GetExtension(f).ToLowerInvariant())).ToArray();

                if (files.Length == 0)
                {
                    await DisplayAlert("Ошибка!", "В выбранной папке нет аудиофайлов.", "блин");
                    return;
                }

                AudioManager.Instance.LoadTracksFromPaths(files);

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