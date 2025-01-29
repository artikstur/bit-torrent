namespace TorrentMauiApp;

public partial class MainPage : ContentPage
{
    public MainPage()
    {
        InitializeComponent();
    }

    private async void OnSelectFileClicked(object sender, EventArgs e)
    {
        var result = await FilePicker.PickAsync(new PickOptions
        {
            PickerTitle = "Выберите JSON-файл",
            FileTypes = new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
            {
                { DevicePlatform.WinUI, new[] { ".json" } }
            })
        });

        if (result != null)
        {
            FilePathEntry.Text = result.FullPath; 
        }
    }

    private async void OnStartDownloadClicked(object sender, EventArgs e)
    {
        // Логика для начала загрузки
    }

    private async void OnStopDownloadClicked(object sender, EventArgs e)
    {
        // Логика для остановки загрузки
    }

    private async void OnStopDistributionClicked(object sender, EventArgs e)
    {
        // Логика для остановки раздачи
    }
}