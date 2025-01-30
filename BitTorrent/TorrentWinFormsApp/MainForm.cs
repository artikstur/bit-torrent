using MerkleTree;
using System.Text.Json;
using TorrentClient;

namespace TorrentWinFormsApp
{
    public partial class MainForm : Form
    {
        private Client _client = new Client();
        private List<FileMetaData> _sharedFiles = new List<FileMetaData>();
        private List<FileMetaData> _downloadingFiles = new List<FileMetaData>();

        public MainForm()
        {
            InitializeComponent();
            _client.Start();
        }

        private void OnSelectFileClicked(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    string filePath = openFileDialog.FileName;
                    int blockSize = 1024;
                    byte[][] blocks = FileWorker.SplitFileIntoBlocks(filePath, blockSize);
                    long fileSize = FileWorker.GetFileSize(filePath);
                    var merkleTree = new ByteMerkleTree(blocks);

                    var fileMetaData = new FileMetaData
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

                    _sharedFiles.Add(fileMetaData);
                    _client.AddFile(fileMetaData.RootHash, fileMetaData);
                    listSharedFiles.Items.Add(fileMetaData.FileName);

                    CreateDownloadButton(fileMetaData.FileName);
                }
            }
        }

        private void CreateDownloadButton(string fileName)
        {
            Button btnDownload = new Button();
            btnDownload.Text = $"Скачать {fileName}";
            btnDownload.Tag = fileName;
            btnDownload.Size = new Size(200, 30);
            btnDownload.BackColor = Color.FromArgb(0, 122, 255);
            btnDownload.Click += (sender, e) => OnDownloadClicked(fileName);

            panelDownloadButtons.Controls.Add(btnDownload);
        }

        private void OnDownloadClicked(string fileName)
        {
            var fileMetaData = _sharedFiles.Find(f => f.FileName == fileName);
            if (fileMetaData == null) return;

            using (FolderBrowserDialog folderDialog = new FolderBrowserDialog())
            {
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
                    File.WriteAllText(jsonFilePath, json);

                    MessageBox.Show($"Файл {fileMetaData.FileName} успешно сохранен в {folderPath}", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        private void OnImportClicked(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "JSON files (*.json)|*.json";
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    string jsonFilePath = openFileDialog.FileName;
                    string jsonContent = File.ReadAllText(jsonFilePath);

                    var fileMetaData = JsonSerializer.Deserialize<FileMetaData>(jsonContent);
                    if (fileMetaData == null) return;

                    string directoryPath = Path.GetDirectoryName(jsonFilePath);
                    fileMetaData.FilePath = Path.Combine(directoryPath, fileMetaData.FileName);
                    fileMetaData.FileStatus = FileStatus.Downloading;

                    _downloadingFiles.Add(fileMetaData);
                    _client.AddFile(fileMetaData.RootHash, fileMetaData);
                    listDownloadingFiles.Items.Add(fileMetaData.FileName);
                }
            }
        }

        private void OnStartSharingClicked(object sender, EventArgs e)
        {
            MessageBox.Show("Раздача запущена.", "Torrent", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void OnStopSharingClicked(object sender, EventArgs e)
        {
            MessageBox.Show("Раздача остановлена.", "Torrent", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void OnStartDownloadClicked(object sender, EventArgs e)
        {
            MessageBox.Show("Скачивание запущено.", "Torrent", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void OnStopDownloadClicked(object sender, EventArgs e)
        {
            MessageBox.Show("Скачивание остановлено.", "Torrent", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void lblDownloading_Click(object sender, EventArgs e)
        {
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
        }
    }
}