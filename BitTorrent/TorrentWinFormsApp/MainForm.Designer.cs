namespace TorrentWinFormsApp
{
    partial class MainForm
    {
        private System.Windows.Forms.Button btnStartSharing;
        private System.Windows.Forms.Button btnStopSharing;
        private System.Windows.Forms.Button btnStartDownload;
        private System.Windows.Forms.Button btnStopDownload;
        private System.Windows.Forms.Button btnSelectFile;
        private System.Windows.Forms.Button btnImport;
        private System.Windows.Forms.ListBox listSharedFiles;
        private System.Windows.Forms.ListBox listDownloadingFiles;
        private System.Windows.Forms.Label lblShared;
        private System.Windows.Forms.Label lblDownloading;
        private FlowLayoutPanel panelDownloadButtons;

        private void InitializeComponent()
        {
            btnStartSharing = new Button();
            btnStopSharing = new Button();
            btnStartDownload = new Button();
            btnStopDownload = new Button();
            btnSelectFile = new Button();
            btnImport = new Button();
            listSharedFiles = new ListBox();
            listDownloadingFiles = new ListBox();
            lblShared = new Label();
            lblDownloading = new Label();
            panelDownloadButtons = new FlowLayoutPanel();
            SuspendLayout();
            // 
            // btnStartSharing
            // 
            btnStartSharing.BackColor = Color.FromArgb(52, 199, 89);
            btnStartSharing.Location = new Point(12, 170);
            btnStartSharing.Name = "btnStartSharing";
            btnStartSharing.Size = new Size(154, 56);
            btnStartSharing.TabIndex = 0;
            btnStartSharing.Text = "Запустить раздачу";
            btnStartSharing.UseVisualStyleBackColor = false;
            btnStartSharing.Click += OnStartSharingClicked;
            // 
            // btnStopSharing
            // 
            btnStopSharing.BackColor = Color.FromArgb(255, 59, 48);
            btnStopSharing.Location = new Point(191, 170);
            btnStopSharing.Name = "btnStopSharing";
            btnStopSharing.Size = new Size(136, 56);
            btnStopSharing.TabIndex = 1;
            btnStopSharing.Text = "Остановить раздачу";
            btnStopSharing.UseVisualStyleBackColor = false;
            btnStopSharing.Click += OnStopSharingClicked;
            // 
            // btnStartDownload
            // 
            btnStartDownload.BackColor = Color.FromArgb(52, 199, 89);
            btnStartDownload.Location = new Point(424, 171);
            btnStartDownload.Name = "btnStartDownload";
            btnStartDownload.Size = new Size(147, 56);
            btnStartDownload.TabIndex = 2;
            btnStartDownload.Text = "Запустить скачивание";
            btnStartDownload.UseVisualStyleBackColor = false;
            btnStartDownload.Click += OnStartDownloadClicked;
            // 
            // btnStopDownload
            // 
            btnStopDownload.BackColor = Color.FromArgb(255, 59, 48);
            btnStopDownload.Location = new Point(599, 171);
            btnStopDownload.Name = "btnStopDownload";
            btnStopDownload.Size = new Size(140, 56);
            btnStopDownload.TabIndex = 3;
            btnStopDownload.Text = "Остановить скачивание";
            btnStopDownload.UseVisualStyleBackColor = false;
            btnStopDownload.Click += OnStopDownloadClicked;
            // 
            // btnSelectFile
            // 
            btnSelectFile.BackColor = Color.FromArgb(0, 122, 255);
            btnSelectFile.Location = new Point(12, 254);
            btnSelectFile.Name = "btnSelectFile";
            btnSelectFile.Size = new Size(124, 46);
            btnSelectFile.TabIndex = 4;
            btnSelectFile.Text = "Выбрать файл";
            btnSelectFile.UseVisualStyleBackColor = false;
            btnSelectFile.Click += OnSelectFileClicked;
            // 
            // btnImport
            // 
            btnImport.BackColor = Color.FromArgb(255, 149, 0);
            btnImport.Location = new Point(424, 254);
            btnImport.Name = "btnImport";
            btnImport.Size = new Size(147, 46);
            btnImport.TabIndex = 5;
            btnImport.Text = "Импорт образа";
            btnImport.UseVisualStyleBackColor = false;
            btnImport.Click += OnImportClicked;
            // 
            // listSharedFiles
            // 
            listSharedFiles.Location = new Point(20, 370);
            listSharedFiles.Name = "listSharedFiles";
            listSharedFiles.Size = new Size(337, 44);
            listSharedFiles.TabIndex = 7;
            // 
            // listDownloadingFiles
            // 
            listDownloadingFiles.Location = new Point(406, 370);
            listDownloadingFiles.Name = "listDownloadingFiles";
            listDownloadingFiles.Size = new Size(345, 44);
            listDownloadingFiles.TabIndex = 9;
            // 
            // lblShared
            // 
            lblShared.Location = new Point(20, 340);
            lblShared.Name = "lblShared";
            lblShared.Size = new Size(100, 23);
            lblShared.TabIndex = 6;
            lblShared.Text = "Раздается:";
            // 
            // lblDownloading
            // 
            lblDownloading.Location = new Point(517, 132);
            lblDownloading.Name = "lblDownloading";
            lblDownloading.Size = new Size(146, 23);
            lblDownloading.TabIndex = 8;
            lblDownloading.Text = "Загружается:";
            lblDownloading.Click += lblDownloading_Click;
            // 
            // panelDownloadButtons
            // 
            panelDownloadButtons.AutoScroll = true;
            panelDownloadButtons.FlowDirection = FlowDirection.TopDown;
            panelDownloadButtons.Location = new Point(20, 420);
            panelDownloadButtons.Name = "panelDownloadButtons";
            panelDownloadButtons.Size = new Size(337, 44);
            panelDownloadButtons.TabIndex = 10;
            panelDownloadButtons.WrapContents = false;
            // 
            // MainForm
            // 
            ClientSize = new Size(790, 571);
            Controls.Add(btnStartSharing);
            Controls.Add(btnStopSharing);
            Controls.Add(btnStartDownload);
            Controls.Add(btnStopDownload);
            Controls.Add(btnSelectFile);
            Controls.Add(btnImport);
            Controls.Add(lblShared);
            Controls.Add(listSharedFiles);
            Controls.Add(lblDownloading);
            Controls.Add(listDownloadingFiles);
            Controls.Add(panelDownloadButtons);
            Name = "MainForm";
            Text = "Torrent";
            Load += MainForm_Load;
            ResumeLayout(false);
        }
    }

}
