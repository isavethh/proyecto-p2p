namespace whatsapp
{
    public partial class Form1 : Form
    {
        private sealed class WireMessage
        {
            public string? Sender { get; init; }
            public string? Text { get; init; }
        }

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

        private void TxtChatMessages_Resize(object? sender, EventArgs e)
        {
            foreach (Control control in txtChatMessages.Controls)
            {
                control.Width = Math.Max(txtChatMessages.ClientSize.Width - 32, 100);
            }
        }

        private P2PNode _p2pNode;
        private bool _isNodeStarted;
        private bool _isDarkMode = true; // Modo oscuro por defecto
        private readonly List<ChatInfo> _chats = [];
        private ChatInfo? _activeChat;
        private readonly string _localUsername;

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

        public Form1() : this("Tú")
        {
        }

        public Form1(string username)
        {
            InitializeComponent();
            _isNodeStarted = false;
            _localUsername = string.IsNullOrWhiteSpace(username) ? "Tú" : username.Trim();
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
                    AppendMessageBubble($"{_localUsername} ({_activeChat.Name})", message, timestamp, true);
                    txtMessageInput.Clear();
                    txtMessageInput.Focus();
                });

                // Enviar en background
                string wireMessage = SerializeWireMessage(_localUsername, message);
                _ = _p2pNode.SendMessage(_activeChat.RemoteIP, _activeChat.RemotePort, wireMessage);
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
                ParseWireMessage(message, out string senderName, out string messageText);
                AppendMessageBubble(senderName, messageText, timestamp, false);
            });
        }

        private static string SerializeWireMessage(string sender, string text)
        {
            var payload = new WireMessage { Sender = sender, Text = text };
            return System.Text.Json.JsonSerializer.Serialize(payload);
        }

        private static void ParseWireMessage(string rawMessage, out string sender, out string text)
        {
            sender = "Contacto";
            text = rawMessage;

            try
            {
                WireMessage? parsed = System.Text.Json.JsonSerializer.Deserialize<WireMessage>(rawMessage);
                if (!string.IsNullOrWhiteSpace(parsed?.Text))
                {
                    text = parsed.Text;
                    sender = string.IsNullOrWhiteSpace(parsed.Sender) ? "Contacto" : parsed.Sender;
                }
            }
            catch
            {
                // Compatibilidad con nodos antiguos que envían solo texto plano.
            }
        }

        private void AppendMessageBubble(string sender, string message, string timestamp, bool isOwnMessage)
        {
            var bubbleBgColor = isOwnMessage
                ? Color.FromArgb(88, 101, 242)
                : (_isDarkMode ? Color.FromArgb(64, 68, 75) : Color.FromArgb(220, 224, 230));

            var bubbleTextColor = isOwnMessage
                ? Color.White
                : (_isDarkMode ? Color.White : Color.FromArgb(32, 34, 37));

            var headerColor = _isDarkMode
                ? Color.FromArgb(185, 187, 190)
                : Color.FromArgb(110, 115, 125);

            Panel wrapper = new()
            {
                Width = Math.Max(txtChatMessages.ClientSize.Width - 32, 100),
                Height = 1,
                Margin = new Padding(0, 0, 0, 12)
            };

            Label headerLabel = new()
            {
                Text = $"{sender} • {timestamp}",
                AutoSize = true,
                ForeColor = headerColor,
                BackColor = Color.Transparent,
                Font = new Font("Segoe UI", 8.5f, FontStyle.Regular),
                Top = 0
            };

            Label bubbleLabel = new()
            {
                Text = message,
                AutoSize = true,
                MaximumSize = new Size((int)(wrapper.Width * 0.72f), 0),
                MinimumSize = new Size(90, 0),
                BackColor = bubbleBgColor,
                ForeColor = bubbleTextColor,
                Font = new Font("Segoe UI", 10f, FontStyle.Regular),
                Padding = new Padding(12, 9, 12, 9),
                Top = 22
            };

            void AlignBubble()
            {
                bubbleLabel.MaximumSize = new Size((int)(wrapper.Width * 0.72f), 0);

                if (isOwnMessage)
                {
                    headerLabel.Left = wrapper.Width - headerLabel.PreferredWidth;
                    bubbleLabel.Left = wrapper.Width - bubbleLabel.Width;
                }
                else
                {
                    headerLabel.Left = 0;
                    bubbleLabel.Left = 0;
                }

                wrapper.Height = bubbleLabel.Bottom + 2;
                RoundControl(bubbleLabel, 14);
            }

            bubbleLabel.SizeChanged += (_, _) => AlignBubble();
            wrapper.Resize += (_, _) => AlignBubble();

            wrapper.Controls.Add(headerLabel);
            wrapper.Controls.Add(bubbleLabel);
            AlignBubble();

            txtChatMessages.Controls.Add(wrapper);
            txtChatMessages.ScrollControlIntoView(wrapper);
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
            else if (ctrl == pnlHeader || ctrl == pnlChatContainer || ctrl == pnlInputContainer)
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
            else if (ctrl is FlowLayoutPanel flowPanel && flowPanel == txtChatMessages)
            {
                flowPanel.BackColor = Color.FromArgb(49, 51, 56);
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
            else if (ctrl == pnlHeader || ctrl == pnlChatContainer || ctrl == pnlInputContainer)
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
            else if (ctrl is FlowLayoutPanel flowPanel && flowPanel == txtChatMessages)
            {
                flowPanel.BackColor = LightBg;
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
            RoundControl(txtChatMessages, 10);
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
