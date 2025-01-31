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
        private System.Windows.Forms.Button btnCreateFileImage;
        private FlowLayoutPanel panelDownloadButtons;
        private DataGridView dataGridView;

        private void InitializeComponent()
        {
            btnStartSharing = new Button();
            btnStopSharing = new Button();
            btnStartDownload = new Button();
            btnStopDownload = new Button();
            btnSelectFile = new Button();
            btnImport = new Button();
            btnCreateFileImage = new Button();
            panelDownloadButtons = new FlowLayoutPanel();
            dataGridView = new DataGridView();
            dataGridViewTextBoxColumn1 = new DataGridViewTextBoxColumn();
            dataGridViewTextBoxColumn2 = new DataGridViewTextBoxColumn();
            dataGridViewTextBoxColumn3 = new DataGridViewTextBoxColumn();
            ((System.ComponentModel.ISupportInitialize)dataGridView).BeginInit();
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
            btnStopSharing.Location = new Point(205, 171);
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
            btnSelectFile.Location = new Point(75, 93);
            btnSelectFile.Name = "btnSelectFile";
            btnSelectFile.Size = new Size(266, 46);
            btnSelectFile.TabIndex = 4;
            btnSelectFile.Text = "Загрузить файл для раздачи";
            btnSelectFile.UseVisualStyleBackColor = false;
            btnSelectFile.Click += OnSelectSharingFileClicked;
            // 
            // btnImport
            // 
            btnImport.BackColor = Color.FromArgb(255, 149, 0);
            btnImport.Location = new Point(412, 93);
            btnImport.Name = "btnImport";
            btnImport.Size = new Size(278, 46);
            btnImport.TabIndex = 5;
            btnImport.Text = "Загрузить образ для скачивания";
            btnImport.UseVisualStyleBackColor = false;
            btnImport.Click += OnImportClicked;
            // 
            // btnCreateFileImage
            // 
            btnCreateFileImage.BackColor = Color.FromArgb(255, 149, 0);
            btnCreateFileImage.Location = new Point(12, 12);
            btnCreateFileImage.Name = "btnCreateFileImage";
            btnCreateFileImage.Size = new Size(178, 46);
            btnCreateFileImage.TabIndex = 5;
            btnCreateFileImage.Text = "Создать образ файла";
            btnCreateFileImage.UseVisualStyleBackColor = false;
            btnCreateFileImage.Click += OnSelectCreateImageFileClicked;
            // 
            // panelDownloadButtons
            // 
            panelDownloadButtons.AutoScroll = true;
            panelDownloadButtons.FlowDirection = FlowDirection.TopDown;
            panelDownloadButtons.Location = new Point(205, 14);
            panelDownloadButtons.Name = "panelDownloadButtons";
            panelDownloadButtons.Size = new Size(234, 44);
            panelDownloadButtons.TabIndex = 10;
            panelDownloadButtons.WrapContents = false;
            // 
            // dataGridView
            // 
            dataGridView.ColumnHeadersHeight = 29;
            dataGridView.Columns.AddRange(new DataGridViewColumn[] { dataGridViewTextBoxColumn1, dataGridViewTextBoxColumn2, dataGridViewTextBoxColumn3 });
            dataGridView.Location = new Point(5, 359);
            dataGridView.Name = "dataGridView";
            dataGridView.RowHeadersWidth = 51;
            dataGridView.Size = new Size(773, 200);
            dataGridView.TabIndex = 0;
            dataGridView.CellContentClick += dataGridView_CellContentClick;
            // 
            // dataGridViewTextBoxColumn1
            // 
            dataGridViewTextBoxColumn1.HeaderText = "Название файла";
            dataGridViewTextBoxColumn1.MinimumWidth = 6;
            dataGridViewTextBoxColumn1.Name = "dataGridViewTextBoxColumn1";
            dataGridViewTextBoxColumn1.Width = 200;
            // 
            // dataGridViewTextBoxColumn2
            // 
            dataGridViewTextBoxColumn2.HeaderText = "Размер файла";
            dataGridViewTextBoxColumn2.MinimumWidth = 6;
            dataGridViewTextBoxColumn2.Name = "dataGridViewTextBoxColumn2";
            dataGridViewTextBoxColumn2.Width = 125;
            // 
            // dataGridViewTextBoxColumn3
            // 
            dataGridViewTextBoxColumn3.HeaderText = "Состояние";
            dataGridViewTextBoxColumn3.MinimumWidth = 6;
            dataGridViewTextBoxColumn3.Name = "dataGridViewTextBoxColumn3";
            dataGridViewTextBoxColumn3.Width = 125;
            // 
            // MainForm
            // 
            ClientSize = new Size(790, 571);
            Controls.Add(dataGridView);
            Controls.Add(btnStartSharing);
            Controls.Add(btnStopSharing);
            Controls.Add(btnStartDownload);
            Controls.Add(btnStopDownload);
            Controls.Add(btnSelectFile);
            Controls.Add(btnImport);
            Controls.Add(btnCreateFileImage);
            Controls.Add(panelDownloadButtons);
            Name = "MainForm";
            Text = "Torrent";
            ((System.ComponentModel.ISupportInitialize)dataGridView).EndInit();
            ResumeLayout(false);
        }

        private DataGridViewTextBoxColumn dataGridViewTextBoxColumn1;
        private DataGridViewTextBoxColumn dataGridViewTextBoxColumn2;
        private DataGridViewTextBoxColumn dataGridViewTextBoxColumn3;
    }
}
