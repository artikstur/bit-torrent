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
            _client.FileStatusChanged += OnFileStatusChanged;
            _client.DownloadStatusChanged += OnDownloadStatusChanged;
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

                    _sharedFiles.Add(sharingFile);
                    AddRow(sharingFile.FileName, sharingFile.FileSize, sharingFile.FileStatus, "Sharing");
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
            _btnDownload.Size = new Size(200, 36);
            _btnDownload.BackColor = Color.FromArgb(80, 0, 255);
            _btnDownload.ForeColor = Color.White;
            _btnDownload.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            _btnDownload.FlatStyle = FlatStyle.Flat;
            _btnDownload.FlatAppearance.BorderSize = 0;
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

                    _downloadingFiles.Add(downloadingFile);

                    AddRow(downloadingFile.FileName, downloadingFile.FileSize, downloadingFile.FileStatus, "Downloading");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при импорте файла: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void AddRow(string fileName, long fileSize, FileStatus fileStatus, string fileType)
        {
            var fileSizeString = FormatBytes(fileSize);
            var fileStatusString = "In waiting";

            int rowIndex = dataGridView.Rows.Add(fileName, fileSizeString, fileStatusString);

            dataGridView.Rows[rowIndex].Cells["FileTypeColumn"].Value = fileType;
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

        private async void OnStartSharingClicked(string fileName)
        {
            var sharingFile = _sharedFiles.Where(f => f.FileName == fileName).FirstOrDefault();

            if (sharingFile == null)
            {
                MessageBox.Show("Файл для раздачи еще не добавлен.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            OnFileStatusChanged(sharingFile);
            await _client.AddFile(sharingFile.RootHash, sharingFile);


            MessageBox.Show("Раздача запущена.", "Torrent", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private async void OnStopSharingClicked(string fileName)
        {
            var sharingFile = _sharedFiles.FirstOrDefault(f => f.FileName == fileName);

            if (sharingFile == null)
            {
                MessageBox.Show("Файл не найден среди раздаваемых.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            foreach (DataGridViewRow row in dataGridView.Rows)
            {
                if (row.Cells[0].Value.ToString() == sharingFile.FileName)
                {
                    row.Cells[2].Value = "Paused";
                    break;
                }
            }

            await _client.RemoveFile(sharingFile.RootHash);

            MessageBox.Show("Раздача остановлена.", "Torrent", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private async void OnStartDownloadClicked(string fileName)
        {
            var downloadingFile = _downloadingFiles.Where(f => f.FileName == fileName).FirstOrDefault();

            if (downloadingFile == null)
            {
                MessageBox.Show("Образ для раздачи еще не добавлен", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            OnFileStatusChanged(downloadingFile);
            await _client.AddFile(downloadingFile.RootHash, downloadingFile);


            MessageBox.Show("Скачивание запущено.", "Torrent", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private async void OnStopDownloadClicked(string fileName)
        {
            var downloadingFile = _downloadingFiles.Where(f => f.FileName == fileName).FirstOrDefault();

            if (downloadingFile == null)
            {
                MessageBox.Show("Файл не найден среди скачиваемых.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            await _client.RemoveFile(downloadingFile.RootHash);

            MessageBox.Show("Скачивание остановлено.", "Torrent", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }



        private void OnFileStatusChanged(FileMetaData file)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => OnFileStatusChanged(file)));
                return;
            }

            foreach (DataGridViewRow row in dataGridView.Rows)
            {
                if (row.Cells[0].Value.ToString() == file.FileName)
                {
                    row.Cells[2].Value = file.FileStatus.ToString();
                    break;
                }
            }
        }

        private void OnDownloadStatusChanged()
        {
            foreach (DataGridViewRow row in dataGridView.Rows)
            {
                DataGridViewCell cell = row.Cells["DownloadStatusColumn"];

                if (cell.Value == null || string.IsNullOrWhiteSpace(cell.Value.ToString()))
                {
                    cell.Style.BackColor = Color.Green;
                    cell.Style.ForeColor = Color.White;
                    cell.Value = "✓";
                    cell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
                }
            }
        }

        private void dataGridView_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            string columnName = dataGridView.Columns[e.ColumnIndex].Name;
            string fileName = (string)dataGridView.Rows[e.RowIndex].Cells[0].Value;
            string fileType = (string)dataGridView.Rows[e.RowIndex].Cells[7].Value;

            if (columnName == "startButtonColumn")
            {
                if (fileType == "Sharing")
                    OnStartSharingClicked(fileName);
                else if (fileType == "Downloading")
                    OnStartDownloadClicked(fileName);
            }
            else if (columnName == "stopButtonColumn")
            {
                if (fileType == "Sharing")
                    OnStopSharingClicked(fileName);
                else if (fileType == "Downloading")
                    OnStopDownloadClicked(fileName);
            }
            else if (columnName == "deleteButtonColumn")
            {
                DialogResult result = MessageBox.Show("Вы уверены, что хотите удалить этот файл?", "Подтверждение", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                if (result == DialogResult.Yes)
                {
                    if (fileType == "Sharing")
                    {
                        var sharingFile = _sharedFiles.Where(f => f.FileName == fileName).FirstOrDefault();
                        _sharedFiles.RemoveAll(f => f.FileName == fileName);
                        _client.RemoveFile(sharingFile.RootHash);
                    }
                    else if (fileType == "Downloading")
                    {
                        var downloadingFile = _downloadingFiles.Where(f => f.FileName == fileName).FirstOrDefault();
                        _downloadingFiles.RemoveAll(f => f.FileName == fileName);
                        _client.RemoveFile(downloadingFile.RootHash);
                    }

                    dataGridView.Rows.RemoveAt(e.RowIndex);
                }
            }
        }

        private void DataGridView_RowPostPaint(object sender, DataGridViewRowPostPaintEventArgs e)
        {
            try
            {
                DataGridView grid = sender as DataGridView;
                if (grid != null)
                {
                    using (SolidBrush brush = new SolidBrush(grid.RowHeadersDefaultCellStyle.ForeColor))
                    {
                        string rowNumber = (e.RowIndex + 1).ToString();
                        e.Graphics.DrawString(rowNumber, grid.Font, brush,
                            e.RowBounds.Location.X + 10, e.RowBounds.Location.Y + 4);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка в нумерации строк: " + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void MainForm_Load(object sender, EventArgs e)
        {

        }

        private void lblImport_Click(object sender, EventArgs e)
        {

        }

        private void lblCreateFileImage_Click(object sender, EventArgs e)
        {

        }

        private void panelDownloadButtons_Paint(object sender, PaintEventArgs e)
        {

        }

        private void lblSelectFile_Click(object sender, EventArgs e)
        {

        }
    }
}