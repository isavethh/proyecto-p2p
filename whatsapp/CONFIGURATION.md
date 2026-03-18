# Configuración y Mejores Prácticas

## 1. Configuración de Seguridad

### Clave Compartida Segura
```
❌ INSEGURO:
- "password"
- "123456"
- "admin"
- Usuario:Contraseña

✓ SEGURO:
- "mY#$uP3r$ecure&Key2024!"
- Mínimo 16 caracteres
- Mezclar mayúsculas, minúsculas, números, símbolos
- Nunca usar información personal
```

### Recomendaciones
1. **Generar claves aleatorias**
   - Usar generador online: passwordsgenerator.net
   - Generar claves de 32+ caracteres

2. **Compartir claves de forma segura**
   - ❌ NO por email
   - ❌ NO por chat público
   - ✓ En persona
   - ✓ Llamada telefónica
   - ✓ Mensaje de WhatsApp/Signal (cifrado)

3. **Cambiar claves regularmente**
   - Cada 30-90 días en producción
   - Después de cada sesión de desarrollo en testing

## 2. Configuración de Puertos

### Rango de Puertos Recomendados
```
Sistema:
- 0-1023: Reservados (se requiere admin)

Desarrollo:
- 5000-5999: Aplicaciones personalizadas
- 8000-8999: Desarrollo local

Producción:
- 8000+: Usar puertos > 1024
- 443: Si se implementa HTTPS
```

### Ejemplo de Configuración
```
Instancia 1: Puerto 5000
Instancia 2: Puerto 5001
Instancia 3: Puerto 5002
...
```

## 3. Firewall - Permitir Puertos

### Windows (PowerShell - Admin)
```powershell
# Permitir puerto 5000
netsh advfirewall firewall add rule name="P2P Chat 5000" `
  dir=in action=allow protocol=tcp localport=5000

# Permitir puerto 5001
netsh advfirewall firewall add rule name="P2P Chat 5001" `
  dir=in action=allow protocol=tcp localport=5001

# Verificar reglas
netsh advfirewall firewall show rule name="P2P*"
```

### Linux/macOS
```bash
# Ubuntu/Debian
sudo ufw allow 5000
sudo ufw allow 5001

# Verificar
sudo ufw status
```

## 4. Pruebas en Red Local

### Paso 1: Verificar Conectividad
```bash
# En máquina A
ipconfig /all  # Windows
ifconfig       # Linux/Mac

# Anotar IP Local (ej: 192.168.1.100)
```

### Paso 2: Configurar Instancias
```
Máquina A (192.168.1.100):
- Puerto Escucha: 5000
- Clave: "SecureTestKey123"
- IP Destino: 192.168.1.101
- Puerto Destino: 5001

Máquina B (192.168.1.101):
- Puerto Escucha: 5001
- Clave: "SecureTestKey123"  (IGUAL)
- IP Destino: 192.168.1.100
- Puerto Destino: 5000
```

### Paso 3: Verificar con Ping
```bash
ping 192.168.1.100  # Desde máquina B
ping 192.168.1.101  # Desde máquina A
```

## 5. Troubleshooting

### Error: "El puerto ya está en uso"
```
Solución 1: Cambiar a otro puerto
- Puerto 5000 → 5002 o 5003

Solución 2: Liberar puerto
Windows:
  netstat -ano | findstr :5000
  taskkill /PID <PID> /F

Linux:
  lsof -i :5000
  kill -9 <PID>
```

### Error: "No se puede conectar a IP"
```
Verificar:
1. ¿La otra instancia está escuchando?
   - Ver "[LOG] Escuchando en puerto"
   
2. ¿IP y puerto son correctos?
   - ipconfig /all (Windows)
   - ifconfig (Linux)
   
3. ¿Firewall bloquea?
   - Permitir puerto en firewall
   
4. ¿Red local conectada?
   - ping <IP>
```

### Error: "Falló desencriptación"
```
Verificar:
1. ¿Misma clave en ambos nodos?
   - Clave A: "MyKey"
   - Clave B: "MyKey" ← Debe ser idéntica
   
2. ¿Mensaje cifrado correctamente?
   - Ver [CIFRADO TX] en Log
   
3. ¿Datos no corrompidos en tránsito?
   - Verificar [CIFRADO RX] contiene datos
```

## 6. Monitoreo en Producción

### Métricas Importantes
```csharp
private class Metrics
{
    public int MessagesSent { get; set; }
    public int MessagesReceived { get; set; }
    public int ConnectionAttempts { get; set; }
    public int FailedAttempts { get; set; }
    public DateTime StartTime { get; set; }
}
```

