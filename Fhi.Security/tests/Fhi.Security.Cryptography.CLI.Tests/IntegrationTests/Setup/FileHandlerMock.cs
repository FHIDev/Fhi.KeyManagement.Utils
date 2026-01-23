using System.Collections.Concurrent;
using Fhi.Security.Cryptography.CLI.Services;

namespace Fhi.Security.Cryptography.CLI.IntegrationTests.Setup
{
    internal class FileHandlerMock : IFileHandler
    {
        public ConcurrentDictionary<string, string> Files = new();
        public ConcurrentDictionary<string, byte[]> ByteFiles = new();

        public void CreateDirectory(string path)
        {
        }

        public bool PathExists(string path)
        {
            return true;
        }

        public string ReadAllText(string path)
        {
            if (Files.TryGetValue(path, out string? value))
                return value;

            return string.Empty;
        }

        public void WriteAllText(string path, string content)
        {
            Files.TryAdd(path, content);
        }

        public void WriteAllBytes(string path, byte[] content)
        {
            ByteFiles.TryAdd(path, content);
            Files.TryAdd(path, Convert.ToBase64String(content));
        }
    }
}
