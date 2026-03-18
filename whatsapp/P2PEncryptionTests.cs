using System;
using System.Collections.Generic;

namespace whatsapp.Tests
{
    /// <summary>
    /// Pruebas unitarias para validar la funcionalidad de la aplicación P2P
    /// </summary>
    public class P2PEncryptionTests
    {
        /// <summary>
        /// Ejecutar todas las pruebas
        /// </summary>
        public static void RunAllTests()
        {
            Console.WriteLine("╔═════════════════════════════════════════╗");
            Console.WriteLine("║   Pruebas Unitarias - Chat P2P Seguro   ║");
            Console.WriteLine("╚═════════════════════════════════════════╝\n");

            var results = new List<(string Name, bool Passed, string Message)>();

            // Ejecutar todas las pruebas
            results.Add(TestEncryptionBasic());
            results.Add(TestDecryptionBasic());
            results.Add(TestRoundTrip());
            results.Add(TestDifferentIV());
            results.Add(TestInvalidKey());
            results.Add(TestEmptyMessage());
            results.Add(TestLongMessage());
            results.Add(TestMessageProtocolPackUnpack());
            results.Add(TestProtocolWithInvalidData());
            results.Add(TestProtocolBufferOverflow());
            results.Add(TestKeyDerivation());
            results.Add(TestSpecialCharacters());

            // Mostrar resultados
            Console.WriteLine("\n╔═════════════════════════════════════════╗");
            Console.WriteLine("║             RESULTADOS                   ║");
            Console.WriteLine("╚═════════════════════════════════════════╝\n");

            int passed = 0;
            int failed = 0;

            foreach (var (name, success, message) in results)
            {
                string status = success ? "✓ PASS" : "✗ FAIL";
                Console.WriteLine($"{status} | {name}");
                if (!success)
                {
                    Console.WriteLine($"       {message}");
                    failed++;
                }
                else
                {
                    passed++;
                }
            }

            Console.WriteLine($"\n╔═════════════════════════════════════════╗");
            Console.WriteLine($"║ Total: {passed + failed} | Exitosas: {passed} | Fallidas: {failed} |");
            Console.WriteLine($"╚═════════════════════════════════════════╝\n");
        }

        // ============== Pruebas de Encriptación ==============

        private static (string, bool, string) TestEncryptionBasic()
        {
            try
            {
                var encService = new EncryptionService("TestKey");
                string message = "Hello World";
                byte[] encrypted = encService.Encrypt(message);

                // Validar que el resultado no sea vacío
                if (encrypted == null || encrypted.Length == 0)
                    return ("EncryptionBasic", false, "Resultado vacío");

                // Validar que sea diferente al original
                if (System.Text.Encoding.UTF8.GetString(encrypted).Contains("Hello"))
                    return ("EncryptionBasic", false, "No fue cifrado correctamente");

                return ("EncryptionBasic", true, "");
            }
            catch (Exception ex)
            {
                return ("EncryptionBasic", false, ex.Message);
            }
        }

        private static (string, bool, string) TestDecryptionBasic()
        {
            try
            {
                var encService = new EncryptionService("TestKey");
                string original = "Secret Message";
                byte[] encrypted = encService.Encrypt(original);
                string decrypted = encService.Decrypt(encrypted);

                if (decrypted != original)
                    return ("DecryptionBasic", false, $"Esperado: '{original}', Obtenido: '{decrypted}'");

                return ("DecryptionBasic", true, "");
            }
            catch (Exception ex)
            {
                return ("DecryptionBasic", false, ex.Message);
            }
        }

        private static (string, bool, string) TestRoundTrip()
        {
            try
            {
                var encService = new EncryptionService("RoundTripKey");
                var messages = new[]
                {
                    "Mensaje 1",
                    "Mensaje con números 12345",
                    "Caracteres especiales: !@#$%^&*()",
                    "Saltos\nde\nlínea",
                    "Tabulaciones\t\tseparadas"
                };

                foreach (var message in messages)
                {
                    byte[] encrypted = encService.Encrypt(message);
                    string decrypted = encService.Decrypt(encrypted);

                    if (decrypted != message)
                        return ("RoundTrip", false, 
                            $"Fallo con: '{message}' -> '{decrypted}'");
                }

                return ("RoundTrip", true, "");
            }
            catch (Exception ex)
            {
                return ("RoundTrip", false, ex.Message);
            }
        }

        private static (string, bool, string) TestDifferentIV()
        {
            try
            {
                var encService = new EncryptionService("IVTestKey");
                string message = "Mismo Mensaje";

                // Cifrar dos veces el mismo mensaje
                byte[] encrypted1 = encService.Encrypt(message);
                byte[] encrypted2 = encService.Encrypt(message);

                // Los resultados deben ser diferentes (por IV aleatorio)
                if (BitConverter.ToString(encrypted1) == BitConverter.ToString(encrypted2))
                    return ("DifferentIV", false, "Los cifrados son idénticos (IV no es aleatorio)");

                // Pero ambos deben descifrar correctamente
                string decrypted1 = encService.Decrypt(encrypted1);
                string decrypted2 = encService.Decrypt(encrypted2);

                if (decrypted1 != message || decrypted2 != message)
                    return ("DifferentIV", false, "Fallo al descifrar con IV diferente");

                return ("DifferentIV", true, "");
            }
            catch (Exception ex)
            {
                return ("DifferentIV", false, ex.Message);
            }
        }

