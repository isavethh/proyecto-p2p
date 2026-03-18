# Guía de Prueba - Chat P2P Seguro

## Prueba Local (Misma Máquina)

### Requisitos
- Aplicación compilada
- Dos instancias de la aplicación ejecutándose

### Pasos de Prueba

#### 1. Iniciar Primera Instancia (Nodo A)
```
Configuración:
- Puerto Escucha: 5000
- Clave Compartida: MiClaveSuperSecreta
- IP Destino: 127.0.0.1
- Puerto Destino: 5001 (Nodo B)
```

**Acciones:**
1. Ingresar puerto 5000
2. Ingresar clave "MiClaveSuperSecreta"
3. Hacer clic en "Iniciar Escucha"
4. Observar en Log: "[LOG] Escuchando en puerto 5000"

#### 2. Iniciar Segunda Instancia (Nodo B)
```
Configuración:
- Puerto Escucha: 5001
- Clave Compartida: MiClaveSuperSecreta (IGUAL que Nodo A)
- IP Destino: 127.0.0.1
- Puerto Destino: 5000 (Nodo A)
```

**Acciones:**
1. Ingresar puerto 5001
2. Ingresar la misma clave "MiClaveSuperSecreta"
3. Hacer clic en "Iniciar Escucha"
4. Observar en Log: "[LOG] Escuchando en puerto 5001"

#### 3. Prueba de Comunicación

**Desde Nodo A:**
1. Escribir mensaje: "Hola desde Nodo A"
2. Hacer clic en "Enviar"
3. Observar:
   - Mensaje aparece en chat: "[TÚ] Hola desde Nodo A"
   - Log muestra: "[CIFRADO TX] [hexadecimal]"
   - Log muestra: "[LOG] Mensaje enviado: Hola desde Nodo A"

**En Nodo B:**
1. Observar automáticamente:
   - "[Nueva conexión desde 127.0.0.1:xxxxx]"
   - "[CIFRADO RX] [hexadecimal diferente]"
   - "[DESCIFRADO] Hola desde Nodo A"
   - "[REMOTO] Hola desde Nodo A"

**Desde Nodo B (Respuesta):**
1. Escribir mensaje: "Respuesta desde Nodo B"
2. Hacer clic en "Enviar"
3. El Nodo A recibe el mensaje automáticamente

### Verificaciones de Seguridad

#### 1. Verificar Cifrado Funcionando
✓ Los valores hexadecimales en [CIFRADO TX] y [CIFRADO RX] son DIFERENTES
✓ El contenido en hexadecimal NO es legible (es cifrado)
✓ El mensaje aparece desencriptado en el chat

#### 2. Verificar Integridad de Clave
**Prueba 1: Misma clave**
```
Nodo A: MiClaveSuperSecreta
Nodo B: MiClaveSuperSecreta
Resultado: ✓ Funcionará correctamente
```

**Prueba 2: Clave diferente**
```
Nodo A: MiClaveSuperSecreta
Nodo B: OtraClave
Resultado: ✗ Mostrará error de desencriptación
```

#### 3. Probar Desconexión
1. Detener Nodo A con "Detener Escucha"
2. En Nodo B observar:
   - "[LOG] Conexión cerrada por el remoto"
   - "Estado: Desconectado"

### Casos de Error Esperados

#### Error 1: Puerto Ocupado
```
Configuración:
- Nodo A en puerto 5000
- Nodo B también en puerto 5000
Resultado: [ERROR] No se pudo iniciar listener: El puerto ya está en uso
```

#### Error 2: IP No Alcanzable
```
Configuración:
- Nodo A envía a: 192.168.1.999:5000
Resultado: [ERROR] No se pudo enviar mensaje: La IP no es alcanzable
```

#### Error 3: Clave Incorrecta
```
Nodo A cifra con: MiClaveSuperSecreta
Nodo B trata de descifrar con: OtraClave
Resultado: [ERROR] Falló desencriptación: Datos inválidos
```

### Prueba en Red Local

Para comunicarse entre máquinas diferentes:

**Máquina A (192.168.1.100):**
```
- Puerto Escucha: 5000
- Clave: MiClaveSuperSecreta
- IP Destino: 192.168.1.101
- Puerto Destino: 5001
```

**Máquina B (192.168.1.101):**
```
- Puerto Escucha: 5001
- Clave: MiClaveSuperSecreta
- IP Destino: 192.168.1.100
- Puerto Destino: 5000
```

**Requisito:** Ambas máquinas deben estar en la misma red local y no tener firewall bloqueando los puertos.

### Monitoreo de Logs Técnicos

El panel de Log Técnico muestra información valiosa:

```
[FORMATO ESPERADO]
12:34:56 - [LOG] Escuchando en puerto 5000
12:34:57 - [LOG] Nueva conexión desde 127.0.0.1:12345
12:34:58 - [CIFRADO TX] 4A7B9E2F1C... (variable según IV aleatorio)
12:34:59 - [LOG] Mensaje enviado: Hola
12:35:00 - [CIFRADO RX] 8D3A1F7E9B... (diferente cada vez)
12:35:01 - [DESCIFRADO] Hola
```

### Verificación de Flujo Completo

1. ✓ Iniciar Escucha en ambos nodos
2. ✓ Ver "[LOG] Escuchando en puerto XXXX"
3. ✓ Enviar mensaje desde Nodo A
4. ✓ Ver [CIFRADO TX] en Nodo A
5. ✓ Ver [CIFRADO RX] en Nodo B (hexadecimal diferente)
6. ✓ Ver [DESCIFRADO] en Nodo B
7. ✓ Ver mensaje descifrado en chat de Nodo B
8. ✓ Responder desde Nodo B
9. ✓ Recibir respuesta en Nodo A
10. ✓ Verificar que el log muestra eventos de conexión

### Conclusión

Si todas estas pruebas pasan correctamente:
- ✅ Arquitectura P2P funcional
- ✅ Cifrado AES-256 trabajando
- ✅ Protocolo personalizado correcto
- ✅ Interfaz WinForms responsiva
- ✅ Manejo de errores robusto
