using System;
using System.Threading.Tasks;

namespace whatsapp.Examples
{
    /// <summary>
    /// Ejemplos de uso programático de la aplicación P2P
    /// Nota: Estos ejemplos son para fines educativos y demostración
    /// </summary>
    public class UsageExamples
    {
        /// <example>
        /// Ejemplo 1: Uso básico de EncryptionService
        /// Demostra cómo cifrar y descifrar mensajes
        /// </example>
        public static void Example1_BasicEncryption()
        {
            Console.WriteLine("=== Ejemplo 1: Cifrado AES-256 ===\n");

            // Crear servicio de encriptación
            var encryptionService = new EncryptionService("MyCryptographicKey");

            // Mensaje a cifrar
            string originalMessage = "¡Este es un mensaje secreto!";
            Console.WriteLine($"Mensaje Original: {originalMessage}");

            // Cifrar
            byte[] encryptedData = encryptionService.Encrypt(originalMessage);
            Console.WriteLine($"Datos Cifrados (hex): {BitConverter.ToString(encryptedData)}");
            Console.WriteLine($"Tamaño: {encryptedData.Length} bytes");

            // Descifrar
            string decryptedMessage = encryptionService.Decrypt(encryptedData);
            Console.WriteLine($"Mensaje Descifrado: {decryptedMessage}");

            // Verificación
            bool isCorrect = originalMessage == decryptedMessage;
            Console.WriteLine($"¿Coincide?: {(isCorrect ? "✓ SÍ" : "✗ NO")}\n");
        }

        /// <example>
        /// Ejemplo 2: Mostrar diferencia entre dos cifrados
        /// Demuestra que cada cifrado es diferente debido al IV aleatorio
        /// </example>
        public static void Example2_RandomIV()
        {
            Console.WriteLine("=== Ejemplo 2: IV Aleatorio en Cada Mensaje ===\n");

            var encryptionService = new EncryptionService("MyCryptographicKey");
            string message = "Mensaje repetido";

            // Cifrar el mismo mensaje dos veces
            byte[] encrypted1 = encryptionService.Encrypt(message);
            byte[] encrypted2 = encryptionService.Encrypt(message);

            string hex1 = BitConverter.ToString(encrypted1);
            string hex2 = BitConverter.ToString(encrypted2);

            Console.WriteLine($"Primer Cifrado:  {hex1}");
            Console.WriteLine($"Segundo Cifrado: {hex2}");
            Console.WriteLine($"¿Son iguales?: {(hex1 == hex2 ? "SÍ (INSEGURO)" : "NO (CORRECTO)")}");

            // Los primeros 16 bytes son el IV (deberían ser diferentes)
            Console.WriteLine("\nPrimeros 16 bytes (IV):");
            Console.WriteLine($"IV 1: {hex1.Substring(0, 48)}...");
            Console.WriteLine($"IV 2: {hex2.Substring(0, 48)}...");
            Console.WriteLine("(Deben ser diferentes)\n");
        }

        /// <example>
        /// Ejemplo 3: Demostración de MessageProtocol
        /// Muestra cómo se empaquetan y desempaquetan mensajes
        /// </example>
        public static void Example3_MessageProtocol()
        {
            Console.WriteLine("=== Ejemplo 3: Protocolo de Mensajes ===\n");

            // Simular datos cifrados
            byte[] encryptedData = new byte[] { 0x01, 0x02, 0x03, 0x04, 0x05 };
            Console.WriteLine($"Datos Cifrados: {BitConverter.ToString(encryptedData)}");

            // Empaquetar
            byte[] packed = MessageProtocol.PackMessage(encryptedData);
            Console.WriteLine($"Mensaje Empaquetado: {BitConverter.ToString(packed)}");

            // Mostramos estructura
            byte[] lengthBytes = new byte[4];
            Array.Copy(packed, lengthBytes, 4);
            int length = BitConverter.ToInt32(lengthBytes, 0);
            Console.WriteLine($"Longitud (primeros 4 bytes): {length}");
            Console.WriteLine($"Datos (resto): {BitConverter.ToString(packed, 4)}");

            // Desempaquetar
            if (MessageProtocol.TryUnpackMessage(packed, packed.Length, 
                out byte[] unpacked, out int bytesConsumed))
            {
                Console.WriteLine($"Desempaquetado: {BitConverter.ToString(unpacked)}");
                Console.WriteLine($"Bytes Consumidos: {bytesConsumed}");
                bool matches = 
                    BitConverter.ToString(encryptedData) == 
                    BitConverter.ToString(unpacked);
                Console.WriteLine($"¿Datos coinciden?: {(matches ? "✓ SÍ" : "✗ NO")}\n");
            }
        }

