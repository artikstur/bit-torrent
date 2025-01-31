using MerkleTree;
using System.Text.Json;
using System.Windows.Forms;
using TorrentClient;

namespace TorrentWinFormsApp
{
    public partial class MainForm : Form
    {
        private Client _client;
        private List<FileMetaData> _sharedFiles = new List<FileMetaData>();
        private List<FileMetaData> _downloadingFiles = new List<FileMetaData>();

        private Button _btnDownload;

        public MainForm()
        {
            _client = new Client();
            _ = StartClient();
            InitializeComponent();
        }

        public async Task StartClient()
        {
            var task = Task.Run(async () =>
            {
                await _client.Start();
            });
        }

        private async void OnSelectSharingFileClicked(object sender, EventArgs e)
        {
            try
            {
                using var openFileDialog = new OpenFileDialog();
                openFileDialog.Filter = "All Files (*.*)|*.*";
                openFileDialog.Title = "Выберите файл для раздачи";

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    string filePath = openFileDialog.FileName;
                    int blockSize = 1024; // 1кб

                    var blocks = FileWorker.SplitFileIntoBlocks(filePath, blockSize);

                    var merkleTree = new ByteMerkleTree(blocks);
                    var auditPath = merkleTree.GetAuditPath(2);
                    var isValid = merkleTree.VerifyBlock(blocks[1], 1, auditPath);

                    var sharingFile = new FileMetaData
                    {
                        FileStatus = FileStatus.Sharing,
                        FilePath = filePath,
                        BlockSize = blockSize,
                        TotalBlocks = blocks.Length,
                        RootHash = BitConverter.ToString(merkleTree.Root.Hash),
                        Blocks = blocks,
                        FileSize = FileWorker.GetFileSize(filePath),
                        FileName = Path.GetFileName(filePath)
                    };

                    await _client.AddFile(sharingFile.RootHash, sharingFile);
                    _sharedFiles.Add(sharingFile);

                    AddRow(sharingFile.FileName, sharingFile.FileSize, sharingFile.FileStatus);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Произошла ошибка: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void OnSelectCreateImageFileClicked(object sender, EventArgs e)
        {
            try
            {
                using var openFileDialog = new OpenFileDialog();
                openFileDialog.Filter = "All Files (*.*)|*.*";
                openFileDialog.Title = "Выберите файл для расшаривания";

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    string filePath = openFileDialog.FileName;
                    int blockSize = 1024; // 1кб

                    var blocks = FileWorker.SplitFileIntoBlocks(filePath, blockSize);

                    var merkleTree = new ByteMerkleTree(blocks);
                    var auditPath = merkleTree.GetAuditPath(2);
                    var isValid = merkleTree.VerifyBlock(blocks[1], 1, auditPath);

                    var sharingFile = new FileMetaData
                    {
                        FileStatus = FileStatus.Sharing,
                        FilePath = filePath,
                        BlockSize = blockSize,
                        TotalBlocks = blocks.Length,
                        RootHash = BitConverter.ToString(merkleTree.Root.Hash),
                        Blocks = blocks,
                        FileSize = FileWorker.GetFileSize(filePath),
                        FileName = Path.GetFileName(filePath)
                    };

                    CreateDownloadButton(sharingFile);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Произошла ошибка: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        private void CreateDownloadButton(FileMetaData fileMetaData)
        {
            _btnDownload = new Button();
            _btnDownload.Text = "Скачать";
            _btnDownload.Tag = fileMetaData.FileName;
            _btnDownload.Size = new Size(200, 30);
            _btnDownload.BackColor = Color.FromArgb(0, 122, 255);
            _btnDownload.Click += (sender, e) => OnDownloadClicked(fileMetaData);

            panelDownloadButtons.Controls.Add(_btnDownload);
        }

        private async Task OnDownloadClicked(FileMetaData fileMetaData)
        {
            using var folderDialog = new FolderBrowserDialog();
            folderDialog.Description = "Выберите папку для сохранения JSON-файла";
            folderDialog.ShowNewFolderButton = true;

            if (folderDialog.ShowDialog() == DialogResult.OK)
            {
                string selectedFolder = folderDialog.SelectedPath;

                var downloadFileMetaData = new FileMetaData
                {
                    FileStatus = FileStatus.Downloading,
                    FilePath = null,
                    BlockSize = fileMetaData.BlockSize,
                    TotalBlocks = fileMetaData.TotalBlocks,
                    RootHash = fileMetaData.RootHash,
                    Blocks = null,
                    FileSize = fileMetaData.FileSize,
                    FileName = fileMetaData.FileName
                };

                string json = JsonSerializer.Serialize(downloadFileMetaData);

                string jsonFilePath = Path.Combine(selectedFolder, $"{fileMetaData.FileName}.json");
                await File.WriteAllTextAsync(jsonFilePath, json);

                MessageBox.Show($"Образ успешно сохранен: {jsonFilePath}", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                panelDownloadButtons.Controls.Remove(_btnDownload);
            }
        }

        private async void OnImportClicked(object sender, EventArgs e)
        {
            try
            {
                using var openFileDialog = new OpenFileDialog();
                openFileDialog.Filter = "JSON Files (*.json)|*.json"; // Фильтр для выбора JSON-файлов
                openFileDialog.Title = "Выберите JSON-файл для импорта";

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    string jsonFilePath = openFileDialog.FileName;
                    string json = await File.ReadAllTextAsync(jsonFilePath);

                    var downloadingFile = JsonSerializer.Deserialize<FileMetaData>(json);

                    if (downloadingFile == null)
                    {
                        MessageBox.Show("Не удалось прочитать JSON-файл.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    downloadingFile.Blocks = new byte[downloadingFile.TotalBlocks][];
                    downloadingFile.FilePath = Path.Combine(
                        Path.GetDirectoryName(jsonFilePath),
                        downloadingFile.FileName
                    );

                    await _client.AddFile(downloadingFile.RootHash, downloadingFile);
                    _downloadingFiles.Add(downloadingFile);

                    AddRow(downloadingFile.FileName, downloadingFile.FileSize, downloadingFile.FileStatus);

                    MessageBox.Show("Образ успешно импортирован.", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при импорте файла: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void AddRow(string fileName, long fileSize, FileStatus fileStatus)
        {

            var fileSizeString = FormatBytes(fileSize);
            var fileStatusString = fileStatus.ToString();
            dataGridView.Rows.Add(fileName, fileSizeString, fileStatusString);
        }

        private static string FormatBytes(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB", "TB" };
            int order = 0;
            while (bytes >= 1024 && order < sizes.Length - 1)
            {
                order++;
                bytes = bytes / 1024;
            }

            double size = Math.Round((double)bytes, 1);

            return $"{size} {sizes[order]}";
        }

        private async void OnStartSharingClicked(object sender, EventArgs e)
        {
            if (_sharedFiles.Count == 0)
            {
                MessageBox.Show("Файл для раздачи еще не добавлен.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            MessageBox.Show("Раздача запущена.", "Torrent", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void OnStopSharingClicked(object sender, EventArgs e)
        {
            MessageBox.Show("Раздача остановлена.", "Torrent", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private async void OnStartDownloadClicked(object sender, EventArgs e)
        {
            if (_downloadingFiles.Count == 0)
            {
                MessageBox.Show("Образ для раздачи еще не добавлен", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            MessageBox.Show("Скачивание запущено.", "Torrent", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void OnStopDownloadClicked(object sender, EventArgs e)
        {
            MessageBox.Show("Скачивание остановлено.", "Torrent", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void dataGridView_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }
    }
}