        private static (string, bool, string) TestInvalidKey()
        {
            try
            {
                var correctKey = new EncryptionService("CorrectKey");
                var wrongKey = new EncryptionService("WrongKey");

                string message = "Protected Message";
                byte[] encrypted = correctKey.Encrypt(message);

                try
                {
                    string decrypted = wrongKey.Decrypt(encrypted);
                    // Si llega aquí, la seguridad está comprometida
                    return ("InvalidKey", false, "Pudo descifrar con clave incorrecta");
                }
                catch (Exception)
                {
                    // Esperado: debe fallar con clave incorrecta
                    return ("InvalidKey", true, "");
                }
            }
            catch (Exception ex)
            {
                return ("InvalidKey", false, ex.Message);
            }
        }

        private static (string, bool, string) TestEmptyMessage()
        {
            try
            {
                var encService = new EncryptionService("EmptyTestKey");
                string message = "";

                byte[] encrypted = encService.Encrypt(message);
                string decrypted = encService.Decrypt(encrypted);

                if (decrypted != message)
                    return ("EmptyMessage", false, "No pudo cifrar/descifrar mensaje vacío");

                return ("EmptyMessage", true, "");
            }
            catch (Exception ex)
            {
                return ("EmptyMessage", false, ex.Message);
            }
        }

        private static (string, bool, string) TestLongMessage()
        {
            try
            {
                var encService = new EncryptionService("LongMessageKey");
                string message = new string('A', 100000); // 100 KB

                byte[] encrypted = encService.Encrypt(message);
                string decrypted = encService.Decrypt(encrypted);

                if (decrypted != message)
                    return ("LongMessage", false, "Fallo con mensaje largo");

                return ("LongMessage", true, "");
            }
            catch (Exception ex)
            {
                return ("LongMessage", false, ex.Message);
            }
        }

        // ============== Pruebas de Protocolo ==============

        private static (string, bool, string) TestMessageProtocolPackUnpack()
        {
            try
            {
                byte[] originalData = new byte[] { 0x01, 0x02, 0x03, 0x04, 0x05 };
                
                // Empaquetar
                byte[] packed = MessageProtocol.PackMessage(originalData);

                // Desempaquetar
                if (!MessageProtocol.TryUnpackMessage(packed, packed.Length, 
                    out byte[] unpacked, out int consumed))
                {
                    return ("ProtocolPackUnpack", false, "No pudo desempaquetar");
                }

                // Validar
                if (BitConverter.ToString(originalData) != BitConverter.ToString(unpacked))
                    return ("ProtocolPackUnpack", false, "Datos no coinciden");

                if (consumed != packed.Length)
                    return ("ProtocolPackUnpack", false, "Bytes consumidos incorrectos");

                return ("ProtocolPackUnpack", true, "");
            }
            catch (Exception ex)
            {
                return ("ProtocolPackUnpack", false, ex.Message);
            }
        }

        private static (string, bool, string) TestProtocolWithInvalidData()
        {
            try
            {
                byte[] invalidData = new byte[] { 0xFF, 0xFF };

                // No debe desempaquetar datos con longitud insuficiente
                if (MessageProtocol.TryUnpackMessage(invalidData, invalidData.Length, 
                    out _, out _))
                {
                    return ("ProtocolInvalidData", false, "Aceptó datos inválidos");
                }

                return ("ProtocolInvalidData", true, "");
            }
            catch (Exception ex)
            {
                return ("ProtocolInvalidData", false, ex.Message);
            }
        }

        private static (string, bool, string) TestProtocolBufferOverflow()
        {
            try
            {
                // Crear un mensaje que requiere más bytes de los que tenemos
                byte[] lengthBytes = BitConverter.GetBytes(1000);
                byte[] incompletePacked = new byte[10]; // Solo 10 bytes
                Array.Copy(lengthBytes, incompletePacked, 4);

                // No debe desempaquetar buffer incompleto
                if (MessageProtocol.TryUnpackMessage(incompletePacked, incompletePacked.Length, 
                    out _, out _))
                {
                    return ("ProtocolBufferOverflow", false, "Aceptó buffer incompleto");
                }

                return ("ProtocolBufferOverflow", true, "");
            }
            catch (Exception ex)
            {
                return ("ProtocolBufferOverflow", false, ex.Message);
            }
        }

        // ============== Pruebas de Derivación de Claves ==============

        private static (string, bool, string) TestKeyDerivation()
        {
            try
            {
                // Misma clave compartida debe derivar misma clave interna
                var enc1 = new EncryptionService("SharedKey");
                var enc2 = new EncryptionService("SharedKey");

                string message = "Key Derivation Test";
                byte[] encrypted = enc1.Encrypt(message);

                // Ambas instancias deben poder descifrar
                string decrypted2 = enc2.Decrypt(encrypted);

                if (decrypted2 != message)
                    return ("KeyDerivation", false, "Keys no derivan correctamente");

                return ("KeyDerivation", true, "");
            }
            catch (Exception ex)
            {
                return ("KeyDerivation", false, ex.Message);
            }
        }

        // ============== Pruebas de Caracteres Especiales ==============

        private static (string, bool, string) TestSpecialCharacters()
        {
            try
            {
                var encService = new EncryptionService("SpecialCharKey");
                
                var specialMessages = new[]
                {
                    "Español: áéíóú ñ ü",
                    "Chino: 中文消息",
                    "Árabe: رسالة",
                    "Emoji: 😀😃😄",
                    "Unicode: ℃€£¥",
                    "Símbolos: ©®™§¶†"
                };

                foreach (var message in specialMessages)
                {
                    byte[] encrypted = encService.Encrypt(message);
                    string decrypted = encService.Decrypt(encrypted);

                    if (decrypted != message)
                        return ("SpecialCharacters", false, 
                            $"Fallo con: {message}");
                }

                return ("SpecialCharacters", true, "");
            }
            catch (Exception ex)
            {
                return ("SpecialCharacters", false, ex.Message);
            }
        }

            }
        }
