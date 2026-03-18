using System.Net;
using System.Net.Sockets;
using System.Text;

namespace whatsapp
{
    /// <summary>
    /// Nodo P2P que maneja conexiones de escucha y envío de mensajes
    /// </summary>
    public class P2PNode : IDisposable
    {
        private TcpListener _listener;
        private EncryptionService _encryptionService;
        private int _listeningPort;
        private bool _isListening;
        private TcpClient _incomingClient;
        private NetworkStream _incomingStream;
        private byte[] _receiveBuffer;
        private const int BufferSize = 4096;

        // Eventos para comunicación con la UI
        public event Action<string, byte[]> MessageReceived;
        public event Action<string> LogMessage;
        public event Action<bool> ConnectionStateChanged;

        public int ListeningPort => _listeningPort;
        public bool IsListening => _isListening;

        public P2PNode(int listeningPort, string sharedEncryptionKey)
        {
            _listeningPort = listeningPort;
            _encryptionService = new EncryptionService(sharedEncryptionKey);
            _receiveBuffer = new byte[BufferSize * 10];
        }

        /// <summary>
        /// Inicia el listener en un hilo separado
        /// </summary>
        public void StartListening()
        {
            if (_isListening)
                return;

            try
            {
                _listener = new TcpListener(IPAddress.Any, _listeningPort);
                _listener.Start();
                _isListening = true;
                LogMessage?.Invoke($"[LOG] Escuchando en puerto {_listeningPort}");

                // Iniciar hilo de escucha
                Task.Run(() => AcceptIncomingConnections());
            }
            catch (Exception ex)
            {
                LogMessage?.Invoke($"[ERROR] No se pudo iniciar listener: {ex.Message}");
            }
        }

        /// <summary>
        /// Acepta conexiones entrantes
        /// </summary>
        private async Task AcceptIncomingConnections()
        {
            try
            {
                while (_isListening)
                {
                    _incomingClient = await _listener.AcceptTcpClientAsync();
                    LogMessage?.Invoke($"[LOG] Nueva conexión desde {_incomingClient.Client.RemoteEndPoint}");
                    
                    _incomingStream = _incomingClient.GetStream();
                    ConnectionStateChanged?.Invoke(true);
                    
                    // Iniciar lectura de mensajes
                    _ = ReceiveMessages();
                }
            }
            catch (ObjectDisposedException)
            {
                // Esperado cuando se detiene el listener
            }
            catch (Exception ex)
            {
                LogMessage?.Invoke($"[ERROR] Error aceptando conexión: {ex.Message}");
            }
        }

        /// <summary>
        /// Lee mensajes entrantes de forma continua
        /// </summary>
        private async Task ReceiveMessages()
        {
            try
            {
                int totalBytesRead = 0;

                while (_incomingStream != null && _incomingClient.Connected)
                {
                    int bytesRead = await _incomingStream.ReadAsync(_receiveBuffer, totalBytesRead, BufferSize);

                    if (bytesRead == 0)
                    {
                        LogMessage?.Invoke("[LOG] Conexión cerrada por el remoto");
                        break;
                    }

                    totalBytesRead += bytesRead;

                    // Intentar desempaquetar mensaje
                    while (MessageProtocol.TryUnpackMessage(_receiveBuffer, totalBytesRead, out byte[] encryptedData, out int bytesConsumed))
                    {
                        try
                        {
                            // Mostrar bytes cifrados en log
                            string hexCifrado = BitConverter.ToString(encryptedData).Replace("-", "");
                            LogMessage?.Invoke($"[CIFRADO RX] {hexCifrado}");

                            // Descifrar mensaje
                            string decryptedMessage = _encryptionService.Decrypt(encryptedData);
                            
                            // Notificar a la UI
                            MessageReceived?.Invoke(decryptedMessage, encryptedData);
                            LogMessage?.Invoke($"[DESCIFRADO] {decryptedMessage}");
                        }
                        catch (Exception ex)
                        {
                            LogMessage?.Invoke($"[ERROR] Falló desencriptación: {ex.Message}");
                        }

                        // Mover buffer restante al inicio
                        Array.Copy(_receiveBuffer, bytesConsumed, _receiveBuffer, 0, totalBytesRead - bytesConsumed);
                        totalBytesRead -= bytesConsumed;
                    }
                }
            }
            catch (Exception ex)
            {
                LogMessage?.Invoke($"[ERROR] Error recibiendo mensajes: {ex.Message}");
            }
            finally
            {
                DisconnectIncoming();
            }
        }

        /// <summary>
        /// Envía un mensaje a una dirección remota
        /// </summary>
        public async Task SendMessage(string remoteIP, int remotePort, string message)
        {
            TcpClient client = null;
            try
            {
                client = new TcpClient();
                await client.ConnectAsync(remoteIP, remotePort);
                
                LogMessage?.Invoke($"[LOG] Conectado a {remoteIP}:{remotePort}");

                // Cifrar mensaje
                byte[] encryptedData = _encryptionService.Encrypt(message);
                
                // Mostrar bytes cifrados en log
                string hexCifrado = BitConverter.ToString(encryptedData).Replace("-", "");
                LogMessage?.Invoke($"[CIFRADO TX] {hexCifrado}");

                // Empaquetar y enviar
                byte[] packedMessage = MessageProtocol.PackMessage(encryptedData);
                
                using (var stream = client.GetStream())
                {
                    await stream.WriteAsync(packedMessage, 0, packedMessage.Length);
                    await stream.FlushAsync();
                    LogMessage?.Invoke($"[LOG] Mensaje enviado: {message}");
                }
            }
            catch (Exception ex)
            {
                LogMessage?.Invoke($"[ERROR] No se pudo enviar mensaje: {ex.Message}");
            }
            finally
            {
                client?.Dispose();
            }
        }

        /// <summary>
        /// Detiene el listener
        /// </summary>
        public void StopListening()
        {
            _isListening = false;
            _listener?.Stop();
            DisconnectIncoming();
            LogMessage?.Invoke("[LOG] Listener detenido");
        }

        private void DisconnectIncoming()
        {
            if (_incomingClient != null)
            {
                _incomingStream?.Dispose();
                _incomingClient?.Dispose();
                _incomingClient = null;
                _incomingStream = null;
                ConnectionStateChanged?.Invoke(false);
                LogMessage?.Invoke("[LOG] Desconectado");
            }
        }

        public void Dispose()
        {
            StopListening();
            _listener?.Dispose();
            DisconnectIncoming();
        }
    }
}
