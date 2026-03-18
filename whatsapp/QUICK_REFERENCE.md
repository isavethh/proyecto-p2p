# Referencia Rápida - Chat P2P Seguro

## Inicio Rápido (5 minutos)

### Paso 1: Ejecutar Primera Instancia
1. Compilar: `dotnet build`
2. Ejecutar: `dotnet run`
3. Configurar:
   - Puerto: `5000`
   - Clave: `TestKey123`
4. Hacer clic: `Iniciar Escucha`

### Paso 2: Ejecutar Segunda Instancia
1. Abrir otra ventana/máquina
2. Ejecutar: `dotnet run`
3. Configurar:
   - Puerto: `5001`
   - Clave: `TestKey123` (IGUAL)
   - IP Destino: `127.0.0.1`
   - Puerto Destino: `5000`
4. Hacer clic: `Iniciar Escucha`

### Paso 3: Enviar Mensaje
1. En cualquier instancia
2. Escribir: "Hola"
3. Hacer clic: `Enviar`
4. ✓ Ver en ambas ventanas

## Comandos Útiles

### Compilación
```bash
dotnet build              # Compilar
dotnet run               # Ejecutar
dotnet run --release     # Release optimizado
```

### Verificación de Puertos
```bash
# Windows
netstat -ano | findstr :5000

# Linux/Mac
lsof -i :5000

# Liberar puerto
# Windows: taskkill /PID <PID> /F
# Linux: kill -9 <PID>
```

### Firewall
```bash
# Windows
netsh advfirewall firewall add rule name="P2P" dir=in action=allow protocol=tcp localport=5000

# Linux
sudo ufw allow 5000
```

## Validaciones Rápidas

### ✓ Funcionando Correctamente
```
[LOG] Escuchando en puerto 5000
[LOG] Nueva conexión desde 127.0.0.1:XXXXX
[CIFRADO TX] A1B2C3D4E5F6... (hexadecimal largo)
[CIFRADO RX] 7G8H9I0J1K2L... (diferente cada vez)
[DESCIFRADO] Tu mensaje aquí
```

### ✗ Problemas Comunes
```
Error: Puerto ya en uso
→ Cambiar puerto o liberar el anterior

Error: No se puede conectar
→ Verificar IP y que el otro nodo esté escuchando

Error: Falló desencriptación
→ Verificar que ambos usen la MISMA clave
```

## Estructura de Archivos

```
whatsapp/
├── Form1.cs                  ← Interfaz principal
├── Form1.Designer.cs         ← Diseño de formulario
├── P2PNode.cs              ← Comunicación P2P
├── EncryptionService.cs    ← AES-256 cifrado
├── MessageProtocol.cs      ← Protocolo personalizado
├── UsageExamples.cs        ← Ejemplos de uso
├── P2PEncryptionTests.cs   ← Pruebas unitarias
├── README.md               ← Documentación principal
├── TESTING_GUIDE.md        ← Guía de pruebas
├── TECHNICAL_DOCS.md       ← Documentación técnica
├── CONFIGURATION.md        ← Configuración
└── QUICK_REFERENCE.md      ← Este archivo
```

## Requisitos Cumplidos

✅ **Arquitectura P2P**
- Sin servidor central
- Comunicación directa peer-to-peer
- TcpListener y TcpClient

✅ **Criptografía AES-256**
- 256 bits (32 bytes)
- Modo CBC con PKCS7
- IV aleatorio por mensaje

✅ **WinForms Responsiva**
- Interfaz fluida
- Uso de async/await
- Thread-safe con Invoke()

✅ **Protocolo Personalizado**
- Formato: [Longitud][IV][Datos Cifrados]
- Validación de integridad
- Manejo de errores

✅ **Funcionalidades**
- Modo escucha
- Chat bidireccional
- Log de bytes cifrados
- Manejo de excepciones

## Estadísticas del Proyecto

| Métrica | Valor |
|---------|-------|
| Líneas de Código | ~800 |
| Clases | 4 principales |
| Métodos Públicos | 15+ |
| Pruebas Unitarias | 12 |
| Documentación | 5 archivos |
| .NET Target | 10.0 |

## Código Mínimo para Empezar

```csharp
// Crear nodo
var node = new P2PNode(5000, "SharedKey");

// Suscribirse a eventos
node.LogMessage += (msg) => Console.WriteLine(msg);
node.MessageReceived += (msg, encrypted) => 
    Console.WriteLine($"Recibido: {msg}");

// Iniciar
node.StartListening();

// Enviar
await node.SendMessage("127.0.0.1", 5001, "¡Hola!");
```