        /// <example>
        /// Ejemplo 4: Uso de P2PNode (Nodo Servidor)
        /// Este ejemplo requiere ejecutarse en una tarea separada
        /// </example>
        public static async Task Example4_P2PNodeServer()
        {
            Console.WriteLine("=== Ejemplo 4: Nodo P2P (Servidor) ===\n");

            var node = new P2PNode(5000, "SharedSecureKey");

            // Suscribirse a eventos
            node.LogMessage += (log) => Console.WriteLine($"[LOG] {log}");
            node.MessageReceived += (msg, encrypted) => 
            {
                Console.WriteLine($"[RX] Mensaje: {msg}");
                Console.WriteLine($"[RX] Bytes: {BitConverter.ToString(encrypted)}");
            };
            node.ConnectionStateChanged += (connected) =>
            {
                Console.WriteLine($"[CONEXIÓN] {(connected ? "Conectado" : "Desconectado")}");
            };

            // Iniciar
            node.StartListening();
            Console.WriteLine("Servidor escuchando en puerto 5000...");
            Console.WriteLine("Presiona Enter para detener...");
            Console.ReadLine();

            node.StopListening();
            node.Dispose();
        }

        /// <example>
        /// Ejemplo 5: Uso de P2PNode (Envío de Mensaje)
        /// </example>
        public static async Task Example5_P2PNodeClient()
        {
            Console.WriteLine("=== Ejemplo 5: Nodo P2P (Cliente) ===\n");

            var node = new P2PNode(5001, "SharedSecureKey");

            node.LogMessage += (log) => Console.WriteLine($"[LOG] {log}");

            node.StartListening();

            // Esperar 1 segundo para que el servidor esté listo
            await Task.Delay(1000);

            // Enviar mensaje
            await node.SendMessage("127.0.0.1", 5000, "¡Hola desde Cliente!");

            // Esperar respuesta
            await Task.Delay(2000);

            node.StopListening();
            node.Dispose();
        }

        /// <example>
        /// Ejemplo 6: Validaciones de Seguridad
        /// Demuestra cómo fallan los mensajes con clave incorrecta
        /// </example>
        public static void Example6_SecurityValidation()
        {
            Console.WriteLine("=== Ejemplo 6: Validación de Seguridad ===\n");

            var correctKey = new EncryptionService("CorrectKey");
            var wrongKey = new EncryptionService("WrongKey");

            string message = "Información Sensible";
            Console.WriteLine($"Mensaje Original: {message}\n");

            // Cifrar con clave correcta
            byte[] encrypted = correctKey.Encrypt(message);
            Console.WriteLine($"Cifrado con Clave Correcta: {BitConverter.ToString(encrypted)}\n");

            // Desencriptar con clave correcta
            try
            {
                string decrypted1 = correctKey.Decrypt(encrypted);
                Console.WriteLine($"✓ Descifrado con Clave Correcta: {decrypted1}\n");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ Error: {ex.Message}\n");
            }

            // Intentar desencriptar con clave incorrecta
            try
            {
                string decrypted2 = wrongKey.Decrypt(encrypted);
                Console.WriteLine($"✗ SEGURIDAD COMPROMETIDA - Descifrado con Clave Incorrecta: {decrypted2}\n");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✓ Protegido - Error esperado con clave incorrecta:\n   {ex.GetType().Name}\n");
            }
        }

