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
            try
            {
                using var openFileDialog = new OpenFileDialog();
                openFileDialog.Filter = "All Files (*.*)|*.*";
                openFileDialog.Title = "�������� ���� ��� ������������";

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    string filePath = openFileDialog.FileName;
                    int blockSize = 1024; // 1��

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

                    listSharedFiles.Items.Add(sharingFile.FileName);
                    CreateDownloadButton(sharingFile.FileName);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"��������� ������: {ex.Message}", "������", MessageBoxButtons.OK, MessageBoxIcon.Error);
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

        private async Task OnDownloadClicked(string fileName)
        {
            var fileMetaData = _sharedFiles.Find(f => f.FileName == fileName);
            if (fileMetaData == null)
            {
                MessageBox.Show("���� �� ������.", "������", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            using var folderDialog = new FolderBrowserDialog();
            folderDialog.Description = "�������� ����� ��� ���������� JSON-�����";
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

                string jsonFilePath = Path.Combine(selectedFolder, $"{fileName}.json");
                await File.WriteAllTextAsync(jsonFilePath, json);

                MessageBox.Show($"����� ������� ��������: {jsonFilePath}", "�����", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private async void OnImportClicked(object sender, EventArgs e)
        {
            try
            {
                using var openFileDialog = new OpenFileDialog();
                openFileDialog.Filter = "JSON Files (*.json)|*.json"; // ������ ��� ������ JSON-������
                openFileDialog.Title = "�������� JSON-���� ��� �������";

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    string jsonFilePath = openFileDialog.FileName;
                    string json = await File.ReadAllTextAsync(jsonFilePath);

                    var downloadingFile = JsonSerializer.Deserialize<FileMetaData>(json);

                    if (downloadingFile == null)
                    {
                        MessageBox.Show("�� ������� ��������� JSON-����.", "������", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    downloadingFile.Blocks = new byte[downloadingFile.TotalBlocks][];
                    downloadingFile.FilePath = Path.Combine(
                        Path.GetDirectoryName(jsonFilePath),
                        downloadingFile.FileName
                    );

                    await _client.AddFile(downloadingFile.RootHash, downloadingFile);
                    _downloadingFiles.Add(downloadingFile);
                    listDownloadingFiles.Items.Add(downloadingFile.FileName);

                    MessageBox.Show("����� ������� ������������.", "�����", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"������ ��� ������� �����: {ex.Message}", "������", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private async void OnStartSharingClicked(object sender, EventArgs e)
        {
            if (_sharedFiles.Count == 0)
            {
                MessageBox.Show("���� ��� ������� ��� �� ��������.", "������", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

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

            MessageBox.Show("���������� ��������.", "Torrent", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void OnStopDownloadClicked(object sender, EventArgs e)
        {
            MessageBox.Show("���������� �����������.", "Torrent", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}