using System.Security.Cryptography;

namespace IntimacyAI.Server.Security
{
    public sealed class DerivedKey
    {
        public byte[] Key { get; set; } = Array.Empty<byte>();
        public byte[] Salt { get; set; } = Array.Empty<byte>();
        public int Iterations { get; set; }
        public string Algorithm { get; set; } = "PBKDF2-SHA256";
    }

    public interface IKeyDerivationService
    {
        DerivedKey DeriveKey(string password, byte[]? salt = null);
    }

    public sealed class KeyDerivationService : IKeyDerivationService
    {
        private const int SaltSize = 32;
        private const int KeySize = 32;
        private const int Iterations = 100_000;

        public DerivedKey DeriveKey(string password, byte[]? salt = null)
        {
            salt ??= GenerateRandomSalt();
            using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, Iterations, HashAlgorithmName.SHA256);
            var key = pbkdf2.GetBytes(KeySize);
            return new DerivedKey
            {
                Key = key,
                Salt = salt,
                Iterations = Iterations,
                Algorithm = "PBKDF2-SHA256"
            };
        }

        private static byte[] GenerateRandomSalt()
        {
            var salt = new byte[SaltSize];
            RandomNumberGenerator.Fill(salt);
            return salt;
        }
    }
}

