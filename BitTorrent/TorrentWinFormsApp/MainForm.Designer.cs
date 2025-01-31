namespace TorrentWinFormsApp
{
    partial class MainForm
    {
        private System.Windows.Forms.Button btnSelectFile;
        private System.Windows.Forms.Button btnImport;
        private System.Windows.Forms.Button btnCreateFileImage;
        private System.Windows.Forms.Label lblSelectFile;
        private System.Windows.Forms.Label lblCreateFileImage;
        private System.Windows.Forms.Label lblTorrentTitle;

        private FlowLayoutPanel panelDownloadButtons;
        private DataGridView dataGridView;

        private void InitializeComponent()
        {
            DataGridViewCellStyle dataGridViewCellStyle1 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle2 = new DataGridViewCellStyle();
            lblTorrentTitle = new Label();
            btnSelectFile = new Button();
            btnImport = new Button();
            btnCreateFileImage = new Button();
            lblSelectFile = new Label();
            lblCreateFileImage = new Label();
            panelDownloadButtons = new FlowLayoutPanel();
            dataGridView = new DataGridView();
            dataGridViewTextBoxColumn1 = new DataGridViewTextBoxColumn();
            dataGridViewTextBoxColumn2 = new DataGridViewTextBoxColumn();
            dataGridViewTextBoxColumn3 = new DataGridViewTextBoxColumn();
            startButtonColumn = new DataGridViewButtonColumn();
            stopButtonColumn = new DataGridViewButtonColumn();
            deleteButtonColumn = new DataGridViewButtonColumn();
            DownloadStatusColumn = new DataGridViewTextBoxColumn();
            FileTypeColumn = new DataGridViewTextBoxColumn();
            ((System.ComponentModel.ISupportInitialize)dataGridView).BeginInit();
            SuspendLayout();
            // 
            // lblTorrentTitle
            // 
            lblTorrentTitle.AutoSize = true;
            lblTorrentTitle.Font = new Font("Segoe UI", 20F, FontStyle.Bold);
            lblTorrentTitle.ForeColor = Color.FromArgb(0, 122, 255);
            lblTorrentTitle.Location = new Point(12, 9);
            lblTorrentTitle.Name = "lblTorrentTitle";
            lblTorrentTitle.Size = new Size(138, 46);
            lblTorrentTitle.TabIndex = 11;
            lblTorrentTitle.Text = "Torrent";
            // 
            // btnSelectFile
            // 
            btnSelectFile.BackColor = Color.FromArgb(0, 122, 255);
            btnSelectFile.FlatStyle = FlatStyle.Flat;
            btnSelectFile.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnSelectFile.ForeColor = Color.White;
            btnSelectFile.Location = new Point(26, 212);
            btnSelectFile.Name = "btnSelectFile";
            btnSelectFile.Size = new Size(200, 40);
            btnSelectFile.TabIndex = 4;
            btnSelectFile.Text = "Загрузить файл";
            btnSelectFile.UseVisualStyleBackColor = false;
            btnSelectFile.Click += OnSelectSharingFileClicked;
            // 
            // btnImport
            // 
            btnImport.BackColor = Color.FromArgb(0, 122, 255);
            btnImport.FlatStyle = FlatStyle.Flat;
            btnImport.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnImport.ForeColor = Color.White;
            btnImport.Location = new Point(242, 212);
            btnImport.Name = "btnImport";
            btnImport.Size = new Size(200, 40);
            btnImport.TabIndex = 5;
            btnImport.Text = "Загрузить образ";
            btnImport.UseVisualStyleBackColor = false;
            btnImport.Click += OnImportClicked;
            // 
            // btnCreateFileImage
            // 
            btnCreateFileImage.BackColor = Color.FromArgb(8, 63, 191);
            btnCreateFileImage.FlatStyle = FlatStyle.Flat;
            btnCreateFileImage.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnCreateFileImage.ForeColor = Color.White;
            btnCreateFileImage.Location = new Point(26, 127);
            btnCreateFileImage.Name = "btnCreateFileImage";
            btnCreateFileImage.Size = new Size(200, 40);
            btnCreateFileImage.TabIndex = 5;
            btnCreateFileImage.Text = "Создать образ";
            btnCreateFileImage.UseVisualStyleBackColor = false;
            btnCreateFileImage.Click += OnSelectCreateImageFileClicked;
            // 
            // lblSelectFile
            // 
            lblSelectFile.AutoSize = true;
            lblSelectFile.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblSelectFile.ForeColor = Color.FromArgb(64, 64, 64);
            lblSelectFile.Location = new Point(26, 186);
            lblSelectFile.Name = "lblSelectFile";
            lblSelectFile.Size = new Size(839, 23);
            lblSelectFile.TabIndex = 0;
            lblSelectFile.Text = "Выберите файл для раздачи или выберите образ, по которому вы сможете скачать нужный файл";
            lblSelectFile.Click += lblSelectFile_Click;
            // 
            // lblCreateFileImage
            // 
            lblCreateFileImage.AutoSize = true;
            lblCreateFileImage.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblCreateFileImage.ForeColor = Color.FromArgb(64, 64, 64);
            lblCreateFileImage.Location = new Point(24, 101);
            lblCreateFileImage.Name = "lblCreateFileImage";
            lblCreateFileImage.Size = new Size(208, 23);
            lblCreateFileImage.TabIndex = 2;
            lblCreateFileImage.Text = "Создание образа файла";
            lblCreateFileImage.Click += lblCreateFileImage_Click;
            // 
            // panelDownloadButtons
            // 
            panelDownloadButtons.AutoScroll = true;
            panelDownloadButtons.FlowDirection = FlowDirection.TopDown;
            panelDownloadButtons.Location = new Point(246, 127);
            panelDownloadButtons.Name = "panelDownloadButtons";
            panelDownloadButtons.Size = new Size(258, 46);
            panelDownloadButtons.TabIndex = 10;
            panelDownloadButtons.WrapContents = false;
            panelDownloadButtons.Paint += panelDownloadButtons_Paint;
            // 
            // dataGridView
            // 
            dataGridView.AllowUserToAddRows = false;
            dataGridView.BackgroundColor = Color.White;
            dataGridView.BorderStyle = BorderStyle.None;
            dataGridViewCellStyle1.BackColor = Color.FromArgb(240, 240, 240);
            dataGridViewCellStyle1.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            dataGridViewCellStyle1.ForeColor = Color.FromArgb(64, 64, 64);
            dataGridView.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            dataGridView.ColumnHeadersHeight = 29;
            dataGridView.Columns.AddRange(new DataGridViewColumn[] { dataGridViewTextBoxColumn1, dataGridViewTextBoxColumn2, dataGridViewTextBoxColumn3, startButtonColumn, stopButtonColumn, deleteButtonColumn, DownloadStatusColumn, FileTypeColumn });
            dataGridView.Location = new Point(-2, 295);
            dataGridView.Name = "dataGridView";
            dataGridViewCellStyle2.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle2.BackColor = SystemColors.Control;
            dataGridViewCellStyle2.Font = new Font("Segoe UI", 9F);
            dataGridViewCellStyle2.ForeColor = Color.FromArgb(64, 64, 64);
            dataGridViewCellStyle2.SelectionBackColor = Color.FromArgb(0, 122, 255);
            dataGridViewCellStyle2.SelectionForeColor = Color.White;
            dataGridViewCellStyle2.WrapMode = DataGridViewTriState.True;
            dataGridView.RowHeadersDefaultCellStyle = dataGridViewCellStyle2;
            dataGridView.RowHeadersWidth = 50;
            dataGridView.Size = new Size(896, 250);
            dataGridView.TabIndex = 0;
            dataGridView.CellContentClick += dataGridView_CellContentClick;
            dataGridView.RowPostPaint += DataGridView_RowPostPaint;
            // 
            // dataGridViewTextBoxColumn1
            // 
            dataGridViewTextBoxColumn1.HeaderText = "Название файла";
            dataGridViewTextBoxColumn1.MinimumWidth = 6;
            dataGridViewTextBoxColumn1.Name = "dataGridViewTextBoxColumn1";
            dataGridViewTextBoxColumn1.ReadOnly = true;
            dataGridViewTextBoxColumn1.Resizable = DataGridViewTriState.True;
            dataGridViewTextBoxColumn1.Width = 200;
            // 
            // dataGridViewTextBoxColumn2
            // 
            dataGridViewTextBoxColumn2.HeaderText = "Размер";
            dataGridViewTextBoxColumn2.MinimumWidth = 6;
            dataGridViewTextBoxColumn2.Name = "dataGridViewTextBoxColumn2";
            dataGridViewTextBoxColumn2.ReadOnly = true;
            dataGridViewTextBoxColumn2.Width = 75;
            // 
            // dataGridViewTextBoxColumn3
            // 
            dataGridViewTextBoxColumn3.HeaderText = "Состояние";
            dataGridViewTextBoxColumn3.MinimumWidth = 6;
            dataGridViewTextBoxColumn3.Name = "dataGridViewTextBoxColumn3";
            dataGridViewTextBoxColumn3.ReadOnly = true;
            dataGridViewTextBoxColumn3.Width = 125;
            // 
            // startButtonColumn
            // 
            startButtonColumn.HeaderText = "Запустить";
            startButtonColumn.MinimumWidth = 6;
            startButtonColumn.Name = "startButtonColumn";
            startButtonColumn.Text = "▶";
            startButtonColumn.UseColumnTextForButtonValue = true;
            startButtonColumn.Width = 125;
            // 
            // stopButtonColumn
            // 
            stopButtonColumn.HeaderText = "Остановить";
            stopButtonColumn.MinimumWidth = 6;
            stopButtonColumn.Name = "stopButtonColumn";
            stopButtonColumn.Text = "⏸";
            stopButtonColumn.UseColumnTextForButtonValue = true;
            stopButtonColumn.Width = 125;
            // 
            // deleteButtonColumn
            // 
            deleteButtonColumn.HeaderText = "Удалить";
            deleteButtonColumn.MinimumWidth = 6;
            deleteButtonColumn.Name = "deleteButtonColumn";
            deleteButtonColumn.Text = "🗑";
            deleteButtonColumn.UseColumnTextForButtonValue = true;
            deleteButtonColumn.Width = 125;
            // 
            // DownloadStatusColumn
            // 
            DownloadStatusColumn.HeaderText = "Статус";
            DownloadStatusColumn.MinimumWidth = 6;
            DownloadStatusColumn.Name = "DownloadStatusColumn";
            DownloadStatusColumn.Width = 70;
            // 
            // FileTypeColumn
            // 
            FileTypeColumn.HeaderText = "Column1";
            FileTypeColumn.MinimumWidth = 6;
            FileTypeColumn.Name = "FileTypeColumn";
            FileTypeColumn.Visible = false;
            FileTypeColumn.Width = 125;
            // 
            // MainForm
            // 
            BackColor = Color.White;
            ClientSize = new Size(892, 570);
            Controls.Add(btnImport);
            Controls.Add(lblSelectFile);
            Controls.Add(lblCreateFileImage);
            Controls.Add(dataGridView);
            Controls.Add(btnSelectFile);
            Controls.Add(btnCreateFileImage);
            Controls.Add(panelDownloadButtons);
            Controls.Add(lblTorrentTitle);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            Name = "MainForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Torrent Client";
            Load += MainForm_Load;
            ((System.ComponentModel.ISupportInitialize)dataGridView).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        private DataGridViewTextBoxColumn dataGridViewTextBoxColumn1;
        private DataGridViewTextBoxColumn dataGridViewTextBoxColumn2;
        private DataGridViewTextBoxColumn dataGridViewTextBoxColumn3;
        private DataGridViewButtonColumn startButtonColumn;
        private DataGridViewButtonColumn stopButtonColumn;
        private DataGridViewButtonColumn deleteButtonColumn;
        private DataGridViewTextBoxColumn DownloadStatusColumn;
        private DataGridViewTextBoxColumn FileTypeColumn;
    }
}