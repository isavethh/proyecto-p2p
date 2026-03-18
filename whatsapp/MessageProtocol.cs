using System.Text;

namespace whatsapp
{
    /// <summary>
    /// Protocolo personalizado para mensajes P2P
    /// Formato: [Longitud (4 bytes)][Datos Cifrados]
    /// </summary>
    public class MessageProtocol
    {
        public const int HeaderSize = 4; // Tamaño del encabezado

        /// <summary>
        /// Empaqueta un mensaje cifrado con su longitud
        /// </summary>
        public static byte[] PackMessage(byte[] encryptedData)
        {
            using (var ms = new MemoryStream())
            {
                // Escribir longitud (4 bytes)
                byte[] lengthBytes = BitConverter.GetBytes(encryptedData.Length);
                ms.Write(lengthBytes, 0, 4);
                
                // Escribir datos cifrados
                ms.Write(encryptedData, 0, encryptedData.Length);
                
                return ms.ToArray();
            }
        }

        /// <summary>
        /// Desempaqueta un mensaje y retorna los datos cifrados
        /// </summary>
        public static bool TryUnpackMessage(byte[] buffer, int bytesRead, out byte[] encryptedData, out int bytesConsumed)
        {
            encryptedData = null;
            bytesConsumed = 0;

            if (bytesRead < HeaderSize)
                return false;

            // Leer longitud
            int messageLength = BitConverter.ToInt32(buffer, 0);
            
            if (messageLength <= 0 || messageLength > 1024 * 1024) // Máximo 1MB
                return false;

            int totalRequired = HeaderSize + messageLength;
            
            if (bytesRead < totalRequired)
                return false;

            // Extraer datos cifrados
            encryptedData = new byte[messageLength];
            Array.Copy(buffer, HeaderSize, encryptedData, 0, messageLength);
            bytesConsumed = totalRequired;

            return true;
        }
    }
}
