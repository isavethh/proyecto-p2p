namespace whatsapp
{
    public partial class Form1 : Form
    {
        private Panel pnlServerRail;
        private Panel pnlHeader;
        private Panel pnlSidebar;
        private Panel pnlChatContainer;
        private Panel pnlInputContainer;
        private Panel pnlMessageInputShell;
        private Panel pnlLog;

        private Label lblConnectionStatus;
        private NumericUpDown numLocalPort;
        private TextBox txtEncryptionKey;

        private Button btnStartListener;
        private Button btnStopListener;
        private Button btnSendMessage;
        private Button btnToggleTheme;
        private Button btnNewChat;

        private FlowLayoutPanel txtChatMessages;
        private TextBox txtMessageInput;
        private TextBox txtTechnicalLog;
        private ListBox lstChats;

        private void InitializeComponent()
        {
            this.SuspendLayout();

            // FORM
            this.ClientSize = new Size(1500, 820);
            this.Text = "FBIchat";
            this.BackColor = Color.FromArgb(49, 51, 56);
            this.StartPosition = FormStartPosition.CenterScreen;

            // ================= SERVER RAIL =================
            pnlServerRail = new Panel
            {
                Dock = DockStyle.Left,
                Width = 76,
                BackColor = Color.FromArgb(30, 31, 34),
                Padding = new Padding(10, 12, 10, 12)
            };

            var lblHomeServer = new Label
            {
                Text = "💬",
                ForeColor = Color.White,
                BackColor = Color.FromArgb(88, 101, 242),
                Font = new Font("Segoe UI Emoji", 16, FontStyle.Regular),
                TextAlign = ContentAlignment.MiddleCenter,
                Location = new Point(10, 12),
                Size = new Size(56, 56)
            };

            var lblExtraServer = new Label
            {
                Text = "+",
                ForeColor = Color.FromArgb(35, 165, 90),
                BackColor = Color.FromArgb(43, 45, 49),
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter,
                Location = new Point(10, 80),
                Size = new Size(56, 56)
            };

            pnlServerRail.Controls.Add(lblHomeServer);
            pnlServerRail.Controls.Add(lblExtraServer);

            // ================= HEADER =================
            pnlHeader = new Panel
            {
                Dock = DockStyle.Top,
                Height = 56,
                BackColor = Color.FromArgb(49, 51, 56),
                Padding = new Padding(12, 10, 12, 10)
            };

            lblConnectionStatus = new Label
            {
                Text = "● Desconectado",
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Location = new Point(15, 19),
                AutoSize = true
            };

            var lblHeaderTitle = new Label
            {
                Text = "# chat-general",
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                Location = new Point(180, 15),
                AutoSize = true
            };

            var lblLocalPortLabel = new Label
            {
                Text = "Puerto Local:",
                ForeColor = Color.FromArgb(185, 187, 190),
                Font = new Font("Segoe UI", 9),
                Location = new Point(330, 5),
                Size = new Size(90, 24),
                TextAlign = ContentAlignment.MiddleLeft
            };

            numLocalPort = new NumericUpDown
            {
                Location = new Point(420, 10),
                Size = new Size(100, 36),
                Minimum = 1024,
                Maximum = 65535,
                Value = 5000,
                BackColor = Color.FromArgb(56, 58, 64),
                ForeColor = Color.White,
                BorderStyle = BorderStyle.None,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                TextAlign = HorizontalAlignment.Center,
                InterceptArrowKeys = true
            };
            numLocalPort.KeyDown += NumLocalPort_KeyDown;
            ((System.ComponentModel.ISupportInitialize)numLocalPort).BeginInit();

            btnStartListener = new Button
            {
                Text = "Conectar",
                Location = new Point(535, 13),
                Size = new Size(90, 30),
                BackColor = Color.FromArgb(35, 165, 90),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9, FontStyle.Bold)
            };
            btnStartListener.FlatAppearance.BorderSize = 0;
            btnStartListener.Click += BtnStartListener_Click;

            btnStopListener = new Button
            {
                Text = "Desconectar",
                Location = new Point(631, 13),
                Size = new Size(105, 30),
                BackColor = Color.FromArgb(237, 66, 69),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Enabled = false
            };
            btnStopListener.FlatAppearance.BorderSize = 0;
            btnStopListener.Click += BtnStopListener_Click;

            btnToggleTheme = new Button
            {
                Text = "🌙",
                Location = new Point(1380, 10),
                Size = new Size(90, 36),
                BackColor = Color.FromArgb(43, 45, 49),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter
            };
            btnToggleTheme.FlatAppearance.BorderSize = 0;
            btnToggleTheme.Click += BtnToggleTheme_Click;

            pnlHeader.Controls.Add(lblConnectionStatus);
            pnlHeader.Controls.Add(lblHeaderTitle);
            pnlHeader.Controls.Add(lblLocalPortLabel);
            pnlHeader.Controls.Add(numLocalPort);
            pnlHeader.Controls.Add(btnStartListener);
            pnlHeader.Controls.Add(btnStopListener);
            pnlHeader.Controls.Add(btnToggleTheme);

            // ================= SIDEBAR =================
            pnlSidebar = new Panel
            {
                Dock = DockStyle.Left,
                Width = 320,
                BackColor = Color.FromArgb(43, 45, 49),
                Padding = new Padding(14, 12, 14, 12)
            };

            Label lblConfig = new Label
            {
                Text = "MENSAJES DIRECTOS",
                ForeColor = Color.FromArgb(148, 155, 164),
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Location = new Point(14, 12),
                AutoSize = true
            };

            btnNewChat = new Button
            {
                Text = "+ Nuevo chat",
                Location = new Point(14, 35),
                Size = new Size(292, 34),
                BackColor = Color.FromArgb(64, 68, 75),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9, FontStyle.Bold)
            };
            btnNewChat.FlatAppearance.BorderSize = 0;
            btnNewChat.Click += BtnNewChat_Click;

            Label lblKey = new Label
            {
                Text = "Clave:",
                ForeColor = Color.FromArgb(185, 187, 190),
                Font = new Font("Segoe UI", 8),
                Location = new Point(14, 82),
                AutoSize = true
            };

            txtEncryptionKey = new TextBox
            {
                Location = new Point(14, 108),
                Width = 292,
                Height = 50,
                Text = "SharedKey123",
                BackColor = Color.FromArgb(30, 31, 34),
                ForeColor = Color.White,
                BorderStyle = BorderStyle.None,
                Multiline = true
            };

            lstChats = new ListBox
            {
                Location = new Point(14, 156),
                Size = new Size(292, 600),
                BackColor = Color.FromArgb(43, 45, 49),
                ForeColor = Color.White,
                BorderStyle = BorderStyle.None,
                Font = new Font("Segoe UI", 10)
            };
            lstChats.SelectedIndexChanged += LstChats_SelectedIndexChanged;

            pnlSidebar.Controls.Add(lblConfig);
            pnlSidebar.Controls.Add(btnNewChat);
            pnlSidebar.Controls.Add(lblKey);
            pnlSidebar.Controls.Add(txtEncryptionKey);
            pnlSidebar.Controls.Add(lstChats);

            // ================= CHAT =================
            pnlChatContainer = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(49, 51, 56),
                Padding = new Padding(0)
            };

            txtChatMessages = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                BackColor = Color.FromArgb(49, 51, 56),
                Padding = new Padding(16, 16, 16, 20),
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false
            };
            txtChatMessages.Resize += TxtChatMessages_Resize;

            pnlInputContainer = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 140,
                BackColor = Color.FromArgb(49, 51, 56),
                Padding = new Padding(16, 12, 16, 14)
            };

            pnlMessageInputShell = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(56, 58, 64),
                Padding = new Padding(16, 10, 16, 10)
            };

            txtMessageInput = new TextBox
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(56, 58, 64),
                ForeColor = Color.White,
                BorderStyle = BorderStyle.None,
                Font = new Font("Segoe UI", 11),
                Multiline = true
            };

            btnSendMessage = new Button
            {
                Text = "Enviar ➤",
                Dock = DockStyle.Right,
                Width = 120,
                BackColor = Color.FromArgb(88, 101, 242),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9, FontStyle.Bold)
            };
            btnSendMessage.FlatAppearance.BorderSize = 0;
            btnSendMessage.Click += BtnSendMessage_Click;

            pnlMessageInputShell.Controls.Add(txtMessageInput);
            pnlInputContainer.Controls.Add(pnlMessageInputShell);
            pnlInputContainer.Controls.Add(btnSendMessage);

            pnlChatContainer.Controls.Add(txtChatMessages);
            pnlChatContainer.Controls.Add(pnlInputContainer);

            // ================= LOG =================
            pnlLog = new Panel
            {
                Dock = DockStyle.Right,
                Width = 310,
                BackColor = Color.FromArgb(43, 45, 49),
                Padding = new Padding(12)
            };

            var lblLogTitle = new Label
            {
                Text = "ACTIVIDAD TÉCNICA",
                ForeColor = Color.FromArgb(148, 155, 164),
                Font = new Font("Segoe UI", 8, FontStyle.Bold),
                Dock = DockStyle.Top,
                Height = 24
            };

            txtTechnicalLog = new TextBox
            {
                Dock = DockStyle.Fill,
                Multiline = true,
                ReadOnly = true,
                BackColor = Color.FromArgb(30, 31, 34),
                ForeColor = Color.FromArgb(185, 187, 190),
                BorderStyle = BorderStyle.None,
                ScrollBars = ScrollBars.Vertical
            };

            pnlLog.Controls.Add(txtTechnicalLog);
            pnlLog.Controls.Add(lblLogTitle);
            // ================= ADD CONTROLS =================
            this.Controls.Add(pnlChatContainer);
            this.Controls.Add(pnlLog);
            this.Controls.Add(pnlSidebar);
            this.Controls.Add(pnlServerRail);
            this.Controls.Add(pnlHeader);

            this.ResumeLayout(false);
        }
    }
}
