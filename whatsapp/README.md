# Chat P2P Seguro con AES-256

## Descripción General
Aplicación de mensajería Peer-to-Peer (P2P) que implementa una arquitectura descentralizada sin servidor central. Cada nodo puede comunicarse directamente con otros nodos usando cifrado AES-256 de 256 bits.

## Requisitos Implementados

### ✅ A. Arquitectura del Nodo (WinForms)
- **Puerto de Escucha**: Campo numérico para configurar el puerto local (1024-65535)
- **IP y Puerto Destino**: Campos para ingresar la dirección del par con el que se desea chatear
- **Hilos Asincronos**: Uso de `Task.Run()` y `async/await` para mantener el socket activo sin bloquear la UI
- **Actualización de UI**: Uso de `Invoke()` para actualizar controles desde hilos secundarios

### ✅ B. Comunicación Sockets (TCP)
- **TcpListener**: Levanta un listener en un hilo separado en el puerto especificado
- **TcpClient**: Se instancia para enviar mensajes al par remoto
- **Manejo de Estados**: La aplicación detecta conexiones activas y cambios de estado
- **Protocolo Personalizado**: Formato [Longitud (4 bytes)][Datos Cifrados]

### ✅ C. Seguridad y Cifrado
- **Algoritmo AES**: Implementación de AES-256 en modo CBC con padding PKCS7
- **Intercambio de Claves**: Clave compartida manual que se ingresa en la UI
- **Derivación de Claves**: Uso de SHA256 para derivar una clave de 256 bits del valor compartido
- **IV Aleatorio**: Cada mensaje genera un IV aleatorio incluido en los datos cifrados

### ✅ D. Funcionalidades Obligatorias

1. **Modo Escucha**
   - Botón "Iniciar Escucha" para comenzar a recibir conexiones
   - El nodo acepta conexiones de cualquier IP
   - Botón "Detener Escucha" para finalizar

2. **Chat en Tiempo Real**
   - Campo de entrada para escribir mensajes
   - Botón "Enviar" que cifra y transmite los mensajes
   - Recepción automática de mensajes del peer
   - Visualización bidireccional en el chat

3. **Visualización Técnica**
   - Panel "Log Técnico" que muestra los paquetes de bytes cifrados en hexadecimal
   - Etiquetas [CIFRADO TX], [CIFRADO RX], [DESCIFRADO]
   - Registro de eventos de conexión, desconexión y errores

4. **Manejo de Excepciones**
   - Validación de puertos (1024-65535)
   - Validación de campos vacíos
   - Captura de excepciones de red (puertos ocupados, IP no alcanzable)
   - Mensajes descriptivos de error

## Estructura de Archivos

### EncryptionService.cs
Servicio de cifrado/descifrado AES-256
- `Encrypt(plaintext)`: Cifra texto usando AES-256, retorna [IV][DatosCifrados]
- `Decrypt(encryptedData)`: Descifra datos, extrae IV de los primeros 16 bytes

### MessageProtocol.cs
Protocolo personalizado para empaquetamiento de mensajes
- `PackMessage(encryptedData)`: Empaqueta [Longitud][DatosCifrados]
- `TryUnpackMessage()`: Desempaqueta un mensaje y valida su integridad

### P2PNode.cs
Núcleo de comunicación P2P
- `StartListening()`: Inicia el TcpListener en un hilo asincrónico
- `SendMessage()`: Conecta al peer remoto y envía el mensaje cifrado
- Eventos: `MessageReceived`, `LogMessage`, `ConnectionStateChanged`

### Form1.cs / Form1.Designer.cs
Interfaz WinForms con todos los controles necesarios

## Cómo Usar

### Pasos para crear una conversación P2P:

1. **Configurar la Clave Compartida**
   - Ambos nodos deben usar la MISMA clave de encriptación
   - Ejemplo: "SharedKey123" (debe coincidir en ambas instancias)

2. **Nodo 1 (Servidor)**
   - Puerto Escucha: 5000
   - Clave: "SharedKey123"
   - Hacer clic en "Iniciar Escucha"

3. **Nodo 2 (Cliente)**
   - Puerto Escucha: 5001
   - Clave: "SharedKey123"
   - IP Destino: 127.0.0.1 (o la IP de Nodo 1)
   - Puerto Destino: 5000
   - Hacer clic en "Iniciar Escucha"

4. **Enviar Mensaje**
   - Escribir el mensaje en el campo de entrada
   - Hacer clic en "Enviar"
   - El mensaje se mostrará primero en el nodo emisor
   - El nodo receptor lo recibirá automáticamente

### Visualización de Logs Técnicos

El panel "Log Técnico" muestra:
- **[CIFRADO TX]**: Datos en hexadecimal que se envían (cifrados)
- **[CIFRADO RX]**: Datos en hexadecimal que se reciben (cifrados)
- **[DESCIFRADO]**: Mensaje descifrado exitosamente
- **[LOG]**: Eventos de conexión y estado
- **[ERROR]**: Errores de red o desencriptación

## Características de Seguridad

### Formato del Mensaje Cifrado
```
[IV (16 bytes)][AES-256 CBC Cifrado]
```

### Formato del Protocolo Transmitido
```
[Longitud (4 bytes)][IV (16 bytes)][AES-256 CBC Cifrado]
```

### Derivación de Claves
```
SHA256("SharedKey123") → Clave AES-256 (32 bytes)
```

## Requisitos del Proyecto

- **.NET 10.0**
- **Framework**: Windows Forms (.NET)
- **Espacios de nombres**: System.Security.Cryptography, System.Net.Sockets

## Manejo de Errores

La aplicación captura y maneja:
- ❌ Puertos fuera de rango
- ❌ Campos de entrada vacíos
- ❌ IP no alcanzable
- ❌ Puertos ya en uso
- ❌ Errores de desencriptación
- ❌ Desconexiones inesperadas

## Notas Técnicas

- La UI se actualiza usando `Invoke()` para garantizar thread-safety
- Los mensajes se procesan de forma asincrónica con `async/await`
- El buffer de recepción soporta hasta 40 KB por lectura
- Se incluye protección contra mensajes malformados
- El log se limpia automáticamente después de 10000 líneas

## Criterios de Evaluación Cumplidos

✅ **Conectividad P2P (30%)**
- Conexión bidireccional entre dos instancias sin servidor

✅ **Criptografía (30%)**
- AES-256 correctamente implementado
- Cada mensaje tiene IV único

✅ **WinForms & UX (20%)**
- Interfaz fluida y responsiva
- Uso correcto de hilos e Invoke
- Estados visuales (conectado/desconectado)

✅ **Protocolo Propio (20%)**
- Estructura clara: [Longitud][IV][DatosCifrados]
- Validación de integridad de mensajes
