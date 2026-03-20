namespace whatsapp
{
    public partial class Form1 : Form
    {
        private sealed class ChatMessage
        {
            public required string Sender { get; init; }
            public required string Text { get; init; }
            public required string Timestamp { get; init; }
            public required bool IsOwnMessage { get; init; }
        }

        private sealed class ChatInfo
        {
            public required string Name { get; init; }
            public required string RemoteIP { get; init; }
            public required int RemotePort { get; init; }
            public List<ChatMessage> Messages { get; } = [];

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
        private readonly Color DarkBg = Color.FromArgb(49, 51, 56);
        private readonly Color DarkTextBg = Color.FromArgb(56, 58, 64);
        private readonly Color DarkText = Color.White;
        private readonly Color DarkBorder = Color.FromArgb(88, 101, 242);

        // Colores Modo Claro
        private readonly Color LightBg = Color.FromArgb(245, 246, 250);
        private readonly Color LightTextBg = Color.White;
        private readonly Color LightText = Color.FromArgb(32, 34, 37);
        private readonly Color LightBorder = Color.FromArgb(200, 200, 200);

        public Form1()
        {
            InitializeComponent();
            _isNodeStarted = false;
            ApplyDarkTheme(); // Aplicar tema oscuro al iniciar
            ApplyRoundedStyles();
            Load += (_, _) => ApplyRoundedStyles();
            Shown += (_, _) => ApplyRoundedStyles();
            Resize += (_, _) => ApplyRoundedStyles();
        }

        private void NumLocalPort_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Up)
            {
                if (numLocalPort.Value < numLocalPort.Maximum)
                {
                    numLocalPort.Value++;
                }
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.Down)
            {
                if (numLocalPort.Value > numLocalPort.Minimum)
                {
                    numLocalPort.Value--;
                }
                e.Handled = true;
            }
        }

        private void PnlChatMessages_Resize(object? sender, EventArgs e)
        {
            foreach (Control c in pnlChatMessages.Controls)
            {
                c.Width = pnlChatMessages.ClientSize.Width - 40;
            }
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
                    var chatMsg = new ChatMessage 
                    { 
                        Sender = $"Tú ({_activeChat.Name})", 
                        Text = message, 
                        Timestamp = timestamp, 
                        IsOwnMessage = true 
                    };
                    _activeChat.Messages.Add(chatMsg);
                    AppendMessageBubble(chatMsg.Sender, chatMsg.Text, chatMsg.Timestamp, chatMsg.IsOwnMessage);
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

            pnlChatMessages.SuspendLayout();
            pnlChatMessages.Controls.Clear();

            if (_activeChat != null)
            {
                foreach (var msg in _activeChat.Messages)
                {
                    AppendMessageBubble(msg.Sender, msg.Text, msg.Timestamp, msg.IsOwnMessage);
                }
            }

            pnlChatMessages.ResumeLayout(true);
            pnlChatMessages.ScrollControlIntoView(pnlChatMessages.Controls.Count > 0 ? pnlChatMessages.Controls[pnlChatMessages.Controls.Count - 1] : null);
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
                var chatMsg = new ChatMessage 
                { 
                    Sender = "Contacto", 
                    Text = message, 
                    Timestamp = timestamp, 
                    IsOwnMessage = false 
                };

                ChatInfo targetChat = _activeChat;

                // Si no hay chat activo, intenta agregar al primero de la lista
                if (targetChat == null && _chats.Count > 0)
                    targetChat = _chats[0];

                if (targetChat != null)
                {
                    targetChat.Messages.Add(chatMsg);

                    // Solo renderizar si el chat al que llegó es el que se está mostrando
                    if (_activeChat == targetChat)
                    {
                        AppendMessageBubble(chatMsg.Sender, chatMsg.Text, chatMsg.Timestamp, chatMsg.IsOwnMessage);
                    }
                }
            });
        }

        private void AppendMessageBubble(string sender, string message, string timestamp, bool isOwnMessage)
        {
            var bubbleBgColor = isOwnMessage
                ? Color.FromArgb(88, 101, 242)      // Azul moderno para propios
                : Color.FromArgb(64, 68, 75);       // Gris oscuro para contacto

            var foreColor = Color.White;
            var headerColor = Color.FromArgb(185, 187, 190);

            if (!_isDarkMode)
            {
                bubbleBgColor = isOwnMessage ? Color.FromArgb(88, 101, 242) : Color.FromArgb(220, 224, 230);
                foreColor = isOwnMessage ? Color.White : Color.FromArgb(32, 34, 37);
                headerColor = Color.Gray;
            }

            Panel msgWrapper = new Panel
            {
                Width = pnlChatMessages.ClientSize.Width - 40,
                AutoSize = false,
                Margin = new Padding(0, 0, 0, 15)
            };

            Label lblHeader = new Label
            {
                Text = $"{sender} • {timestamp}",
                AutoSize = true,
                ForeColor = headerColor,
                Font = new Font("Segoe UI", 8.5f, FontStyle.Regular),
                Top = 0
            };

            Label lblMessage = new Label
            {
                Text = message,
                AutoSize = true,
                MaximumSize = new Size((int)(msgWrapper.Width * 0.7), 0),
                BackColor = bubbleBgColor,
                ForeColor = foreColor,
                Font = new Font("Segoe UI", 10.5f, FontStyle.Regular),
                Padding = new Padding(12, 10, 12, 10)
            };

            lblMessage.Top = 20; // below header

            Action alignControls = () =>
            {
                lblMessage.MaximumSize = new Size((int)(msgWrapper.Width * 0.7), 0);
                if (isOwnMessage)
                {
                    lblHeader.Left = msgWrapper.Width - lblHeader.PreferredWidth;
                    lblMessage.Left = msgWrapper.Width - lblMessage.PreferredWidth;
                }
                else
                {
                    lblHeader.Left = 0;
                    lblMessage.Left = 0;
                }
                msgWrapper.Height = lblMessage.Bottom + 5;
            };

            lblMessage.SizeChanged += (s, e) => 
            {
                alignControls();
                RoundControl(lblMessage, 12);
            };

            msgWrapper.Resize += (s, e) => alignControls();

            msgWrapper.Controls.Add(lblHeader);
            msgWrapper.Controls.Add(lblMessage);

            alignControls(); // Forzar alineación inicial antes de agregar al contenedor

            pnlChatMessages.Controls.Add(msgWrapper);
            pnlChatMessages.ScrollControlIntoView(msgWrapper);
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
            BackColor = DarkBg;
            btnToggleTheme.Text = "☀️";
            btnToggleTheme.BackColor = Color.FromArgb(43, 45, 49);

            foreach (Control ctrl in Controls)
            {
                ApplyDarkThemeRecursive(ctrl);
            }

            btnSendMessage.BackColor = DarkBorder;
            btnNewChat.BackColor = Color.FromArgb(64, 68, 75);
            btnStartListener.BackColor = Color.FromArgb(35, 165, 90);
            btnStopListener.BackColor = Color.FromArgb(237, 66, 69);
        }

        private void ApplyLightTheme()
        {
            BackColor = LightBg;
            btnToggleTheme.Text = "🌙";
            btnToggleTheme.BackColor = Color.FromArgb(230, 232, 236);

            foreach (Control ctrl in Controls)
            {
                ApplyLightThemeRecursive(ctrl);
            }

            btnSendMessage.BackColor = Color.FromArgb(88, 101, 242);
            btnNewChat.BackColor = Color.FromArgb(220, 224, 230);
            btnStartListener.BackColor = Color.FromArgb(35, 165, 90);
            btnStopListener.BackColor = Color.FromArgb(237, 66, 69);
        }

        private void ApplyDarkThemeRecursive(Control ctrl)
        {
            if (ctrl == pnlServerRail)
            {
                ctrl.BackColor = Color.FromArgb(30, 31, 34);
            }
            else if (ctrl == pnlSidebar || ctrl == pnlLog)
            {
                ctrl.BackColor = Color.FromArgb(43, 45, 49);
            }
            else if (ctrl == pnlHeader || ctrl == pnlChatContainer || ctrl == pnlInputContainer || ctrl == pnlChatMessages)
            {
                ctrl.BackColor = Color.FromArgb(49, 51, 56);
            }
            else if (ctrl == pnlMessageInputShell)
            {
                ctrl.BackColor = DarkTextBg;
            }
            if (ctrl is TextBox textBox && textBox != txtMessageInput)
            {
                textBox.BackColor = textBox == txtTechnicalLog ? Color.FromArgb(30, 31, 34) : Color.FromArgb(49, 51, 56);
                textBox.ForeColor = DarkText;
            }
            else if (ctrl is TextBox messageBox && messageBox == txtMessageInput)
            {
                messageBox.BackColor = DarkTextBg;
                messageBox.ForeColor = DarkText;
            }
            else if (ctrl is Label label && label != lblConnectionStatus)
            {
                label.BackColor = Color.Transparent;
                label.ForeColor = Color.FromArgb(185, 187, 190);
            }
            else if (ctrl is NumericUpDown numericUpDown)
            {
                numericUpDown.BackColor = Color.FromArgb(56, 58, 64);
                numericUpDown.ForeColor = DarkText;
            }
            else if (ctrl is ListBox listBox)
            {
                listBox.BackColor = Color.FromArgb(43, 45, 49);
                listBox.ForeColor = DarkText;
            }
            else if (ctrl is RichTextBox richTextBox)
            {
                richTextBox.BackColor = Color.FromArgb(49, 51, 56);
                richTextBox.ForeColor = DarkText;
            }
            else if (ctrl is Panel panel)
            {
                panel.BackColor = panel.BackColor;
            }

            // Aplicar recursivamente a controles hijos
            foreach (Control child in ctrl.Controls)
            {
                ApplyDarkThemeRecursive(child);
            }
        }

        private void ApplyLightThemeRecursive(Control ctrl)
        {
            if (ctrl == pnlServerRail)
            {
                ctrl.BackColor = Color.FromArgb(231, 234, 238);
            }
            else if (ctrl == pnlSidebar || ctrl == pnlLog)
            {
                ctrl.BackColor = Color.FromArgb(236, 239, 244);
            }
            else if (ctrl == pnlHeader || ctrl == pnlChatContainer || ctrl == pnlInputContainer || ctrl == pnlChatMessages)
            {
                ctrl.BackColor = LightBg;
            }
            else if (ctrl == pnlMessageInputShell)
            {
                ctrl.BackColor = Color.FromArgb(229, 233, 240);
            }
            if (ctrl is TextBox textBox && textBox != txtMessageInput)
            {
                textBox.BackColor = textBox == txtTechnicalLog ? Color.FromArgb(229, 233, 240) : LightTextBg;
                textBox.ForeColor = LightText;
            }
            else if (ctrl is TextBox messageBox && messageBox == txtMessageInput)
            {
                messageBox.BackColor = Color.FromArgb(229, 233, 240);
                messageBox.ForeColor = LightText;
            }
            else if (ctrl is Label label && label != lblConnectionStatus)
            {
                label.BackColor = Color.Transparent;
                label.ForeColor = LightText;
            }
            else if (ctrl is NumericUpDown numericUpDown)
            {
                numericUpDown.BackColor = Color.FromArgb(229, 233, 240);
                numericUpDown.ForeColor = LightText;
            }
            else if (ctrl is ListBox listBox)
            {
                listBox.BackColor = Color.FromArgb(236, 239, 244);
                listBox.ForeColor = LightText;
            }
            else if (ctrl is RichTextBox richTextBox)
            {
                richTextBox.BackColor = LightTextBg;
                richTextBox.ForeColor = LightText;
            }
            else if (ctrl is Panel panel)
            {
                panel.BackColor = panel.BackColor;
            }

            // Aplicar recursivamente a controles hijos
            foreach (Control child in ctrl.Controls)
            {
                ApplyLightThemeRecursive(child);
            }
        }

        private void ApplyRoundedStyles()
        {
            RoundControl(btnSendMessage, 12);
            RoundControl(btnNewChat, 10);
            RoundControl(btnStartListener, 10);
            RoundControl(btnStopListener, 10);
            RoundControl(btnToggleTheme, 10);
            RoundControl(txtEncryptionKey, 8);
            RoundControl(numLocalPort, 8);
            RoundControl(pnlInputContainer, 14);
            RoundControl(pnlChatMessages, 10);
            RoundControl(lstChats, 10);
        }

        private static void RoundControl(Control control, int radius)
        {
            if (control.Width <= 0 || control.Height <= 0)
            {
                return;
            }

            using var path = new System.Drawing.Drawing2D.GraphicsPath();
            int diameter = radius * 2;

            path.AddArc(0, 0, diameter, diameter, 180, 90);
            path.AddArc(control.Width - diameter, 0, diameter, diameter, 270, 90);
            path.AddArc(control.Width - diameter, control.Height - diameter, diameter, diameter, 0, 90);
            path.AddArc(0, control.Height - diameter, diameter, diameter, 90, 90);
            path.CloseFigure();

            control.Region = new Region(path);
        }
    }
}