## Cifrado en 3 Pasos

```csharp
// 1. Crear servicio
var crypto = new EncryptionService("MiClave");

// 2. Cifrar
byte[] encriptado = crypto.Encrypt("Mensaje");

// 3. Descifrar
string mensaje = crypto.Decrypt(encriptado);
```

## Protocolo en 3 Pasos

```csharp
// 1. Empaquetar
byte[] paquete = MessageProtocol.PackMessage(encriptado);

// 2. Enviar (NetworkStream)
stream.Write(paquete, 0, paquete.Length);

// 3. Recibir y Desempaquetar
if (MessageProtocol.TryUnpackMessage(buffer, bytesLeidos, 
    out byte[] datos, out int consumidos))
{
    string mensaje = crypto.Decrypt(datos);
}
```

## Configuración de Ejemplo

### Desarrollo Local
```
Node A: 127.0.0.1:5000 (Escucha)
Node B: 127.0.0.1:5001 (Escucha)
Key: "DevTestKey123"
```

### Red Local
```
Node A: 192.168.1.100:5000
Node B: 192.168.1.101:5001
Key: "LocalNetworkKey"
```

### Producción (Ejemplo)
```
Node A: 203.0.113.10:8000
Node B: 203.0.113.11:8000
Key: "SecureProduction#Key$2024!"
```

## Tiempos de Respuesta Esperados

| Operación | Tiempo |
|-----------|--------|
| Inicio del listener | < 100 ms |
| Conexión peer | 100-500 ms |
| Cifrado del mensaje | < 10 ms |
| Transmisión (LAN) | 1-5 ms |
| Descifrado | < 10 ms |
| Total por mensaje | < 100 ms |

## Tamaños Típicos

| Elemento | Tamaño |
|----------|--------|
| IV | 16 bytes |
| Mensaje "Hola" | 9 bytes |
| Cifrado de "Hola" | 16 bytes (PKCS7) |
| Empaquetado | 4 + 16 + 16 = 36 bytes |
| Log Hex | ~72 caracteres |

## Llamadas de Función Principales

```csharp
// P2PNode
node.StartListening()                    // Iniciar
node.StopListening()                     // Detener
await node.SendMessage(ip, port, msg)    // Enviar

// EncryptionService
encService.Encrypt(plaintext)            // Cifrar
encService.Decrypt(encryptedData)        // Descifrar

// MessageProtocol
PackMessage(encryptedData)               // Empaquetar
TryUnpackMessage(buffer, len, ...)       // Desempaquetar
```

## Eventos Disponibles

```csharp
node.LogMessage += (log) => { }          // Mensajes de log
node.MessageReceived += (msg, enc) => {} // Mensaje recibido
node.ConnectionStateChanged += (con) => // Cambio de estado
```

## Manejo Típico de Errores

```
Validación de Entrada
    ↓
Intentar Operación
    ↓ (Error)
Capturar Excepción
    ↓
Registrar en Log
    ↓
Mostrar al Usuario
```

## Testing Checklist

- [ ] Cifrado/Descifrado correcto
- [ ] IV es aleatorio
- [ ] Diferentes claves fallan
- [ ] Protocolo empaqueta/desempaqueta
- [ ] Mensajes largos funcionan
- [ ] Caracteres especiales funciona
- [ ] Conexión establece
- [ ] Mensajes se reciben
- [ ] Desconexión correcta
- [ ] Log muestra datos

## Recursos Externos

- **Documentación AES**: https://nvlpubs.nist.gov/
- **RFC Protocolos**: https://tools.ietf.org/
- **.NET Docs**: https://docs.microsoft.com/dotnet/
- **WinForms**: https://docs.microsoft.com/windows/
- **Sockets**: https://docs.microsoft.com/api/system.net.sockets/

## Apoyo y Documentación

1. **README.md** - Comenzar aquí
2. **TESTING_GUIDE.md** - Cómo probar
3. **TECHNICAL_DOCS.md** - Detalles arquitectónicos
4. **CONFIGURATION.md** - Configuración avanzada
5. **Código comentado** - Explicaciones inline

## Versiones

- **v1.0** - Versión inicial
  - P2P básico
  - AES-256 CBC
  - Protocolo personalizado
  - Interfaz WinForms

- **v1.1 (Planeado)**
  - Múltiples conexiones
  - Persistencia de mensajes
  - Compresión GZIP
  - TLS/SSL

---

**Última Actualización**: 2024
**Estado**: Funcional
**License**: Educational Use
