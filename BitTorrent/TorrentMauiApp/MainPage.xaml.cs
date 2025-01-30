using System.Collections.ObjectModel;
using System.Text.Json;
using MerkleTree;
using TorrentClient;

namespace TorrentMauiApp;

public partial class MainPage : ContentPage
{
    private Client _client = new Client();
    private ObservableCollection<FileMetaData> _sharedFiles = new ObservableCollection<FileMetaData>();
    private ObservableCollection<FileMetaData> _dowloadingFiles = new ObservableCollection<FileMetaData>();

    public MainPage()
    {
        InitializeComponent();
        _client.Start();
        BindingContext = this;
        FilesSharingView.ItemsSource = _sharedFiles;
        FilesDownloadingView.ItemsSource = _dowloadingFiles;
    }

    private async void OnSelectFileClicked(object sender, EventArgs e)
    {
        var result = await FilePicker.PickAsync(new PickOptions
        {
            PickerTitle = "Выберите файл для раздачи"
        });

        if (result == null) return;

        string filePath = result.FullPath;
        int blockSize = 1024;

        byte[][] blocks = FileWorker.SplitFileIntoBlocks(filePath, blockSize);
        long fileSize = FileWorker.GetFileSize(filePath);
        var merkleTree = new ByteMerkleTree(blocks);

        var fileMetaDataSharing = new FileMetaData
        {
            RootHash = BitConverter.ToString(merkleTree.Root.Hash),
            FileStatus = FileStatus.Sharing,
            FileName = Path.GetFileName(filePath),
            FileSize = fileSize,
            BlockSize = blockSize,
            TotalBlocks = blocks.Length,
            FilePath = filePath,
            Blocks = blocks
        };

        _sharedFiles.Add(fileMetaDataSharing);
        _client.AddFile(fileMetaDataSharing.RootHash, fileMetaDataSharing);
    }

    private async void OnDownloadClicked(object sender, EventArgs e)
    {
        var button = sender as Button;
        var fileMetaData = button.BindingContext as FileMetaData;

        if (fileMetaData == null) return;

        var folderPath = @"C:\Users\egor\Desktop\test";

        if (string.IsNullOrEmpty(folderPath)) return;

        var downloadMetaData = new FileMetaData
        {
            RootHash = fileMetaData.RootHash,
            FileStatus = FileStatus.Downloading,
            FileName = fileMetaData.FileName,
            FileSize = fileMetaData.FileSize,
            BlockSize = fileMetaData.BlockSize,
            TotalBlocks = fileMetaData.TotalBlocks,
            Blocks = new byte[][] { }
        };

        string json = JsonSerializer.Serialize(downloadMetaData);
        string jsonFilePath = Path.Combine(folderPath, $"{fileMetaData.FileName}.json");

        await File.WriteAllTextAsync(jsonFilePath, json);

        await DisplayAlert("Успех", $"Файл {fileMetaData.FileName} успешно сохранен в {folderPath}", "OK");
    }

    private async void OnImportClicked(object sender, EventArgs e)
    {
        var result = await FilePicker.PickAsync(new PickOptions
        {
            PickerTitle = "Выберите JSON-файл для импорта"
        });

        if (result == null) return;

        string jsonFilePath = result.FullPath;
        string jsonContent = await File.ReadAllTextAsync(jsonFilePath);

        var fileMetaData = JsonSerializer.Deserialize<FileMetaData>(jsonContent);

        if (fileMetaData == null) return;

        string directoryPath = Path.GetDirectoryName(jsonFilePath)!;
        string newFilePath = Path.Combine(directoryPath, fileMetaData.FileName);
        fileMetaData.FilePath = newFilePath;

        fileMetaData.FileStatus = FileStatus.Downloading;

        _dowloadingFiles.Add(fileMetaData);
        _client.AddFile(fileMetaData.RootHash, fileMetaData);

        await DisplayAlert("Успех", $"Файл {fileMetaData.FileName} успешно импортирован", "OK");
    }

    private void OnStartSharingClicked(object sender, EventArgs e)
    {
        Console.WriteLine("Запущена раздача файлов.");
    }

    private void OnStopSharingClicked(object sender, EventArgs e)
    {
        Console.WriteLine("Раздача файлов остановлена.");
    }

    private void OnStartDownloadClicked(object sender, EventArgs e)
    {
        Console.WriteLine("Запущено скачивание файлов.");
    }

    private void OnStopDownloadClicked(object sender, EventArgs e)
    {
        Console.WriteLine("Скачивание файлов остановлено.");
    }
}