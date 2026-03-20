namespace whatsapp
{
    public sealed class WelcomeForm : Form
    {
        public WelcomeForm()
        {
            Text = "FBIchat";
            StartPosition = FormStartPosition.CenterScreen;
            ClientSize = new Size(920, 560);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            MinimizeBox = false;
            BackColor = Color.FromArgb(49, 51, 56);

            var backgroundPictureBox = new PictureBox
            {
                Dock = DockStyle.Fill,
                Image = Properties.Resources.abstract_smooth_speed_wave_lines_isolated_for_banner_template_background_png,
                SizeMode = PictureBoxSizeMode.StretchImage
            };

            var mainPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(40),
                BackColor = Color.Transparent
            };

            var card = new Panel
            {
                Size = new Size(760, 420),
                BackColor = Color.FromArgb(56, 58, 64)
            };

            var iconLabel = new Label
            {
                Text = "💬",
                AutoSize = false,
                Dock = DockStyle.Top,
                Height = 95,
                TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = Color.White,
                Font = new Font("Segoe UI Emoji", 36, FontStyle.Regular)
            };

            var titleLabel = new Label
            {
                Text = "FBIchat • Mensajería instantánea",
                AutoSize = false,
                Dock = DockStyle.Top,
                Height = 70,
                TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 24, FontStyle.Bold)
            };

            var subtitleLabel = new Label
            {
                Text = "Habla en tiempo real con una conexión directa y privada.",
                AutoSize = false,
                Dock = DockStyle.Top,
                Height = 55,
                TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = Color.FromArgb(185, 187, 190),
                Font = new Font("Segoe UI", 11, FontStyle.Regular)
            };

            var usernameLabel = new Label
            {
                Text = "Nombre de usuario",
                AutoSize = false,
                Dock = DockStyle.Top,
                Height = 28,
                TextAlign = ContentAlignment.BottomCenter,
                ForeColor = Color.FromArgb(200, 205, 212),
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };

            var usernameTextBox = new TextBox
            {
                Width = 320,
                Height = 34,
                Font = new Font("Segoe UI", 11, FontStyle.Regular),
                BackColor = Color.FromArgb(67, 70, 78),
                ForeColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                TextAlign = HorizontalAlignment.Center,
                Text = Environment.UserName
            };

            var chatButton = new Button
            {
                Text = "Entrar al chat",
                Width = 220,
                Height = 56,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(88, 101, 242),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                Anchor = AnchorStyles.None
            };
            chatButton.FlatAppearance.BorderSize = 0;
            chatButton.Click += (_, _) => ChatButton_Click(usernameTextBox.Text);

            var buttonPanel = new Panel
            {
                Dock = DockStyle.Fill
            };
            buttonPanel.Controls.Add(usernameTextBox);
            buttonPanel.Controls.Add(chatButton);
            buttonPanel.Resize += (_, _) =>
            {
                usernameTextBox.Left = (buttonPanel.ClientSize.Width - usernameTextBox.Width) / 2;
                usernameTextBox.Top = Math.Max((buttonPanel.ClientSize.Height - chatButton.Height) / 2 - 56, 10);
                chatButton.Left = (buttonPanel.ClientSize.Width - chatButton.Width) / 2;
                chatButton.Top = usernameTextBox.Bottom + 14;
            };

            var footerLabel = new Label
            {
                Text = "Crea un chat, conecta por IP y empieza a conversar.",
                AutoSize = false,
                Dock = DockStyle.Bottom,
                Height = 48,
                TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = Color.FromArgb(148, 155, 164),
                Font = new Font("Segoe UI", 9, FontStyle.Italic)
            };

            card.Controls.Add(buttonPanel);
            card.Controls.Add(footerLabel);
            card.Controls.Add(usernameLabel);
            card.Controls.Add(subtitleLabel);
            card.Controls.Add(titleLabel);
            card.Controls.Add(iconLabel);

            mainPanel.Controls.Add(card);
            mainPanel.Resize += (_, _) =>
            {
                card.Left = (mainPanel.ClientSize.Width - card.Width) / 2;
                card.Top = (mainPanel.ClientSize.Height - card.Height) / 2;
            };

            backgroundPictureBox.Controls.Add(mainPanel);
            Controls.Add(backgroundPictureBox);
        }

        private void ChatButton_Click(string username)
        {
            string finalUsername = string.IsNullOrWhiteSpace(username) ? "Usuario" : username.Trim();
            Hide();
            using var chatForm = new Form1(finalUsername);
            chatForm.ShowDialog(this);
            Close();
        }

    }
}
