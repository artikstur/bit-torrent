namespace TorrentWinFormsApp
{

    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        private void OnStartSharingClicked(object sender, EventArgs e)
        {
            MessageBox.Show("������� ��������.", "Torrent", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void OnStopSharingClicked(object sender, EventArgs e)
        {
            MessageBox.Show("������� �����������.", "Torrent", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void OnStartDownloadClicked(object sender, EventArgs e)
        {
            MessageBox.Show("���������� ��������.", "Torrent", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void OnStopDownloadClicked(object sender, EventArgs e)
        {
            MessageBox.Show("���������� �����������.", "Torrent", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void OnSelectFileClicked(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    listSharedFiles.Items.Add(openFileDialog.FileName);
                }
            }
        }

        private void OnImportClicked(object sender, EventArgs e)
        {
            MessageBox.Show("������ ������ ��������.", "Torrent", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void lblDownloading_Click(object sender, EventArgs e)
        {

        }

        private void MainForm_Load(object sender, EventArgs e)
        {

        }
    }
}
