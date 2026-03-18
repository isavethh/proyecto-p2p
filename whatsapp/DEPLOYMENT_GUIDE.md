# 🚀 Guía de Despliegue - Chat P2P Seguro

## Pre-Requisitos

- **.NET 10.0** instalado
- **Visual Studio 2026** o **Visual Studio Code**
- **Windows 10/11** (para WinForms)
- **Admin para algunos comandos de firewall**

---

## Instalación Rápida

### 1. Obtener el Código

```bash
# Clonar o descargar el repositorio
cd C:\Users\DELL\source\repos\whatsapp\
```

### 2. Compilar

```bash
# Opción A: Con dotnet CLI
dotnet build

# Opción B: Con Visual Studio
# Abrir whatsapp.sln y compilar (Ctrl+Shift+B)

# Opción C: Release optimizado
dotnet build -c Release
```

### 3. Ejecutar

```bash
# Opción A: CLI
dotnet run

# Opción B: Ejecutable compilado
# Navegar a: bin/Debug/net10.0-windows/whatsapp.exe
# Doble clic

# Opción C: Desde Visual Studio
# F5 para ejecutar
```

---

## Configuración de Firewall

### Windows (Permitir Puertos)

```powershell
# Ejecutar como Administrator
# Permitir puerto 5000
netsh advfirewall firewall add rule name="P2P Chat 5000" `
  dir=in action=allow protocol=tcp localport=5000 program=any

# Permitir puerto 5001
netsh advfirewall firewall add rule name="P2P Chat 5001" `
  dir=in action=allow protocol=tcp localport=5001 program=any

# Verificar reglas
netsh advfirewall firewall show rule name="P2P*"

# Eliminar (si es necesario)
netsh advfirewall firewall delete rule name="P2P Chat 5000"
netsh advfirewall firewall delete rule name="P2P Chat 5001"
```

### Windows (Interfaz Gráfica)

1. Panel de Control → Seguridad de Windows → Firewall
2. Permitir una app a través del firewall
3. Buscar whatsapp.exe
4. Marcar las casillas de Private y Public

### Linux/macOS

```bash
# Ubuntu/Debian
sudo ufw allow 5000
sudo ufw allow 5001
sudo ufw status

# macOS (sin ufw)
# El firewall de macOS es por defecto permisivo
# Para WinForms, compilar en Windows o usar WSL2
```

---

## Configuración de Seguridad

### Generar Clave Segura

```bash
# Windows PowerShell
# Generar 32 caracteres aleatorios
-join ((65..90) + (97..122) + (48..57) | Get-Random -Count 32 | % {[char]$_})

# Linux/macOS
openssl rand -base64 24

# Alternativa online (NO para producción)
# https://passwordsgenerator.net/
```

### Ejemplo de Configuración Segura

```
DESARROLLO:
  Clave: "DevTestKey123"
  Puertos: 5000, 5001

TESTING:
  Clave: "QA#Test$Secure&2024!"
  Puertos: 6000, 6001

PRODUCCIÓN:
  Clave: "Prod#Secure$Key&2024%WithLongerLength!"
  Puertos: 8000, 8001
```

---

## Despliegue Local (Mismo PC)

### Pasos

1. **Compilar**
   ```bash
   dotnet build
   ```

2. **Ejecutar Instancia 1**
   ```bash
   dotnet run
   ```
   Configurar:
   - Puerto: 5000
   - Clave: TestKey123
   - Clic: Iniciar Escucha

3. **Ejecutar Instancia 2** (nueva ventana terminal)
   ```bash
   dotnet run
   ```
   Configurar:
   - Puerto: 5001
   - Clave: TestKey123
   - IP Destino: 127.0.0.1
   - Puerto Destino: 5000
   - Clic: Iniciar Escucha

4. **Probar Chat**
   - Escribir mensaje en cualquiera
   - Hacer clic Enviar
   - Ver en ambas ventanas

---

## Despliegue en Red Local

### Configuración Previa

1. **Obtener IPs Locales**

   **Máquina A:**
   ```bash
   ipconfig /all
   # Buscar: IPv4 Address (ej: 192.168.1.100)
   ```

   **Máquina B:**
   ```bash
   ipconfig /all
   # Buscar: IPv4 Address (ej: 192.168.1.101)
   ```

2. **Permitir Firewall en Ambas Máquinas**

   ```powershell
   # En cada máquina
   netsh advfirewall firewall add rule name="P2P Chat" `
     dir=in action=allow protocol=tcp localport=5000,5001
   ```

3. **Verificar Conectividad**

   ```bash
   # Desde Máquina B
   ping 192.168.1.100
   
   # Desde Máquina A
   ping 192.168.1.101
   ```

### Despliegue

**Máquina A (192.168.1.100):**
```
Puerto Escucha: 5000
Clave: "SecureNetKey123"
IP Destino: [Dejar vacío o 192.168.1.101]
Puerto Destino: 5001
```

**Máquina B (192.168.1.101):**
```
Puerto Escucha: 5001
Clave: "SecureNetKey123" [IGUAL]
IP Destino: 192.168.1.100
Puerto Destino: 5000
```

Ambas hacen clic en "Iniciar Escucha" y pueden chatear.

---

## Despliegue en Producción (Opcional)

### Compilación Release

```bash
# Compilar optimizado
dotnet build -c Release

# Ejecutable ubicado en:
# bin/Release/net10.0-windows/whatsapp.exe
```

### Empaquetamiento

```bash
# Crear ejecutable autocontido (puede ejecutarse sin .NET)
dotnet publish -c Release -r win-x64 --self-contained

