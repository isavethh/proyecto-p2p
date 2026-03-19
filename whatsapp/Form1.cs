namespace whatsapp
{
    public partial class Form1 : Form
    {
        private sealed class ChatInfo
        {
            public required string Name { get; init; }
            public required string RemoteIP { get; init; }
            public required int RemotePort { get; init; }

            public override string ToString()
            {
                return $"{Name} ({RemoteIP}:{RemotePort})";
            }
        }

        private P2PNode _p2pNode;
        private bool _isNodeStarted;
        private bool _isDarkMode = true; // Modo oscuro por defecto
        private readonly List<ChatInfo> _chats = [];
        private ChatInfo? _activeChat;

        // Colores Modo Oscuro
        private readonly Color DarkBg = Color.FromArgb(17, 27, 33);
        private readonly Color DarkTextBg = Color.FromArgb(30, 40, 50);
        private readonly Color DarkText = Color.White;
        private readonly Color DarkBorder = Color.FromArgb(100, 150, 200);

        // Colores Modo Claro
        private readonly Color LightBg = Color.FromArgb(240, 240, 245);
        private readonly Color LightTextBg = Color.White;
        private readonly Color LightText = Color.Black;
        private readonly Color LightBorder = Color.FromArgb(200, 200, 200);

        public Form1()
        {
            InitializeComponent();
            _isNodeStarted = false;
            ApplyDarkTheme(); // Aplicar tema oscuro al iniciar
        }

        private void BtnStartListener_Click(object sender, EventArgs e)
        {
            try
            {
                if (numLocalPort.Value < 1024 || numLocalPort.Value > 65535)
                {
                    MessageBox.Show("El puerto debe estar entre 1024 y 65535", "Error de validación");
                    return;
                }

                if (string.IsNullOrWhiteSpace(txtEncryptionKey.Text))
                {
                    MessageBox.Show("La clave de encriptación no puede estar vacía", "Error de validación");
                    return;
                }

                int localPort = (int)numLocalPort.Value;
                string encryptionKey = txtEncryptionKey.Text;

                _p2pNode = new P2PNode(localPort, encryptionKey);

                // Suscribirse a eventos
                _p2pNode.MessageReceived += P2PNode_MessageReceived;
                _p2pNode.LogMessage += P2PNode_LogMessage;
                _p2pNode.ConnectionStateChanged += P2PNode_ConnectionStateChanged;

                _p2pNode.StartListening();

                _isNodeStarted = true;
                btnStartListener.Enabled = false;
                btnStopListener.Enabled = true;
                txtEncryptionKey.Enabled = false;
                numLocalPort.Enabled = false;

                lblConnectionStatus.Text = "● Conectando...";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al iniciar listener: {ex.Message}", "Error");
            }
        }

        private void BtnStopListener_Click(object sender, EventArgs e)
        {
            try
            {
                _p2pNode?.StopListening();
                _p2pNode?.Dispose();
                _isNodeStarted = false;
                btnStartListener.Enabled = true;
                btnStopListener.Enabled = false;
                numLocalPort.Enabled = true;
                txtEncryptionKey.Enabled = true;
                lblConnectionStatus.Text = "● Desconectado";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al detener listener: {ex.Message}", "Error");
            }
        }

        private void BtnSendMessage_Click(object sender, EventArgs e)
        {
            try
            {
                if (!_isNodeStarted)
                {
                    MessageBox.Show("Debes conectar primero", "Advertencia");
                    return;
                }

                if (_activeChat is null)
                {
                    MessageBox.Show("Debes crear y seleccionar un chat", "Validación");
                    return;
                }

                if (string.IsNullOrWhiteSpace(txtMessageInput.Text))
                {
                    return;
                }

                string message = txtMessageInput.Text;
                string timestamp = DateTime.Now.ToString("HH:mm");

                // Mostrar en chat con formato mejorado
                this.Invoke(() =>
                {
                    txtChatMessages.AppendText($"[{timestamp}] Tú ({_activeChat.Name}):\r\n{message}\r\n\r\n");
                    txtMessageInput.Clear();
                    txtMessageInput.Focus();
                });

                // Enviar en background
                _ = _p2pNode.SendMessage(_activeChat.RemoteIP, _activeChat.RemotePort, message);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al enviar mensaje: {ex.Message}", "Error");
            }
        }

        private void BtnNewChat_Click(object sender, EventArgs e)
        {
            if (!TryShowNewChatDialog(out string chatName, out string remoteIp, out int remotePort))
            {
                return;
            }

            var chat = new ChatInfo
            {
                Name = chatName,
                RemoteIP = remoteIp,
                RemotePort = remotePort
            };

            _chats.Add(chat);
            lstChats.Items.Add(chat);
            lstChats.SelectedItem = chat;
        }

        private void LstChats_SelectedIndexChanged(object sender, EventArgs e)
        {
            _activeChat = lstChats.SelectedItem as ChatInfo;
        }

        private bool TryShowNewChatDialog(out string chatName, out string remoteIp, out int remotePort)
        {
            chatName = string.Empty;
            remoteIp = string.Empty;
            remotePort = 0;

            using Form dialog = new()
            {
                Text = "Crear nuevo chat",
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false,
                MinimizeBox = false,
                ClientSize = new Size(320, 210)
            };

            Label lblName = new() { Left = 15, Top = 15, Width = 290, Text = "Nombre del chat:" };
            TextBox txtName = new() { Left = 15, Top = 35, Width = 290 };

            Label lblIp = new() { Left = 15, Top = 70, Width = 290, Text = "IP destino:" };
            TextBox txtIp = new() { Left = 15, Top = 90, Width = 290, Text = "127.0.0.1" };

            Label lblPort = new() { Left = 15, Top = 125, Width = 290, Text = "Puerto destino:" };
            NumericUpDown numPort = new()
            {
                Left = 15,
                Top = 145,
                Width = 290,
                Minimum = 1024,
                Maximum = 65535,
                Value = 5001
            };

            Button btnOk = new() { Text = "Crear", Left = 145, Top = 178, Width = 75, DialogResult = DialogResult.OK };
            Button btnCancel = new() { Text = "Cancelar", Left = 230, Top = 178, Width = 75, DialogResult = DialogResult.Cancel };

            dialog.Controls.Add(lblName);
            dialog.Controls.Add(txtName);
            dialog.Controls.Add(lblIp);
            dialog.Controls.Add(txtIp);
            dialog.Controls.Add(lblPort);
            dialog.Controls.Add(numPort);
            dialog.Controls.Add(btnOk);
            dialog.Controls.Add(btnCancel);
            dialog.AcceptButton = btnOk;
            dialog.CancelButton = btnCancel;

            if (dialog.ShowDialog(this) != DialogResult.OK)
            {
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                MessageBox.Show("Debes ingresar un nombre para el chat", "Validación");
                return false;
            }

            if (!System.Net.IPAddress.TryParse(txtIp.Text.Trim(), out _))
            {
                MessageBox.Show("La IP destino no es válida", "Validación");
                return false;
            }

            chatName = txtName.Text.Trim();
            remoteIp = txtIp.Text.Trim();
            remotePort = (int)numPort.Value;
            return true;
        }

        private void P2PNode_MessageReceived(string message, byte[] encryptedBytes)
        {
            this.Invoke(() =>
            {
                string timestamp = DateTime.Now.ToString("HH:mm");
                txtChatMessages.AppendText($"[{timestamp}] Contacto:\r\n{message}\r\n\r\n");
            });
        }

        private void P2PNode_LogMessage(string logMessage)
        {
            this.Invoke(() =>
            {
                txtTechnicalLog.AppendText($"{DateTime.Now:HH:mm:ss} - {logMessage}\r\n");
                // Limitar el log a 10000 líneas para evitar uso excesivo de memoria
                if (txtTechnicalLog.Lines.Length > 10000)
                {
                    string[] lines = txtTechnicalLog.Lines;
                    txtTechnicalLog.Clear();
                    for (int i = 5000; i < lines.Length; i++)
                    {
                        txtTechnicalLog.AppendText(lines[i] + "\r\n");
                    }
                }
            });
        }

        private void P2PNode_ConnectionStateChanged(bool isConnected)
        {
            this.Invoke(() =>
            {
                lblConnectionStatus.Text = isConnected ? "● Conectado" : "● Desconectado";
            });
        }

        private void BtnToggleTheme_Click(object sender, EventArgs e)
        {
            _isDarkMode = !_isDarkMode;

            if (_isDarkMode)
                ApplyDarkTheme();
            else
                ApplyLightTheme();
        }

        private void ApplyDarkTheme()
        {
            // Form
            BackColor = DarkBg;

            // Botón toggle
            btnToggleTheme.Text = "☀️";
            btnToggleTheme.BackColor = Color.FromArgb(37, 47, 53);

            // Paneles principales
            foreach (Control ctrl in Controls)
            {
                ApplyDarkThemeRecursive(ctrl);
            }
        }

        private void ApplyLightTheme()
        {
            // Form
            BackColor = LightBg;

            // Botón toggle
            btnToggleTheme.Text = "🌙";
            btnToggleTheme.BackColor = Color.FromArgb(220, 220, 220);

            // Paneles principales
            foreach (Control ctrl in Controls)
            {
                ApplyLightThemeRecursive(ctrl);
            }
        }

        private void ApplyDarkThemeRecursive(Control ctrl)
        {
            if (ctrl is TextBox textBox && textBox != txtMessageInput)
            {
                textBox.BackColor = DarkTextBg;
                textBox.ForeColor = DarkText;
            }
            else if (ctrl is TextBox messageBox && messageBox == txtMessageInput)
            {
                messageBox.BackColor = Color.FromArgb(37, 47, 53);
                messageBox.ForeColor = DarkText;
            }
            else if (ctrl is Label label && label != lblConnectionStatus)
            {
                label.BackColor = Color.Transparent;
                label.ForeColor = DarkText;
            }
            else if (ctrl is NumericUpDown numericUpDown)
            {
                numericUpDown.BackColor = Color.FromArgb(37, 47, 53);
                numericUpDown.ForeColor = DarkText;
            }
            else if (ctrl is ListBox listBox)
            {
                listBox.BackColor = Color.FromArgb(37, 47, 53);
                listBox.ForeColor = DarkText;
            }
            else if (ctrl is Panel panel)
            {
                panel.BackColor = Color.FromArgb(17, 27, 33);
            }

            // Aplicar recursivamente a controles hijos
            foreach (Control child in ctrl.Controls)
            {
                ApplyDarkThemeRecursive(child);
            }
        }

        private void ApplyLightThemeRecursive(Control ctrl)
        {
            if (ctrl is TextBox textBox && textBox != txtMessageInput)
            {
                textBox.BackColor = LightTextBg;
                textBox.ForeColor = LightText;
            }
            else if (ctrl is TextBox messageBox && messageBox == txtMessageInput)
            {
                messageBox.BackColor = Color.FromArgb(240, 240, 245);
                messageBox.ForeColor = LightText;
            }
            else if (ctrl is Label label && label != lblConnectionStatus)
            {
                label.BackColor = Color.Transparent;
                label.ForeColor = LightText;
            }
            else if (ctrl is NumericUpDown numericUpDown)
            {
                numericUpDown.BackColor = LightTextBg;
                numericUpDown.ForeColor = LightText;
            }
            else if (ctrl is ListBox listBox)
            {
                listBox.BackColor = LightTextBg;
                listBox.ForeColor = LightText;
            }
            else if (ctrl is Panel panel)
            {
                panel.BackColor = LightBg;
            }

            // Aplicar recursivamente a controles hijos
            foreach (Control child in ctrl.Controls)
            {
                ApplyLightThemeRecursive(child);
            }
        }
    }
}
