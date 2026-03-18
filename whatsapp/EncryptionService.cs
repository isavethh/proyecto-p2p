using System.Security.Cryptography;
using System.Text;

namespace whatsapp
{
    /// <summary>
    /// Servicio de cifrado/descifrado AES-256
    /// </summary>
    public class EncryptionService
    {
        private readonly byte[] _key;
        private const int KeySize = 32; // 256 bits
        private const int IVSize = 16; // 128 bits

        public EncryptionService(string sharedKey)
        {
            // Derivar una clave de 256 bits del valor compartido
            using (var sha256 = SHA256.Create())
            {
                _key = sha256.ComputeHash(Encoding.UTF8.GetBytes(sharedKey));
            }
        }

        /// <summary>
        /// Cifra un mensaje usando AES-256
        /// </summary>
        /// <param name="plaintext">Mensaje a cifrar</param>
        /// <returns>Bytes: [IV (16 bytes)][Mensaje Cifrado]</returns>
        public byte[] Encrypt(string plaintext)
        {
            using (var aes = Aes.Create())
            {
                aes.Key = _key;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;
                
                // Generar IV aleatorio
                aes.GenerateIV();
                byte[] iv = aes.IV;

                using (var encryptor = aes.CreateEncryptor(aes.Key, aes.IV))
                {
                    byte[] plainBytes = Encoding.UTF8.GetBytes(plaintext);
                    
                    using (var ms = new MemoryStream())
                    {
                        // Escribir IV primero
                        ms.Write(iv, 0, iv.Length);
                        
                        // Escribir texto cifrado
                        using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                        {
                            cs.Write(plainBytes, 0, plainBytes.Length);
                            cs.FlushFinalBlock();
                        }
                        
                        return ms.ToArray();
                    }
                }
            }
        }

        /// <summary>
        /// Descifra un mensaje usando AES-256
        /// </summary>
        /// <param name="encryptedData">Bytes: [IV (16 bytes)][Mensaje Cifrado]</param>
        /// <returns>Mensaje descifrado</returns>
        public string Decrypt(byte[] encryptedData)
        {
            if (encryptedData == null || encryptedData.Length < IVSize)
                throw new ArgumentException("Datos cifrados inválidos");

            using (var aes = Aes.Create())
            {
                aes.Key = _key;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;

                // Extraer IV de los primeros 16 bytes
                byte[] iv = new byte[IVSize];
                Array.Copy(encryptedData, 0, iv, 0, IVSize);
                aes.IV = iv;

                using (var decryptor = aes.CreateDecryptor(aes.Key, aes.IV))
                {
                    using (var ms = new MemoryStream(encryptedData, IVSize, encryptedData.Length - IVSize))
                    {
                        using (var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                        {
                            using (var reader = new StreamReader(cs, Encoding.UTF8))
                            {
                                return reader.ReadToEnd();
                            }
                        }
                    }
                }
            }
        }
    }
}