# Ubicado en:
# bin/Release/net10.0-windows/publish/whatsapp.exe
```

### Instalador (Opcional)

Para crear un instalador profesional:
- WiX Toolset
- NSIS
- Inno Setup

---

## Verificación Post-Despliegue

### Checklist de Funcionamiento

- [ ] Aplicación abre sin errores
- [ ] Puerto de escucha se configura
- [ ] Clave se ingresa correctamente
- [ ] Botón "Iniciar Escucha" funciona
- [ ] Log muestra "[LOG] Escuchando en puerto XXXX"
- [ ] Se puede enviar mensaje
- [ ] Se recibe mensaje en otra instancia
- [ ] Log muestra [CIFRADO TX] y [CIFRADO RX]
- [ ] Mensaje aparece descifrado
- [ ] Botón "Detener Escucha" para
- [ ] Firewall está correctamente configurado

### Validación de Seguridad

```
✓ [CIFRADO TX] muestra hexadecimal largo
✓ [CIFRADO RX] es diferente cada vez
✓ [DESCIFRADO] muestra el mensaje correcto
✓ Clave incorrecta causa error
✓ Puerto ocupado causa error
✓ IP inválida causa error
```

---

## Troubleshooting de Despliegue

### Error: ".NET 10 no instalado"

```bash
# Descargar desde
https://dotnet.microsoft.com/download/dotnet/10.0

# Verificar instalación
dotnet --version
```

### Error: "Acceso denegado (puerto)"

```bash
# Ejecutar como Administrador
# Verificar permisos de puerto
netstat -ano | findstr :5000

# Liberar puerto
taskkill /PID <PID> /F
```

### Error: "No se compila"

```bash
# Limpiar y recompilar
dotnet clean
dotnet restore
dotnet build
```

### Error: "No se ve la interfaz WinForms"

- Asegurarse de compilar en Windows
- Verificar que .NET 10 esté instalado
- No es soportado en Linux/macOS para WinForms

---

## Monitoreo

### Logs de Aplicación

La aplicación crea logs en:
```
[Se muestran en el panel "Log Técnico" de la UI]
```

### Exportar Logs

```csharp
// En Form1.cs, agregar:
private void ExportLogs()
{
    string logs = txtTechnicalLog.Text;
    File.WriteAllText("logs.txt", logs);
}
```

### Monitor de Puerto

```bash
# Verificar qué usa puerto 5000
netstat -ano | findstr :5000

# En tiempo real
netstat -ab 1 | findstr :5000
```

---

## Mantenimiento

### Actualizar .NET

```bash
# Verificar versión actual
dotnet --version

# Descargar nueva versión
# https://dotnet.microsoft.com/download

# Actualizar proyecto
dotnet workload update
```

### Actualizar Dependencias

```bash
# Restaurar paquetes
dotnet restore

# Verificar actualizaciones
dotnet package search [paquete]
```

---

## Backup y Recuperación

### Guardar Configuración

```csharp
// Agregar a Form1.cs
private void SaveSettings()
{
    Properties.Settings.Default.Port = (int)numLocalPort.Value;
    Properties.Settings.Default.EncryptionKey = txtEncryptionKey.Text;
    Properties.Settings.Default.Save();
}

private void LoadSettings()
{
    numLocalPort.Value = Properties.Settings.Default.Port;
    txtEncryptionKey.Text = Properties.Settings.Default.EncryptionKey;
}
```

### Exportar Historial de Chat

```csharp
private void ExportChat()
{
    string chat = txtChatMessages.Text;
    string timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
    File.WriteAllText($"chat_{timestamp}.txt", chat);
}
```

---

## Desinstalación

### Remover de Windows

```bash
# Si se instaló como MSI
# Control Panel → Programs → Uninstall

# Si es ejecutable portable
# Solo eliminar el archivo .exe
```

### Limpiar Registros

```powershell
# Eliminar reglas de firewall
netsh advfirewall firewall delete rule name="P2P Chat 5000"
netsh advfirewall firewall delete rule name="P2P Chat 5001"
```

---

## Soporte Técnico

### Contacto

Para reportar issues:
1. Revisar TESTING_GUIDE.md
2. Consultar CONFIGURATION.md
3. Revisar TECHNICAL_DOCS.md

### Información Útil para Reportar

Cuando reporte un problema, incluya:

```
Ambiente:
- Windows/Linux/macOS
- .NET Version: dotnet --version
- Visual Studio Version

Error:
- Mensaje exacto
- Stack trace
- Pasos para reproducir

Configuración:
- Puertos usados
- IP (localhost o red)
- Firewall habilitado/deshabilitado
```

---

## Próximos Pasos (Mejoras Futuras)

1. **Múltiples Conexiones**
   - Soportar chat grupal

2. **Persistencia**
   - Base de datos para historial

3. **TLS/SSL**
   - Cifrado a nivel de transporte

4. **Autenticación**
   - Usuario y contraseña

5. **Interfaz Mejorada**
   - WPF o ASP.NET

---

## Resumen

La aplicación está lista para:

✅ **Desarrollo local** (mismo PC)
✅ **Testing en red local** (múltiples máquinas)
✅ **Producción** (con configuración adecuada)

Siga los pasos según su caso de uso.

---

**Última Actualización**: 2024
**Versión**: 1.0
**Estado**: Listo para Despliegue ✅
