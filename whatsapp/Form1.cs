namespace whatsapp
{
    public partial class Form1 : Form
    {
        private P2PNode _p2pNode;
        private bool _isNodeStarted;
        private bool _isDarkMode = true; // Modo oscuro por defecto

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

                if (string.IsNullOrWhiteSpace(txtMessageInput.Text))
                {
                    return;
                }

                if (string.IsNullOrWhiteSpace(txtRemoteIP.Text))
                {
                    MessageBox.Show("Debes ingresar la IP destino", "Validación");
                    return;
                }

                string remoteIP = txtRemoteIP.Text;
                string message = txtMessageInput.Text;
                string timestamp = DateTime.Now.ToString("HH:mm");

                // Mostrar en chat con formato mejorado
                this.Invoke(() =>
                {
                    txtChatMessages.AppendText($"[{timestamp}] Tú:\r\n{message}\r\n\r\n");
                    txtMessageInput.Clear();
                    txtMessageInput.Focus();
                });

                // Enviar en background
                _ = _p2pNode.SendMessage(remoteIP, (int)numRemotePort.Value, message);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al enviar mensaje: {ex.Message}", "Error");
            }
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