        /// <example>
        /// Ejemplo 7: Demostración de Flujo Completo
        /// Muestra todo el proceso desde cifrado hasta transmisión
        /// </example>
        public static void Example7_CompleteFlow()
        {
            Console.WriteLine("=== Ejemplo 7: Flujo Completo ===\n");

            var encService = new EncryptionService("AppSecurityKey");

            // Paso 1: Crear mensaje
            string message = "Mensaje confidencial";
            Console.WriteLine("1. Crear Mensaje");
            Console.WriteLine($"   Texto: {message}\n");

            // Paso 2: Cifrar
            Console.WriteLine("2. Cifrar (AES-256-CBC)");
            byte[] encrypted = encService.Encrypt(message);
            Console.WriteLine($"   Bytes: {BitConverter.ToString(encrypted)}");
            Console.WriteLine($"   Tamaño: {encrypted.Length} bytes\n");

            // Paso 3: Empaquetar
            Console.WriteLine("3. Empaquetar (Protocolo)");
            byte[] packet = MessageProtocol.PackMessage(encrypted);
            Console.WriteLine($"   Bytes: {BitConverter.ToString(packet)}");
            Console.WriteLine($"   Tamaño Total: {packet.Length} bytes\n");

            // Paso 4: "Transmitir" (simular recepción)
            Console.WriteLine("4. Transmitir y Recibir");
            Console.WriteLine($"   [TRANSMISIÓN POR RED]\n");

            // Paso 5: Desempaquetar
            Console.WriteLine("5. Desempaquetar");
            if (MessageProtocol.TryUnpackMessage(packet, packet.Length, 
                out byte[] receivedEncrypted, out int consumed))
            {
                Console.WriteLine($"   Datos Extraídos: {BitConverter.ToString(receivedEncrypted)}");
                Console.WriteLine($"   Bytes Consumidos: {consumed}\n");

                // Paso 6: Descifrar
                Console.WriteLine("6. Descifrar");
                try
                {
                    string decrypted = encService.Decrypt(receivedEncrypted);
                    Console.WriteLine($"   Texto: {decrypted}\n");

                    // Verificación final
                    Console.WriteLine("7. Verificación");
                    if (message == decrypted)
                    {
                        Console.WriteLine("   ✓ ÉXITO - Mensaje íntegro y seguro\n");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"   ✗ Error: {ex.Message}\n");
                }
            }
        }

        // Método principal para ejecutar ejemplos
        public static async Task Main(string[] args)
        {
            Console.WriteLine("╔════════════════════════════════════════════╗");
            Console.WriteLine("║   Ejemplos de Chat P2P Seguro (AES-256)   ║");
            Console.WriteLine("╚════════════════════════════════════════════╝\n");

            Console.WriteLine("Selecciona un ejemplo (1-7) o 'q' para salir:");
            Console.WriteLine("1. Cifrado Básico AES-256");
            Console.WriteLine("2. IV Aleatorio por Mensaje");
            Console.WriteLine("3. Protocolo de Mensajes");
            Console.WriteLine("4. Nodo Servidor P2P");
            Console.WriteLine("5. Nodo Cliente P2P");
            Console.WriteLine("6. Validación de Seguridad");
            Console.WriteLine("7. Flujo Completo");

            while (true)
            {
                Console.Write("\nOpción: ");
                string input = Console.ReadLine()?.ToLower() ?? "q";

                try
                {
                    switch (input)
                    {
                        case "1":
                            Example1_BasicEncryption();
                            break;
                        case "2":
                            Example2_RandomIV();
                            break;
                        case "3":
                            Example3_MessageProtocol();
                            break;
                        case "4":
                            await Example4_P2PNodeServer();
                            break;
                        case "5":
                            await Example5_P2PNodeClient();
                            break;
                        case "6":
                            Example6_SecurityValidation();
                            break;
                        case "7":
                            Example7_CompleteFlow();
                            break;
                        case "q":
                            Console.WriteLine("¡Hasta luego!");
                            return;
                        default:
                            Console.WriteLine("Opción no válida. Intenta nuevamente.");
                            break;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                }
            }
        }
    }
}