### Logging Recomendado
```
[TIMESTAMP] [LEVEL] [COMPONENT] Mensaje
12:34:56.789 [INFO] P2PNode Escuchando en puerto 5000
12:34:57.123 [INFO] P2PNode Conexión de 192.168.1.101:45678
12:34:58.456 [DEBUG] Encryption IV: A1B2C3D4...
12:34:59.789 [INFO] P2PNode Mensaje recibido
12:35:00.012 [WARN] P2PNode Conexión desconectada
12:35:01.345 [ERROR] P2PNode Falló desencriptación
```

## 7. Optimizaciones de Performance

### Buffer Management
```csharp
// Actualizar tamaño de buffer según necesidad
private const int BufferSize = 4096;     // 4 KB por lectura
private byte[] _receiveBuffer;           // 40 KB total

// Calidad vs Velocidad
4096   bytes → Rápido, múltiples lecturas
65536  bytes → Balance
262144 bytes → Para archivos grandes
```

### Async/Await
```csharp
// ✓ CORRECTO - No bloqueante
await client.ConnectAsync(ip, port);
int bytesRead = await stream.ReadAsync(buffer, 0, bufferSize);

// ❌ INCORRECTO - Bloqueante
client.Connect(ip, port);
int bytesRead = stream.Read(buffer, 0, bufferSize);
```

### Pooling de Conexiones
```csharp
// Futuro: Reutilizar conexiones
private Dictionary<string, TcpClient> _connectionPool;

public async Task SendWithPooling(string key, string message)
{
    if (_connectionPool.TryGetValue(key, out var client))
    {
        // Reutilizar conexión existente
        await SendViaExistingConnection(client, message);
    }
    else
    {
        // Crear nueva conexión
        await SendViaNewConnection(key, message);
    }
}
```

## 8. Seguridad Avanzada

### Validación de Mensajes
```csharp
public class MessageValidator
{
    public bool IsValid(string message)
    {
        // No está vacío
        if (string.IsNullOrWhiteSpace(message))
            return false;
        
        // No es muy largo (ej: >100KB)
        if (message.Length > 102400)
            return false;
        
        // No contiene caracteres nulos
        if (message.Contains('\0'))
            return false;
        
        return true;
    }
}
```

### Rate Limiting
```csharp
public class RateLimiter
{
    private Dictionary<string, List<DateTime>> _messageHistory;
    private const int MaxMessagesPerSecond = 10;
    
    public bool IsAllowed(string senderIP)
    {
        var now = DateTime.UtcNow;
        if (!_messageHistory.TryGetValue(senderIP, out var history))
            return true;
        
        // Remover mensajes > 1 segundo atrás
        history.RemoveAll(t => (now - t).TotalSeconds > 1);
        
        return history.Count < MaxMessagesPerSecond;
    }
}
```

## 9. Backup y Recuperación

### Guardar Historial de Chat
```csharp
public class ChatHistory
{
    private List<ChatMessage> _messages;
    
    public void SaveToFile(string filename)
    {
        var json = JsonConvert.SerializeObject(_messages);
        File.WriteAllText(filename, json);
    }
    
    public void LoadFromFile(string filename)
    {
        if (File.Exists(filename))
        {
            var json = File.ReadAllText(filename);
            _messages = JsonConvert.DeserializeObject<List<ChatMessage>>(json);
        }
    }
}
```

## 10. Checklist de Despliegue

- [ ] Clave compartida generada (32+ caracteres)
- [ ] Clave comunicada de forma segura
- [ ] Puertos permitidos en firewall
- [ ] Conectividad de red verificada (ping)
- [ ] Puertos libres (no en uso)
- [ ] Aplicación compilada en Release
- [ ] Log técnico verificado
- [ ] Mensajes de prueba enviados
- [ ] Desencriptación validada
- [ ] Reconexión probada
- [ ] Performance monitoreada
- [ ] Errores documentados

---

## Configuración de Ejemplo - Producción

```
NODO_A = {
    Puerto: 8000,
    Clave: "Pr0duct1on!SecureK3y#2024$",
    PermitirRedes: ["192.168.1.0/24"],
    MaxConexiones: 10,
    TimeoutSegundos: 30
}

NODO_B = {
    Puerto: 8001,
    Clave: "Pr0duct1on!SecureK3y#2024$",
    PermitirRedes: ["192.168.1.0/24"],
    MaxConexiones: 10,
    TimeoutSegundos: 30
}
```
