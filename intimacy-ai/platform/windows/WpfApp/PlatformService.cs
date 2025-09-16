using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp
{
    public interface IPlatformService
    {
        Task<byte[]> CaptureScreenshotAsync();
        Task<bool> AuthenticateUserAsync();
        Task StoreSecureDataAsync(string key, byte[] data);
        Task<byte[]> RetrieveSecureDataAsync(string key);
    }

    public sealed class WindowsPlatformService : IPlatformService
    {
        private static string SafePath(string key)
        {
            var baseDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "IntimacyAI");
            Directory.CreateDirectory(baseDir);
            var name = Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(key)));
            return Path.Combine(baseDir, name + ".bin");
        }

        public Task<byte[]> CaptureScreenshotAsync()
        {
            // Minimal stub; real implementation would use Windows Graphics Capture APIs
            return Task.FromResult(Array.Empty<byte>());
        }

        public Task<bool> AuthenticateUserAsync()
        {
            // Stub for Windows Hello; always true in demo
            return Task.FromResult(true);
        }

        public async Task StoreSecureDataAsync(string key, byte[] data)
        {
            var path = SafePath(key);
            var protectedBytes = ProtectedData.Protect(data, null, DataProtectionScope.CurrentUser);
            await File.WriteAllBytesAsync(path, protectedBytes);
        }

        public async Task<byte[]> RetrieveSecureDataAsync(string key)
        {
            var path = SafePath(key);
            if (!File.Exists(path)) return Array.Empty<byte>();
            var protectedBytes = await File.ReadAllBytesAsync(path);
            return ProtectedData.Unprotect(protectedBytes, null, DataProtectionScope.CurrentUser);
        }
    }
}

