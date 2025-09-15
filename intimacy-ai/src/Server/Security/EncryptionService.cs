using System.Security.Cryptography;
using System.Text;

namespace IntimacyAI.Server.Security
{
    public interface IEncryptionService
    {
        string Encrypt(string plaintext);
        string Decrypt(string ciphertext);
    }

    public sealed class EncryptionService : IEncryptionService
    {
        private readonly byte[] _key;

        public EncryptionService(IConfiguration configuration)
        {
            // Base64 key expected (32 bytes for AES-256)
            var base64 = configuration["Security:EncryptionKey"] ?? string.Empty;
            _key = string.IsNullOrWhiteSpace(base64) ? RandomNumberGenerator.GetBytes(32) : Convert.FromBase64String(base64);
        }

        public string Encrypt(string plaintext)
        {
            using var aes = new AesGcm(_key, 16);
            var nonce = RandomNumberGenerator.GetBytes(12);
            var plainBytes = Encoding.UTF8.GetBytes(plaintext);
            var cipher = new byte[plainBytes.Length];
            var tag = new byte[16];
            aes.Encrypt(nonce, plainBytes, cipher, tag);
            var result = new byte[nonce.Length + tag.Length + cipher.Length];
            Buffer.BlockCopy(nonce, 0, result, 0, nonce.Length);
            Buffer.BlockCopy(tag, 0, result, nonce.Length, tag.Length);
            Buffer.BlockCopy(cipher, 0, result, nonce.Length + tag.Length, cipher.Length);
            return Convert.ToBase64String(result);
        }

        public string Decrypt(string ciphertext)
        {
            var data = Convert.FromBase64String(ciphertext);
            var nonce = data.AsSpan(0, 12).ToArray();
            var tag = data.AsSpan(12, 16).ToArray();
            var cipher = data.AsSpan(28).ToArray();
            using var aes = new AesGcm(_key, 16);
            var plain = new byte[cipher.Length];
            aes.Decrypt(nonce, cipher, tag, plain);
            return Encoding.UTF8.GetString(plain);
        }
    }
}

