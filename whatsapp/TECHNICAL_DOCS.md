# Documentación Técnica - Chat P2P Seguro

## Arquitectura Técnica

### Componentes Principales

```
┌─────────────────────────────────────────────────────────┐
│                     Form1 (WinForms)                    │
│  - Interfaz de Usuario                                  │
│  - Manejo de Eventos                                    │
│  - Actualización de Controles Thread-Safe               │
└─────────────────────────────────────────────────────────┘
                            ▲
                            │
                ┌───────────┴───────────┐
                │                       │
         ┌──────▼────────┐      ┌──────▼────────┐
         │   P2PNode     │      │ EncryptionSvc │
         │ - Listener    │      │ - AES-256     │
         │ - Client      │      │ - SHA256      │
         │ - Eventos     │      │ - IV Gen.     │
         └──────┬────────┘      └───────────────┘
                │
         ┌──────▼────────┐
         │ MessageProto  │
         │ - Empaqueta   │
         │ - Desempaqueta│
         │ - Valida      │
         └───────────────┘
                │
         ┌──────▼──────────────┐
         │   TcpListener/      │
         │   TcpClient         │
         │   NetworkStream     │
         └─────────────────────┘
```

## Flujo de Cifrado de Mensajes

### Envío (TX)

```
"Hola Mundo"
     │
     ▼
┌─────────────────────────────┐
│ EncryptionService.Encrypt() │
│ 1. Generar IV aleatorio     │
│ 2. Aplicar AES-256 CBC      │
│ 3. Retornar [IV][Cifrado]   │
└─────────────────────────────┘
     │
     ▼
[16 bytes IV][32+ bytes cifrados]
     │
     ▼
┌──────────────────────────────┐
│ MessageProtocol.PackMessage()│
│ Agregar: [Longitud (4 bytes)]│
└──────────────────────────────┘
     │
     ▼
[4 bytes Longitud][16 IV][Cifrado...]
     │
     ▼
TcpClient.GetStream().WriteAsync()
```

### Recepción (RX)

```
[4 bytes Longitud][16 IV][Cifrado...] ──┐
                                         │
                                         ▼
                            ┌──────────────────────────┐
                            │ MessageProtocol.Unpack   │
                            │ 1. Leer longitud         │
                            │ 2. Validar integridad    │
                            │ 3. Extraer datos cifrados│
                            └──────────────────────────┘
                                         │
                                         ▼
                            [16 IV][Cifrado...]
                                         │
                                         ▼
                            ┌──────────────────────────┐
                            │ Encrypt.Decrypt()        │
                            │ 1. Extraer IV            │
                            │ 2. Aplicar AES-256 CBC   │
                            │ 3. Retornar plaintext    │
                            └──────────────────────────┘
                                         │
                                         ▼
                                   "Hola Mundo"
```

## Especificaciones de Seguridad

### Algoritmo AES-256
- **Modo**: CBC (Cipher Block Chaining)
- **Tamaño de Bloque**: 128 bits (16 bytes)
- **Tamaño de Clave**: 256 bits (32 bytes)
- **Padding**: PKCS7
- **IV**: 128 bits aleatorio por mensaje

### Derivación de Claves
```
Input: "SharedKey123" (variable)
       │
       ▼
SHA256.ComputeHash()
       │
       ▼
Output: 32 bytes (256 bits) → Clave AES
```

### Protocolo de Mensaje
```
[HEADER (4 bytes)][PAYLOAD]

HEADER: Longitud en bytes del PAYLOAD (Big Endian)
PAYLOAD: [IV (16 bytes)][AES-256-CBC Ciphertext]

Tamaño Total: 4 + 16 + (ciphertext)
```

## Código de Ejemplo - Uso de EncryptionService

```csharp
// Crear servicio con clave compartida
var encService = new EncryptionService("MySharedKey");

// Cifrar
string plainText = "Mensaje secreto";
byte[] encrypted = encService.Encrypt(plainText);
// encrypted = [16 bytes IV][variable bytes ciphertext]

// Descifrar
string decrypted = encService.Decrypt(encrypted);
// decrypted = "Mensaje secreto"
```

## Código de Ejemplo - Uso de P2PNode

```csharp
// Inicializar nodo
var node = new P2PNode(
    listeningPort: 5000,
    sharedEncryptionKey: "MySharedKey"
);

// Suscribirse a eventos
node.MessageReceived += (msg, encrypted) => 
{
    Console.WriteLine($"Recibido: {msg}");
    Console.WriteLine($"Bytes: {BitConverter.ToString(encrypted)}");
};

node.LogMessage += (log) => Console.WriteLine(log);

node.ConnectionStateChanged += (connected) =>
    Console.WriteLine(connected ? "Conectado" : "Desconectado");

// Iniciar escucha
node.StartListening();

// Enviar mensaje
await node.SendMessage("192.168.1.100", 5001, "¡Hola!");

// Detener
node.StopListening();
node.Dispose();
```

## Thread Safety

### Mecanismo de UI Thread Safety

```csharp
// Desde thread de red
private void P2PNode_MessageReceived(string message, byte[] encrypted)
{
    // Usar Invoke para ejecutar en UI thread
    this.Invoke(() =>
    {
        txtChatMessages.AppendText($"[REMOTO] {message}\r\n");
    });
}
```

### Operaciones Asincrónicas

```csharp
// No bloqueante
await client.ConnectAsync(ip, port);
await stream.WriteAsync(data, 0, data.Length);
int bytesRead = await stream.ReadAsync(buffer, 0, bufferSize);
```

