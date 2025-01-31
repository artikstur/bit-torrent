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

        private async void OnSelectFileClicked(object sender, EventArgs e)
        {
            try { } catch { } finally
            {
                string filePath = @"C:\Users\artur\OneDrive\Desktop\life.png";
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
                };

                await _client.AddFile(sharingFile.RootHash, sharingFile);
                _sharedFiles.Add(sharingFile);
                listSharedFiles.Items.Add(sharingFile.FileName);
                CreateDownloadButton(sharingFile.FileName);
            }
        }


        private void CreateDownloadButton(string fileName)
        {
            Button btnDownload = new Button();
            btnDownload.Text = "Скачать";
            btnDownload.Tag = fileName;
            btnDownload.Size = new Size(200, 30);
            btnDownload.BackColor = Color.FromArgb(0, 122, 255);
            btnDownload.Click += (sender, e) => OnDownloadClicked(fileName);

            panelDownloadButtons.Controls.Add(btnDownload);
        }

        private async Task OnDownloadClicked(string fileName)
        {
            var fileMetaData = _sharedFiles.Find(f => f.FileName == fileName);
            if (fileMetaData == null) return;

            using FolderBrowserDialog folderDialog = new FolderBrowserDialog();
            if (folderDialog.ShowDialog() == DialogResult.OK)
            {
                string folderPath = folderDialog.SelectedPath;
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

                MessageBox.Show($"Файл {fileMetaData.FileName} успешно сохранен в {folderPath}", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private async void OnImportClicked(object sender, EventArgs e)
        {
            try { }
            catch { }
            finally 
            {
                string filePath = @"C:\Users\artur\OneDrive\Desktop\life.png";
                string torrentPath = @"C:\Users\artur\OneDrive\Desktop\test-torrent\life.png";
                int blockSize = 1024; // 1кб
                var blocks = FileWorker.SplitFileIntoBlocks(filePath, blockSize);
                var merkleTree = new ByteMerkleTree(blocks);
                var auditPath = merkleTree.GetAuditPath(2);
                var isValid = merkleTree.VerifyBlock(blocks[1], 1, auditPath);

                var downloadingFile = new FileMetaData
                {
                    FileStatus = FileStatus.Downloading,
                    FilePath = torrentPath,
                    BlockSize = blockSize,
                    TotalBlocks = blocks.Length,
                    RootHash = BitConverter.ToString(merkleTree.Root.Hash),
                    Blocks = new byte[blocks.Length][],
                };

                await _client.AddFile(downloadingFile.RootHash, downloadingFile);
                _downloadingFiles.Add(downloadingFile);
                listDownloadingFiles.Items.Add(downloadingFile.FileName);
            }
        }

        private async void OnStartSharingClicked(object sender, EventArgs e)
        {
            if (_sharedFiles.Count == 0)
            {
                MessageBox.Show("Файл для раздачи еще не добавлен.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var sharingFile = _sharedFiles[0]; // захаркдкодил прост, первый файл для теста

            await _client.AddFile(sharingFile.RootHash, sharingFile);

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

            var dowloadingFile = _downloadingFiles[0];  // захаркдкодил прост, первый файл для теста

            await _client.AddFile(dowloadingFile.RootHash, dowloadingFile);

            MessageBox.Show("Скачивание запущено.", "Torrent", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void OnStopDownloadClicked(object sender, EventArgs e)
        {
            MessageBox.Show("Скачивание остановлено.", "Torrent", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}