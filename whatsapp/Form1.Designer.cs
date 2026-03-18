namespace whatsapp
{
    public partial class Form1 : Form
    {
        private Panel pnlHeader;
        private Panel pnlSidebar;
        private Panel pnlChatContainer;
        private Panel pnlInputContainer;
        private Panel pnlLog;

        private Label lblConnectionStatus;
        private NumericUpDown numLocalPort;
        private NumericUpDown numRemotePort;
        private TextBox txtRemoteIP;
        private TextBox txtEncryptionKey;

        private Button btnStartListener;
        private Button btnStopListener;
        private Button btnSendMessage;
        private Button btnToggleTheme;

        private TextBox txtChatMessages;
        private TextBox txtMessageInput;
        private TextBox txtTechnicalLog;

        private void InitializeComponent()
        {
            this.SuspendLayout();

            // FORM
            this.ClientSize = new Size(1400, 750);
            this.Text = "Chat P2P Seguro";
            this.BackColor = Color.FromArgb(15, 20, 25);
            this.StartPosition = FormStartPosition.CenterScreen;

            // ================= HEADER =================
            pnlHeader = new Panel
            {
                Dock = DockStyle.Top,
                Height = 70,
                BackColor = Color.FromArgb(17, 27, 33),
                Padding = new Padding(10)
            };

            lblConnectionStatus = new Label
            {
                Text = "● Desconectado",
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                Location = new Point(20, 22),
                AutoSize = true
            };

            var lblLocalPortLabel = new Label
            {
                Text = "Puerto Local:",
                ForeColor = Color.LightGray,
                Font = new Font("Segoe UI", 9),
                Location = new Point(300, 15),
                Size = new Size(90, 20)
            };

            numLocalPort = new NumericUpDown
            {
                Location = new Point(300, 35),
                Size = new Size(80, 25),
                Minimum = 1024,
                Maximum = 65535,
                Value = 5000,
                BackColor = Color.FromArgb(37, 47, 53),
                ForeColor = Color.White,
                BorderStyle = BorderStyle.None
            };
            ((System.ComponentModel.ISupportInitialize)numLocalPort).BeginInit();

            btnStartListener = new Button
            {
                Text = "Conectar",
                Location = new Point(410, 22),
                Size = new Size(90, 35),
                BackColor = Color.FromArgb(6, 182, 112),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9, FontStyle.Bold)
            };
            btnStartListener.Click += BtnStartListener_Click;

            btnStopListener = new Button
            {
                Text = "Desconectar",
                Location = new Point(510, 22),
                Size = new Size(90, 35),
                BackColor = Color.FromArgb(230, 124, 115),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Enabled = false
            };
            btnStopListener.Click += BtnStopListener_Click;

            btnToggleTheme = new Button
            {
                Text = "🌙",
                Location = new Point(1270, 10),
                Size = new Size(100, 50),
                BackColor = Color.FromArgb(37, 47, 53),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter
            };
            btnToggleTheme.Click += BtnToggleTheme_Click;

            pnlHeader.Controls.Add(lblConnectionStatus);
            pnlHeader.Controls.Add(lblLocalPortLabel);
            pnlHeader.Controls.Add(numLocalPort);
            pnlHeader.Controls.Add(btnStartListener);
            pnlHeader.Controls.Add(btnStopListener);
            pnlHeader.Controls.Add(btnToggleTheme);

            // ================= SIDEBAR =================
            pnlSidebar = new Panel
            {
                Dock = DockStyle.Left,
                Width = 280,
                BackColor = Color.FromArgb(17, 27, 33)
            };

            Label lblConfig = new Label
            {
                Text = "CONFIGURACIÓN",
                ForeColor = Color.FromArgb(6, 182, 112),
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Location = new Point(15, 15),
                AutoSize = true
            };

            Label lblIP = new Label
            {
                Text = "IP Destino:",
                ForeColor = Color.LightGray,
                Location = new Point(15, 50)
            };

            txtRemoteIP = new TextBox
            {
                Location = new Point(15, 70),
                Width = 250,
                Text = "127.0.0.1",
                BackColor = Color.FromArgb(37, 47, 53),
                ForeColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };

            Label lblPort = new Label
            {
                Text = "Puerto:",
                ForeColor = Color.LightGray,
                Location = new Point(15, 110)
            };

            numRemotePort = new NumericUpDown
            {
                Location = new Point(15, 130),
                Width = 250,
                Minimum = 1024,
                Maximum = 65535,
                Value = 5001,
                BackColor = Color.FromArgb(37, 47, 53),
                ForeColor = Color.White
            };

            Label lblKey = new Label
            {
                Text = "Clave:",
                ForeColor = Color.LightGray,
                Location = new Point(15, 170)
            };

            txtEncryptionKey = new TextBox
            {
                Location = new Point(15, 190),
                Width = 250,
                Text = "SharedKey123",
                BackColor = Color.FromArgb(37, 47, 53),
                ForeColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };

            pnlSidebar.Controls.Add(lblConfig);
            pnlSidebar.Controls.Add(lblIP);
            pnlSidebar.Controls.Add(txtRemoteIP);
            pnlSidebar.Controls.Add(lblPort);
            pnlSidebar.Controls.Add(numRemotePort);
            pnlSidebar.Controls.Add(lblKey);
            pnlSidebar.Controls.Add(txtEncryptionKey);

            // ================= CHAT =================
            pnlChatContainer = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(25, 35, 40)
            };

            txtChatMessages = new TextBox
            {
                Dock = DockStyle.Fill,
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Vertical,
                BackColor = Color.FromArgb(17, 27, 33),
                ForeColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };

            pnlInputContainer = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 60
            };

            txtMessageInput = new TextBox
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(37, 47, 53),
                ForeColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };

            btnSendMessage = new Button
            {
                Text = "Enviar",
                Dock = DockStyle.Right,
                Width = 80,
                BackColor = Color.FromArgb(6, 182, 112),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };

            pnlInputContainer.Controls.Add(txtMessageInput);
            pnlInputContainer.Controls.Add(btnSendMessage);

            pnlChatContainer.Controls.Add(txtChatMessages);
            pnlChatContainer.Controls.Add(pnlInputContainer);

            // ================= LOG =================
            pnlLog = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 100,
                BackColor = Color.FromArgb(15, 20, 25)
            };

            txtTechnicalLog = new TextBox
            {
                Dock = DockStyle.Fill,
                Multiline = true,
                ReadOnly = true,
                BackColor = Color.FromArgb(17, 27, 33),
                ForeColor = Color.LightGray,
                BorderStyle = BorderStyle.None
            };

            pnlLog.Controls.Add(txtTechnicalLog);

            // ================= ADD CONTROLS =================
            this.Controls.Add(pnlChatContainer);
            this.Controls.Add(pnlSidebar);
            this.Controls.Add(pnlHeader);
            this.Controls.Add(pnlLog);

            this.ResumeLayout(false);
        }
    }
}