## Manejo de Errores

### Validaciones en Form1

```csharp
if (numLocalPort.Value < 1024 || numLocalPort.Value > 65535)
    throw new ArgumentException("Puerto inválido");

if (string.IsNullOrWhiteSpace(txtEncryptionKey.Text))
    throw new ArgumentException("Clave no ingresada");
```

### Captura en P2PNode

```csharp
catch (SocketException ex)
    when (ex.SocketErrorCode == SocketError.AddressAlreadyInUse)
{
    LogMessage?.Invoke($"[ERROR] Puerto {port} ya está en uso");
}

catch (InvalidOperationException ex)
{
    LogMessage?.Invoke($"[ERROR] No se puede conectar: {ex.Message}");
}
```

## Optimizaciones Implementadas

### 1. Buffer Circular
```csharp
private byte[] _receiveBuffer = new byte[BufferSize * 10]; // 40 KB

// Mover datos no procesados al inicio
Array.Copy(_receiveBuffer, bytesConsumed, _receiveBuffer, 0, 
           totalBytesRead - bytesConsumed);
```

### 2. Límite de Log
```csharp
if (txtTechnicalLog.Lines.Length > 10000)
{
    // Mantener últimas 5000 líneas
    string[] lines = txtTechnicalLog.Lines;
    txtTechnicalLog.Clear();
    for (int i = 5000; i < lines.Length; i++)
        txtTechnicalLog.AppendText(lines[i] + "\r\n");
}
```

### 3. Operaciones Async
```csharp
// No usar Thread.Sleep ni bloqueos
Task.Run(() => AcceptIncomingConnections()); // No bloqueante
await _p2pNode.SendMessage(ip, port, message); // Async/await
```

## Limitaciones Actuales

1. **Clave Manual**: Requiere intercambio previo de clave (no automatizado)
2. **Una Conexión Activa**: Solo puede recibir de un peer a la vez
3. **Sin Persistencia**: Los mensajes no se almacenan
4. **Sin Compresión**: Los datos se transmiten sin comprimir
5. **Sin Autenticación**: No valida identidad del remitente

## Mejoras Futuras Posibles

### 1. Múltiples Conexiones Simultáneas
```csharp
private Dictionary<string, (TcpClient client, NetworkStream stream)> 
    _activeConnections;
```

### 2. Intercambio de Claves Diffie-Hellman
```csharp
public class KeyExchange
{
    public static (BigInteger publicKey, BigInteger privateKey) 
        GenerateKeyPair();
    
    public static byte[] DeriveSharedKey(BigInteger theirPublic, 
                                        BigInteger ourPrivate);
}
```

### 3. Firma Digital (HMAC-SHA256)
```csharp
using (var hmac = new HMACSHA256(_key))
{
    byte[] signature = hmac.ComputeHash(message);
    // [IV][Ciphertext][Signature]
}
```

### 4. Compresión GZIP
```csharp
using (var ms = new MemoryStream())
using (var gz = new GZipStream(ms, CompressionMode.Compress))
{
    gz.Write(plainBytes, 0, plainBytes.Length);
    return ms.ToArray();
}
```

### 5. Persistencia en Base de Datos
```csharp
public class MessageStore
{
    public void SaveMessage(string sender, string content, DateTime timestamp);
    public List<Message> GetHistory(int limitDays = 7);
}
```

### 6. Reconexión Automática
```csharp
private async Task ReconnectWithRetry(int maxAttempts = 5)
{
    for (int i = 0; i < maxAttempts; i++)
    {
        try
        {
            await _client.ConnectAsync(_lastRemoteIP, _lastRemotePort);
            return;
        }
        catch
        {
            await Task.Delay(TimeSpan.FromSeconds(Math.Pow(2, i)));
        }
    }
}
```

### 7. Interfaz de Múltiples Conversaciones
```
┌──────────────────────────┐
│ Contactos:               │
│ ☐ 192.168.1.100:5000    │
│ ☑ 192.168.1.101:5001    │ ← Activo
│ ☐ 192.168.1.102:5002    │
└──────────────────────────┘
```

## Certificados de Seguridad Futuro (TLS)

```csharp
using (var sslStream = new SslStream(_tcpClient.GetStream()))
{
    await sslStream.AuthenticateAsClientAsync("hostname");
    // Comunicación cifrada a nivel de transporte
}
```

## Monitoreo y Debugging

### Variables de Traza
```csharp
private bool _debugMode = true;

private void DebugLog(string message)
{
    if (_debugMode)
    {
        System.Diagnostics.Debug.WriteLine(
            $"[{DateTime.Now:HH:mm:ss.fff}] {message}");
    }
}
```

### Profiler de Performance
```csharp
var sw = System.Diagnostics.Stopwatch.StartNew();

// Operación a medir
await node.SendMessage(ip, port, message);

sw.Stop();
LogMessage?.Invoke($"[PERF] Envío completado en {sw.ElapsedMilliseconds}ms");
```

## Referencias

- [NIST SP 800-38A - AES Modes](https://nvlpubs.nist.gov/nistpubs/Legacy/SP/nistspecialpublication800-38a.pdf)
- [FIPS 197 - AES](https://nvlpubs.nist.gov/nistpubs/FIPS/NIST.FIPS.197.pdf)
- [RFC 5116 - AEAD Interface](https://tools.ietf.org/html/rfc5116)
