using MerkleTree;
using System.Text.Json;
using System.Windows.Forms;
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
            _client.Start(); //����������� ����� ��� ��
        }

        private void OnSelectFileClicked(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    string filePath = openFileDialog.FileName;

                    if (_sharedFiles.Count > 0)
                    {
                        MessageBox.Show("���� ��� ��������.", "������", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

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
                    listSharedFiles.Items.Add(fileMetaData.FileName);
                    CreateDownloadButton(fileMetaData.FileName);
                }
            }
        }


        private void CreateDownloadButton(string fileName)
        {
            Button btnDownload = new Button();
            btnDownload.Text = "�������";
            btnDownload.Tag = fileName;
            btnDownload.Size = new Size(200, 30);
            btnDownload.BackColor = Color.FromArgb(0, 122, 255);
            btnDownload.Click += (sender, e) => OnDownloadClicked(fileName);

            panelDownloadButtons.Controls.Add(btnDownload);
        }

        private async void OnDownloadClicked(string fileName)
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
                    await File.WriteAllTextAsync(jsonFilePath, json);

                    MessageBox.Show($"���� {fileMetaData.FileName} ������� �������� � {folderPath}", "�����", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        private async void OnImportClicked(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "JSON files (*.json)|*.json";
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    string jsonFilePath = openFileDialog.FileName;
                    string jsonContent = await File.ReadAllTextAsync(jsonFilePath);

                    var fileMetaData = JsonSerializer.Deserialize<FileMetaData>(jsonContent);
                    if (fileMetaData == null) return;

                    if (_downloadingFiles.Count > 0)
                    {
                        MessageBox.Show("����� ��� ��������.", "������", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    string directoryPath = Path.GetDirectoryName(jsonFilePath);
                    fileMetaData.FilePath = Path.Combine(directoryPath, fileMetaData.FileName);
                    fileMetaData.FileStatus = FileStatus.Downloading;

                    _downloadingFiles.Add(fileMetaData);
                    listDownloadingFiles.Items.Add(fileMetaData.FileName);
                }
            }
        }

        private async void OnStartSharingClicked(object sender, EventArgs e)
        {
            if (_sharedFiles.Count == 0)
            {
                MessageBox.Show("���� ��� ������� ��� �� ��������.", "������", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var sharingFile = _sharedFiles[0]; // ������������ �����, ������ ���� ��� �����

            await _client.AddFile(sharingFile.RootHash, sharingFile);

            MessageBox.Show("������� ��������.", "Torrent", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void OnStopSharingClicked(object sender, EventArgs e)
        {
            MessageBox.Show("������� �����������.", "Torrent", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private async void OnStartDownloadClicked(object sender, EventArgs e)
        {
            if (_downloadingFiles.Count == 0)
            {
                MessageBox.Show("����� ��� ������� ��� �� ��������", "������", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var dowloadingFile = _downloadingFiles[0];  // ������������ �����, ������ ���� ��� �����

            await _client.AddFile(dowloadingFile.RootHash, dowloadingFile);

            MessageBox.Show("���������� ��������.", "Torrent", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void OnStopDownloadClicked(object sender, EventArgs e)
        {
            MessageBox.Show("���������� �����������.", "Torrent", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}